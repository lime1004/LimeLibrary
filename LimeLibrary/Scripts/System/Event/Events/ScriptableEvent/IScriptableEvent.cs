#if LIME_R3 && LIME_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;

namespace LimeLibrary.Event.Events {

public interface IScriptableEvent {
  internal void SetContext(IScriptableEventContext context);
  internal UniTask EndExecution(CancellationToken cancellationToken);
  public UniTask Initialize(CancellationToken cancellationToken);
  public UniTask Execute(CancellationToken cancellationToken);
}

}
#endif