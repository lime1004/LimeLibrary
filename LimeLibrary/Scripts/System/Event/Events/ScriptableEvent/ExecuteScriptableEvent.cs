#if LIME_R3 && LIME_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Event.Core;
using LimeLibrary.Extensions;
using LimeLibrary.Resource;
using LimeLibrary.Utility;
using UnityEngine;

namespace LimeLibrary.Event.Events {

public class ExecuteScriptableEvent<T> : AbstractEvent where T : class, IScriptableEvent {
  private DynamicResource<T> _scriptableEventResource;
  private T _scriptableEvent;
  private UniTask _executeTask;

  private readonly string _eventAddress;
  private readonly IScriptableEventPlayer<T> _eventPlayer;
  private readonly IScriptableEventContextCreator _contextCreator;

  public ExecuteScriptableEvent(string eventAddress, IScriptableEventPlayer<T> player = null, IScriptableEventContextCreator contextCreator = null) {
    _eventAddress = eventAddress;
    _eventPlayer = player ?? new ScriptableEventPlayer();
    _contextCreator = contextCreator;
  }

  public override async UniTask InitializeAsync(CancellationToken cancellationToken) {
    try {
      _scriptableEventResource = await ResourceLoader.LoadAsync<T>(_eventAddress, cancellationToken);
      if (!_scriptableEventResource.HasResource()) return;
      _scriptableEvent = Object.Instantiate(_scriptableEventResource.Resource as Object) as T;

      if (_contextCreator != null) {
        var context = await _contextCreator.Create(cancellationToken);
        _scriptableEvent.SetContext(context);
      }

      await _scriptableEvent.Initialize(cancellationToken);

      await base.InitializeAsync(cancellationToken);
    } catch {
      ReleaseScriptableEvent();
      throw;
    }
  }

  public override void Start() {
    base.Start();

    if (_scriptableEvent == null) {
      _executeTask = UniTask.CompletedTask;
      return;
    }

    if (_eventPlayer == null) {
      Assertion.Assert(false, "EventPlayer is not set.");
      _executeTask = UniTask.CompletedTask;
      return;
    }

    _executeTask = _eventPlayer.Play(_scriptableEvent, CancellationToken).RunHandlingError();
  }

  public override EventUpdateResult Update() {
    if (_executeTask.GetAwaiter().IsCompleted) {
      return EventUpdateResult.Finish;
    }

    return EventUpdateResult.Continue;
  }

  public override void End() {
    try {
      base.End();
    } finally {
      ReleaseScriptableEvent();
    }
  }

  private void ReleaseScriptableEvent() {
    var scriptableEvent = _scriptableEvent;
    var resource = _scriptableEventResource;
    _scriptableEvent = null;
    _scriptableEventResource = null;

    try {
      scriptableEvent?.EndExecution();
    } finally {
      if (scriptableEvent is Object unityObject) {
        Object.Destroy(unityObject);
      }
      resource?.Dispose();
    }
  }
}

}
#endif