#if LIME_UNIRX && LIME_UNITASK
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.Utility;
using UnityEngine;
using UniRx;

namespace LimeLibrary.Event {

[DefaultExecutionOrder(5)]
[RequireComponent(typeof(EventUpdaterInterface))]
internal class EventUpdater : MonoBehaviour {
  private class EventData {
    public EventData(Event @event, EventBehaviourType behaviourType) {
      Event = @event;
      BehaviourType = behaviourType;
    }

    public Event Event { get; }
    public EventBehaviourType BehaviourType { get; }
    public EventUpdaterState State { get; set; } = EventUpdaterState.Idle;
    public UniTask InitializeTask { get; set; }
  }

  private readonly Stack<EventData> _interruptGameEventStack = new();
  private readonly Queue<EventData> _gameEventQueue = new();
  private EventUpdaterInterface _interface;
  private EventData _prevUpdateGameEventData;

  private void Awake() {
    _interface = GetComponent<EventUpdaterInterface>();

    // Eventリクエスト時処理
    _interface.OnRequestEventObservable.Subscribe(tuple => {
      AddEvent(tuple.@event, tuple.behaviourType);
    }).AddTo(this);

    // Eventが走っているかどうか
    _interface.IsRunningFunc = type => {
      var eventData = GetExecuteEventData();
      if (eventData == null) return false;
      if (eventData.State == EventUpdaterState.Idle) return false;
      if (eventData.Event.GetType() != type) return false;
      return true;
    };

    // 実行中のEventを取得
    _interface.GetRunningEventFunc = () => {
      var eventData = GetExecuteEventData();
      return eventData?.Event;
    };
  }

  private void AddEvent(Event @event, EventBehaviourType behaviourType) {
    var eventData = new EventData(@event, behaviourType);
    switch (behaviourType) {
    case EventBehaviourType.Order:
      // 順次実行
      _gameEventQueue.Enqueue(eventData);
      break;
    case EventBehaviourType.Interrupt:
      // 割り込み
      _interruptGameEventStack.Push(eventData);
      break;
    default:
      Assertion.Assert(false, "BehaviourType not supported. " + behaviourType);
      break;
    }
  }

  private void Update() {
    bool isLoop = false;
    do {
      isLoop = false;

      var executeEventData = GetExecuteEventData();

      switch (_interface.GetState()) {
      case EventUpdaterState.Idle: {
        if (executeEventData != null) {
          _interface.SetState(EventUpdaterState.Initialize);
          isLoop = true;
        }
        break;
      }

      case EventUpdaterState.Initialize: {
        if (executeEventData == null) {
          Assertion.Assert(false);
          break;
        }
        executeEventData.InitializeTask = executeEventData.Event.InitializeAsync(gameObject.GetCancellationTokenOnDestroy()).RunHandlingError();
        _interface.SetState(EventUpdaterState.InitializeWait);
        executeEventData.State = EventUpdaterState.InitializeWait;
        break;
      }

      case EventUpdaterState.InitializeWait: {
        if (executeEventData == null) {
          Assertion.Assert(false);
          break;
        }
        if (executeEventData.InitializeTask.GetAwaiter().IsCompleted) {
          executeEventData.Event.Start();
          _interface.SetState(EventUpdaterState.Update);
          executeEventData.State = EventUpdaterState.Update;
        }
        break;
      }

      case EventUpdaterState.Update: {
        if (executeEventData == null) {
          Assertion.Assert(false);
          break;
        }

        if (_prevUpdateGameEventData != null && _prevUpdateGameEventData != executeEventData) {
          // 割り込みが入ったのでStartから再開
          _interface.SetState(EventUpdaterState.Initialize);
          _prevUpdateGameEventData = null;
          isLoop = true;
          break;
        }

        var updateResult = executeEventData.Event.Update();
        _prevUpdateGameEventData = executeEventData;
        switch (updateResult) {
        case EventUpdateResult.Continue:
          isLoop = false;
          break;
        case EventUpdateResult.ContinueDirect:
          isLoop = true;
          break;
        case EventUpdateResult.Finish:
          _interface.SetState(EventUpdaterState.End);
          executeEventData.State = EventUpdaterState.End;
          break;
        default:
          Assertion.Assert(false);
          break;
        }

        break;
      }

      case EventUpdaterState.End: {
        // 削除
        switch (executeEventData.BehaviourType) {
        case EventBehaviourType.Interrupt:
          _interruptGameEventStack.Pop();
          break;
        case EventBehaviourType.Order:
          _gameEventQueue.Dequeue();
          break;
        default:
          Assertion.Assert(false);
          break;
        }

        executeEventData.Event.End();
        _prevUpdateGameEventData = null;

        var nextGameEventData = GetExecuteEventData();
        _interface.SetState(nextGameEventData?.State ?? EventUpdaterState.Idle);
        break;
      }

      default:
        Assertion.Assert(false);
        break;
      }
    } while (isLoop);
  }

  private EventData GetExecuteEventData() {
    // 割り込みが優先
    if (_interruptGameEventStack.Count > 0) {
      return _interruptGameEventStack.Peek();
    }
    if (_gameEventQueue.Count > 0) {
      return _gameEventQueue.Peek();
    }
    return null;
  }
}

}
#endif