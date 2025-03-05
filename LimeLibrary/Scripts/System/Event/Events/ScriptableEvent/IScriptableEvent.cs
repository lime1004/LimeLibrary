using System.Threading;
using Cysharp.Threading.Tasks;

namespace LimeLibrary.Event.Events {

internal interface IScriptableEvent {
  public UniTask Initialize(CancellationToken cancellationToken);
  public UniTask Execute(CancellationToken cancellationToken);
}

}