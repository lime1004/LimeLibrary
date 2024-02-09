using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.UI.App;
using LimeLibrary.Utility;

namespace LimeLibrary.UI {

public abstract class UIAppFlow<TState, TContext> where TState : Enum where TContext : UIFlowContext, new() {
  private readonly Dictionary<TState, UIFlowState<TState, TContext>> _dictionary = new();
  private readonly UIApp _uiApp;
  private readonly CancellationTokenSource _cancellationTokenSource;
  private readonly TContext _context;

  private TState _state;

  protected UIApp UIApp => _uiApp;
  public TContext Context => _context;

  public UIAppFlow(UIApp uiApp) {
    _uiApp = uiApp;

    _cancellationTokenSource = new CancellationTokenSource();
    _cancellationTokenSource.RegisterRaiseCancelOnDestroy(_uiApp);
    _context = new TContext {
      UIApp = uiApp,
      CancellationTokenSource = _cancellationTokenSource,
    };
  }

  public async UniTask Start() {
    _state = GetStartState();
    bool isCancel = await OnStart().SuppressCancellationThrow();
    if (!isCancel) isCancel |= await Execute().SuppressCancellationThrow();
    if (!isCancel) await OnEnd().SuppressCancellationThrow();
    if (UIApp) UIApp.Destroy();
  }

  public void Stop() {
    _context.IsEnd = true;
    _cancellationTokenSource.Cancel();
  }

  protected void AddState<TFlowState>(TState state) where TFlowState : UIFlowState<TState, TContext>, new() {
    var flowState = new TFlowState {
      Context = _context
    };
    flowState.Initialize();
    _dictionary.Add(state, flowState);
  }

  protected abstract TState GetStartState();
  protected abstract UniTask OnStart();
  protected abstract UniTask OnEnd();

  private async UniTask Execute() {
    while (!_context.IsEnd) {
      if (!_dictionary.ContainsKey(_state)) {
        Assertion.Assert(false, "Stateが定義されていません. State:" + _state);
        break;
      }

      try {
        _state = await _dictionary[_state].Execute();
      } catch (OperationCanceledException) {
        throw new OperationCanceledException();
      } catch (Exception e) {
        Assertion.Assert(false, e);
      }

      if (_cancellationTokenSource.IsCancellationRequested) throw new OperationCanceledException();
      await UniTask.NextFrame(cancellationToken: _cancellationTokenSource.Token);
    }
  }
}

}