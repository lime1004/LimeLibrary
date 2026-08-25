#if LIME_R3 && LIME_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;

namespace LimeLibrary.Event.Events {

public class ScriptableEventPlayer : IScriptableEventPlayer<IScriptableEvent> {
  public async UniTask Play(IScriptableEvent scriptableEvent, CancellationToken cancellationToken) {
    await scriptableEvent.Execute(cancellationToken);
  }
}

}
#endif