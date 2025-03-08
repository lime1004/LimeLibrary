#if LIME_R3 && LIME_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LimeLibrary.Event.Events {

public abstract class ScriptableEvent<T> : ScriptableObject, IScriptableEvent where T : IScriptableEventContext {
  protected T Context { get; private set; }

  public async UniTask Initialize(CancellationToken cancellationToken) {
    Context = await CreateContext(cancellationToken);
  }

  protected abstract UniTask<T> CreateContext(CancellationToken cancellationToken);
  public abstract UniTask Execute(CancellationToken cancellationToken);
}

}
#endif