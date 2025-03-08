#if LIME_R3 && LIME_UNITASK
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Event.Core.Internal;
using R3;

namespace LimeLibrary.Event.Core {

public abstract class AbstractEvent : IEvent {
  private readonly Subject<Unit> _onStartSubject = new();
  private readonly Subject<Unit> _onEndSubject = new();
  private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

  protected CancellationToken CancellationToken { get; private set; }

  public Observable<Unit> OnStartObservable => _onStartSubject;
  public Observable<Unit> OnEndObservable => _onEndSubject;
  public UniTask FinishAsync(CancellationToken cancellationToken) => OnEndObservable.FirstAsync(cancellationToken).AsUniTask();

  private int _sequence;

  protected void AddSeq() {
    _sequence++;
  }

  protected void SetSeq<T>(T seq) where T : Enum {
    _sequence = (int) (object) seq;
  }

  protected T GetSeq<T>() where T : Enum {
    return (T) Enum.ToObject(typeof(T), _sequence);
  }

  public virtual UniTask InitializeAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;

  public virtual void Start() {
    _onStartSubject.OnNext(Unit.Default);
    _onStartSubject.OnCompleted();
  }

  public abstract EventUpdateResult Update();

  public virtual void End() {
    _onEndSubject.OnNext(Unit.Default);
    _onEndSubject.OnCompleted();
  }

  internal void SetCancellationToken(CancellationToken cancellationToken) {
    CancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token).Token;
  }

  public void Cancel() {
    _cancellationTokenSource.Cancel();
  }
}

}
#endif