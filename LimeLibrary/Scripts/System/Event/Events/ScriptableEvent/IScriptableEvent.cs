using System.Threading;
using Cysharp.Threading.Tasks;

namespace LimeLibrary.Event.Events {

public interface IScriptableEvent {
  public UniTask Initialize(CancellationToken cancellationToken);
  public UniTask Execute(CancellationToken cancellationToken);
}

}