#if LIME_R3 && LIME_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;

namespace LimeLibrary.Event.Events {

public interface IScriptableEventContextCreator {
  public UniTask<IScriptableEventContext> Create(CancellationToken cancellationToken);
}

}
#endif