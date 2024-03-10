﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Text;
using LimeLibrary.UI.Dialog;
using LimeLibrary.UI.View;
using UniRx;
using UnityEngine;

namespace LimeLibrary.UI.App {

[RequireComponent(typeof(Canvas))]
public abstract class UIApp : MonoBehaviour, IUI {
  [SerializeField]
  private List<UIView> _showViewList;

  private UIAppController _controller;
  private UIDialogController _dialogController;
  private IUITextGetter _textGetter;
  private IUICommonInput _commonInput = new UICommonInput();

  internal UIDialogController DialogController => _dialogController;

  public IUIAppEventObservables EventObservables { get; private set; } = new UIAppEventObservables();
  public IUIInputObservables InputObservables { get; private set; }

  public GameObject RootObject => gameObject;
  public UIAppState State => _controller.State;
  public IUICommonInput CommonInput => _commonInput;
  public IObservable<Unit> OnShowEndObservable => EventObservables.GetObservable(UIAppEventType.ShowEnd);
  public IObservable<Unit> OnHideEndObservable => EventObservables.GetObservable(UIAppEventType.HideEnd);

  public CancellationToken ObjectCancellationToken => gameObject.GetCancellationTokenOnDestroy();

  public bool IsInitialized { get; private set; }

  public void SetInputObservables(IUIInputObservables inputObservables) => InputObservables = inputObservables;
  public void SetUItextGetter(IUITextGetter textGetter) => _textGetter = textGetter;
  public void SetCommonInput(IUICommonInput commonInput) => _commonInput = commonInput;

  public async UniTask InitializeAsync(CancellationToken cancellationToken) {
    // Eventの生成
    InputObservables = new UIInputObservables();

    // Viewの収集
    var viewArray = GetComponentsInChildren<UIView>(true);
    Array.Sort(viewArray, (view1, view2) => view1.InitializePriority - view2.InitializePriority);

    // Controllerの生成
    _controller = new UIAppController(gameObject, EventObservables as UIAppEventObservables, viewArray.ToList(), _showViewList);
    _dialogController = new UIDialogController(_controller.Views);

    // App初期化前処理
    InitializeApp();

    await _controller.InitializeView(this, cancellationToken);

    IsInitialized = true;
  }

  public string GetUIText(string label) => _textGetter.GetUIText(label);
  public string GetText<TDataTable, TData>(TextQuery<TDataTable, TData> textQuery, string label) where TDataTable : ITable<TData> where TData : struct, ITextData => _textGetter.GetText(textQuery, label);

  protected abstract void InitializeApp();

  public virtual void OnUpdate() {
    for (int i = 0; i < _controller.Views.Count; i++) {
      var view = _controller.Views[i];
      if (!view.gameObject.activeSelf) continue;
      view.OnUpdate();
    }
  }

  public async UniTask Show(CancellationToken cancellationToken) => await _controller.Show(new UIAppShowOption(), cancellationToken);
  public async UniTask Show(UIAppShowOption showOption, CancellationToken cancellationToken) => await _controller.Show(showOption, cancellationToken);

  public UniTask Hide(CancellationToken cancellationToken) => Hide(new UIAppHideOption(), cancellationToken);
  public async UniTask Hide(UIAppHideOption hideOption, CancellationToken cancellationToken) => await _controller.Hide(hideOption, cancellationToken);

  public async UniTask HideAndDestroy(CancellationToken cancellationToken) => await _controller.HideAndDestroy(cancellationToken);
  public void Destroy() => _controller.Destroy();

  public T GetView<T>(bool containsSubClass = true, int? id = null) where T : UIView => _controller.GetView<T>(containsSubClass, id);
  public IEnumerable<UIView> GetViewAll() => _controller.Views;
  public bool ExistsView<T>(bool isCheckSubClass = false, bool isCheckInterface = false) where T : UIView => _controller.ExistsView<T>(isCheckSubClass, isCheckInterface);

  public async UniTask<T> DuplicateView<T>(T view, CancellationToken cancellationToken) where T : UIView => await _controller.DuplicateView(this, view, cancellationToken);

  public async UniTask ShowDialog<T>(UIDialogOption dialogOption, CancellationToken cancellationToken) where T : UIView {
    var view = GetView<T>();
    if (view == null) return;
    await ShowDialog(view, dialogOption, cancellationToken);
  }

  public async UniTask ShowDialog(IUIView view, UIDialogOption dialogOption, CancellationToken cancellationToken) => await _dialogController.Show(view, dialogOption, cancellationToken);
}

}