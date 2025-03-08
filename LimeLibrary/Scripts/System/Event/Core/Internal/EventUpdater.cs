#if LIME_R3 && LIME_UNITASK
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.Utility;
using R3;
using UnityEngine;

namespace LimeLibrary.Event.Core.Internal {

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(EventUpdaterInterface))]
internal class EventUpdater : MonoBehaviour {
  private class EventData {
    public EventData(IEvent @event, EventBehaviourType behaviourType) {
      Event = @event;
      BehaviourType = behaviourType;
    }

    public IEvent Event { get; }
    public EventBehaviourType BehaviourType { get; }
    public EventUpdaterState State { get; set; } = EventUpdaterState.Idle;
    public UniTask InitializeTask { get; set; }
  }

  private readonly Stack<EventData> _interruptEventStack = new();
  private readonly Queue<EventData> _eventQueue = new();
  private EventUpdaterInterface _interface;
  private EventData _prevUpdateEventData;

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
      return eventData?.Event as AbstractEvent;
    };
  }

  private void AddEvent(IEvent @event, EventBehaviourType behaviourType) {
    var eventData = new EventData(@event, behaviourType);
    switch (behaviourType) {
    case EventBehaviourType.Order:
      // 順次実行
      _eventQueue.Enqueue(eventData);
      break;
    case EventBehaviourType.Interrupt:
      // 割り込み
      _interruptEventStack.Push(eventData);
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

        if (_prevUpdateEventData != null && _prevUpdateEventData != executeEventData) {
          // 割り込みが入ったのでStartから再開
          _interface.SetState(EventUpdaterState.Initialize);
          _prevUpdateEventData = null;
          isLoop = true;
          break;
        }

        var updateResult = executeEventData.Event.Update();
        _prevUpdateEventData = executeEventData;
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
          _interruptEventStack.Pop();
          break;
        case EventBehaviourType.Order:
          _eventQueue.Dequeue();
          break;
        default:
          Assertion.Assert(false);
          break;
        }

        executeEventData.Event.End();
        _prevUpdateEventData = null;

        var nextEventData = GetExecuteEventData();
        _interface.SetState(nextEventData?.State ?? EventUpdaterState.Idle);
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
    if (_interruptEventStack.Count > 0) {
      return _interruptEventStack.Peek();
    }
    if (_eventQueue.Count > 0) {
      return _eventQueue.Peek();
    }
    return null;
  }
}

}
#endif