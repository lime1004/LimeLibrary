#if LIME_R3 && LIME_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Utility;
using UnityEngine;

namespace LimeLibrary.Event.Events {

public abstract class ScriptableEvent : ScriptableObject, IScriptableEvent {
  private bool _isExecuting;

  protected bool IsExecuting => _isExecuting;

  protected internal virtual void SetContext(IScriptableEventContext context) {
    Assertion.Assert(!_isExecuting, "ScriptableEvent is already executing.");
  }

  protected internal virtual void ClearContext() { }

  void IScriptableEvent.SetContext(IScriptableEventContext c) => SetContext(c);
  bool IScriptableEvent.IsExecuting => _isExecuting;

  bool IScriptableEvent.TryBeginExecution() {
    Assertion.Assert(!_isExecuting, "ScriptableEvent is already executing.");
    if (_isExecuting) return false;

    _isExecuting = true;
    return true;
  }

  void IScriptableEvent.EndExecution() {
    _isExecuting = false;
    ClearContext();
  }

  public abstract UniTask Initialize(CancellationToken cancellationToken);
  public abstract UniTask Execute(CancellationToken cancellationToken);
}

public abstract class ScriptableEvent<T> : ScriptableEvent where T : class, IScriptableEventContext {
  protected T Context { get; private set; }
  protected internal override void SetContext(IScriptableEventContext c) {
    base.SetContext(c);
    if (IsExecuting) return;

    Context = (T)c;
  }

  protected internal override void ClearContext() => Context = null;
}

}
#endif