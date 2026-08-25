#if LIME_R3 && LIME_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LimeLibrary.Event.Events {

public abstract class ScriptableEvent : ScriptableObject, IScriptableEvent {
  protected internal virtual void SetContext(IScriptableEventContext context) { }
  void IScriptableEvent.SetContext(IScriptableEventContext c) => SetContext(c);

  public abstract UniTask Initialize(CancellationToken cancellationToken);
  public abstract UniTask Execute(CancellationToken cancellationToken);
}

public abstract class ScriptableEvent<T> : ScriptableEvent where T : IScriptableEventContext {
  protected T Context { get; private set; }
  protected internal override void SetContext(IScriptableEventContext c) => Context = (T)c;
}

}
#endif