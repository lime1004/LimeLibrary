#if LIME_UNIRX && LIME_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Event.Core;
using LimeLibrary.Resource;

namespace LimeLibrary.Event.Events {

public class ExecuteScriptableEvent : AbstractEvent {
  private DynamicResource<IScriptableEvent> _scriptableEventResource;
  private UniTask _executeTask;

  public string EventAddress { get; set; }

  public override async UniTask InitializeAsync(CancellationToken cancellationToken) {
    _scriptableEventResource = await ResourceLoader.LoadAsync<IScriptableEvent>(EventAddress, cancellationToken);
    await _scriptableEventResource.Resource.Initialize(cancellationToken);
    await base.InitializeAsync(cancellationToken);
  }

  public override void Start() {
    base.Start();

    _executeTask = _scriptableEventResource.Resource.Execute(CancellationToken);
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