#if LIME_UNIRX && LIME_UNITASK
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;

namespace LimeLibrary.Event {

public abstract class Event {
  private readonly Subject<Unit> _onStartSubject = new();
  private readonly Subject<Unit> _onEndSubject = new();
  private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

  protected CancellationToken CancellationToken { get; private set; }

  public IObservable<Unit> OnStartObservable => _onStartSubject;
  public IObservable<Unit> OnEndObservable => _onEndSubject;
  public UniTask FinishAsync(CancellationToken cancellationToken) => OnEndObservable.ToUniTask(cancellationToken: cancellationToken);

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

  internal virtual UniTask InitializeAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;

  internal virtual void Start() {
    _onStartSubject.OnNext(Unit.Default);
    _onStartSubject.OnCompleted();
  }

  internal abstract EventUpdateResult Update();

  internal virtual void End() {
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