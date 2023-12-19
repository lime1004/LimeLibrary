#if LIME_UNIRX && LIME_UNITASK
using System;
using Cysharp.Threading.Tasks;
using LimeLibrary.Module;
using UniRx;

namespace LimeLibrary.Event {

/// <summary>
/// Eventシステムアクセス用クラス
/// </summary>
public class EventUpdaterInterface : SingletonMonoBehaviour<EventUpdaterInterface> {
  private EventUpdaterState _state = EventUpdaterState.Idle;

  private readonly Subject<(EventBehaviourType, Event)> _onRequestEventSubject = new();
  internal IObservable<(EventBehaviourType behaviourType, Event @event)> OnRequestEventObservable => _onRequestEventSubject;

  internal Func<Type, bool> IsRunningFunc { get; set; }
  internal Func<Event> GetRunningEventFunc { get; set; }

  internal void SetState(EventUpdaterState state) {
    _state = state;
  }

  /// <summary>
  /// Eventのリクエスト
  /// </summary>
  public T RequestGameEvent<T>(EventBehaviourType behaviourType) where T : Event, new() {
    var @event = new T();
    @event.SetCancellationToken(this.GetCancellationTokenOnDestroy());
    _onRequestEventSubject.OnNext((behaviourType, @event));
    return @event;
  }

  /// <summary>
  /// Eventが走っているかどうか
  /// </summary>
  public bool IsRunning() {
    return _state != EventUpdaterState.Idle;
  }

  /// <summary>
  /// EventUpdaterの状態を取得
  /// </summary>
  public EventUpdaterState GetState() {
    return _state;
  }

  /// <summary>
  /// 特定のEventが走っているかどうか
  /// </summary>
  public bool IsRunning<T>() where T : Event {
    return IsRunningFunc?.Invoke(typeof(T)) ?? false;
  }

  /// <summary>
  /// 実行中のEventを取得
  /// </summary>
  public Event GetRunningEvent() {
    return GetRunningEventFunc?.Invoke();
  }
}

}
#endif