#if LIME_R3 && LIME_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LimeLibrary.Event.Events {

public abstract class ScriptableEvent : ScriptableObject, IScriptableEvent {
  private protected virtual void SetContext(IScriptableEventContext context) { }
  private protected virtual void ClearContext() { }

  void IScriptableEvent.SetContext(IScriptableEventContext c) => SetContext(c);

  async UniTask IScriptableEvent.EndExecution(CancellationToken cancellationToken) {
    try {
      await OnEnd(cancellationToken);
    } finally {
      ClearContext();
    }
  }

  public abstract UniTask Initialize(CancellationToken cancellationToken);
  public abstract UniTask Execute(CancellationToken cancellationToken);
  protected virtual UniTask OnEnd(CancellationToken cancellationToken) => UniTask.CompletedTask;
}

public abstract class ScriptableEvent<T> : ScriptableEvent where T : class, IScriptableEventContext {
  protected T Context { get; private set; }

  private protected override void SetContext(IScriptableEventContext c) {
    base.SetContext(c);
    Context = (T)c;
  }

  private protected override void ClearContext() => Context = null;
}

}
#endif