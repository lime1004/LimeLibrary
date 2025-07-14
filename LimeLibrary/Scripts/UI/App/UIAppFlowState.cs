using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.UI.App;
using LimeLibrary.UI.View;

namespace LimeLibrary.UI {

public abstract class UIAppFlowState<TState, TContext> where TState : Enum where TContext : UIAppFlowContext {
  private CancellationTokenSource _cancellationTokenSource;

  protected UIApp UIApp => Context.UIApp;
  protected CancellationToken CancellationToken => _cancellationTokenSource?.Token ?? CancellationToken.None;

  public TContext Context { get; set; }
  public TState CurrentState { get; set; }
  public TState PrevState { get; set; }

  public virtual void PreExecute() {
    _cancellationTokenSource = new CancellationTokenSource();
    _cancellationTokenSource.RegisterRaiseCancelOnDestroy(UIApp);
  }

  public virtual void PostExecute() {
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource = null;
  }

  public abstract void Initialize();
  public abstract UniTask<TState> Execute();

  protected TView GetView<TView>(bool containsSubClass = true, int? id = null, string name = null) where TView : UIView {
    return UIApp.GetView<TView>(containsSubClass: containsSubClass, id: id, name: name);
  }

  protected void RequestEnd() {
    Context.IsEnd = true;
  }
}

}