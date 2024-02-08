﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.UI.App;
using LimeLibrary.UI.View;

namespace LimeLibrary.UI {

public abstract class UIFlowState<TState, TContext> where TState : Enum where TContext : UIFlowContext {
  protected UIApp UIApp => Context.UIApp;
  protected CancellationToken CancellationToken => Context.CancellationTokenSource.Token;

  public TContext Context { get; set; }

  public abstract void Initialize();
  public abstract UniTask<TState> Execute();

  protected TView GetView<TView>() where TView : UIView {
    return UIApp.GetView<TView>();
  }

  protected void RequestEnd() {
    Context.IsEnd = true;
  }
}

}