#if LIME_R3 && LIME_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;

namespace LimeLibrary.Event.Events {

public interface IScriptableEventPlayer<in T> where T : IScriptableEvent {
  public UniTask Play(T scriptableEvent, CancellationToken cancellationToken);
}

}
#endif