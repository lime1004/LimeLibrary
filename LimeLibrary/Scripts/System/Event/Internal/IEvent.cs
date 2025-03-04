using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;

#if LIME_UNIRX && LIME_UNITASK
namespace LimeLibrary.Event.Internal {

internal interface IEvent {
  public IObservable<Unit> OnStartObservable { get; }
  public IObservable<Unit> OnEndObservable { get; }
  public UniTask FinishAsync(CancellationToken cancellationToken) => OnEndObservable.ToUniTask(cancellationToken: cancellationToken);

  public UniTask InitializeAsync(CancellationToken cancellationToken);
  public void Start();
  public EventUpdateResult Update();
  public void End();
  public void Cancel();
}

}
#endif