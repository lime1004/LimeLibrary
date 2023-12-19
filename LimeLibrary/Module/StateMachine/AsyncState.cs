#if LIME_UNITASK
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using LimeLibrary.Extensions;

namespace LimeLibrary.Module {

public abstract class AsyncState<TStateId, TContext, TEventType> : IState<TStateId, TContext, TEventType> where TStateId : Enum {
  private OptionalEnum<TStateId> _nextState;
  private CancellationTokenSource _cancellationTokenSource;
  private Channel<AsyncUnit> _updateChannel;

  public virtual void Awake(TContext context) { }

  public virtual void Enter(TContext context, TStateId fromStateId) {
    _cancellationTokenSource = new CancellationTokenSource();
    _nextState = OptionalEnum<TStateId>.None;

    _updateChannel = Channel.CreateSingleConsumerUnbounded<AsyncUnit>();
    var asyncEnumerable = _updateChannel.Reader.ReadAllAsync().Publish();
    asyncEnumerable.Connect().AddTo(_cancellationTokenSource.Token);

    UniTask.Void(async cancellationToken => {
      _nextState = await UpdateAsync(context, asyncEnumerable, cancellationToken).RunHandlingError();
    }, _cancellationTokenSource.Token);
  }

  public abstract void ReceiveEvent(TEventType eventType);

  public OptionalEnum<TStateId> Update(TContext context) {
    _updateChannel.Writer.TryWrite(AsyncUnit.Default);
    return _nextState;
  }

  public abstract UniTask<OptionalEnum<TStateId>> UpdateAsync(TContext context, IUniTaskAsyncEnumerable<AsyncUnit> onUpdate, CancellationToken cancellationToken);

  public abstract void FixedUpdate(TContext context);

  public virtual void Exit(TContext context) {
    _updateChannel.Writer.TryComplete();
    _cancellationTokenSource.Cancel();
    _cancellationTokenSource.Dispose();
  }
}

}
#endif