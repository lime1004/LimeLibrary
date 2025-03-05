#if LIME_UNIRX && LIME_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LimeLibrary.Event.Events {

public abstract class ScriptableEvent<T> : ScriptableObject where T : IScriptableEventContext {
  [SerializeField]
  private string _textAddress;

  protected T Context { get; private set; }

  protected abstract UniTask<T> CreateContext(CancellationToken cancellationToken);

  internal async UniTask Initialize(CancellationToken cancellationToken) {
    Context = await CreateContext(cancellationToken);
  }

  internal abstract UniTask Execute(CancellationToken cancellationToken);
}

}
#endif