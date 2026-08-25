#if LIME_R3 && LIME_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Event.Core;
using LimeLibrary.Extensions;
using LimeLibrary.Resource;
using LimeLibrary.Utility;

namespace LimeLibrary.Event.Events {

public class ExecuteScriptableEvent<T> : AbstractEvent where T : class, IScriptableEvent {
  private DynamicResource<T> _scriptableEventResource;
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
    _scriptableEventResource = await ResourceLoader.LoadAsync<T>(_eventAddress, cancellationToken);
    var scriptableEvent = _scriptableEventResource.Resource;

    if (_contextCreator != null) {
      var context = await _contextCreator.Create(cancellationToken);
      scriptableEvent.SetContext(context);
    }

    await scriptableEvent.Initialize(cancellationToken);

    await base.InitializeAsync(cancellationToken);
  }

  public override void Start() {
    base.Start();

    if (_eventPlayer == null) {
      Assertion.Assert(false, "EventPlayer is not set.");
      _executeTask = UniTask.CompletedTask;
      return;
    }

    _executeTask = _eventPlayer.Play(_scriptableEventResource.Resource, CancellationToken).RunHandlingError();
  }

  public override EventUpdateResult Update() {
    if (_executeTask.GetAwaiter().IsCompleted) {
      return EventUpdateResult.Finish;
    }

    return EventUpdateResult.Continue;
  }

  public override void End() {
    base.End();

    _scriptableEventResource.Dispose();
  }
}

}
#endif