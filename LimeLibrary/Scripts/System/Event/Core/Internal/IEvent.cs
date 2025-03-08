using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

#if LIME_R3 && LIME_UNITASK
namespace LimeLibrary.Event.Core.Internal {

internal interface IEvent {
  public Observable<Unit> OnStartObservable { get; }
  public Observable<Unit> OnEndObservable { get; }
  public UniTask FinishAsync(CancellationToken cancellationToken) => OnEndObservable.FirstAsync(cancellationToken).AsUniTask();

  public UniTask InitializeAsync(CancellationToken cancellationToken);
  public void Start();
  public EventUpdateResult Update();
  public void End();
  public void Cancel();
}

}
#endif