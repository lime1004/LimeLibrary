using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.Text;
using LimeLibrary.UI.App;
using LimeLibrary.UI.Parts;
using LimeLibrary.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace LimeLibrary.UI.View {

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Canvas))]
public abstract class UIView : MonoBehaviour, IUIView {
  [SerializeField]
  private UIInitializeBehaviour _initializeBehaviour = UIInitializeBehaviour.Hide;
  [SerializeField]
  private int _initializePriority = 0;
  internal int InitializePriority => _initializePriority;
  [SerializeField]
  private int _focusPriority = 0;
  internal int FocusPriority => _focusPriority;
  [SerializeField]
  private int _id;
  public int Id => _id;

  private UIViewController _controller;
  protected UIAnimator Animator { get; private set; }

  public GameObject RootObject => _controller.RootObject;
  public RectTransform RectTransform => _controller.RectTransform;
  public Canvas Canvas => _controller.Canvas;
  public CanvasGroup CanvasGroup => _controller.CanvasGroup;
  public CanvasScaler CanvasScaler => _controller.CanvasScaler;

  public bool IsInitialized { get; private set; }
  public UIViewState State => _controller.State;
  public bool IsFocus => _controller.IsFocus;
  public IUIViewEventObservables EventObservables { get; private set; }
  public IObservable<Unit> OnShowEndObservable => EventObservables.GetObservable(UIViewEventType.ShowEnd);
  public IObservable<Unit> OnHideEndObservable => EventObservables.GetObservable(UIViewEventType.HideEnd);
  public IUIInputObservables InputObservables => ParentApp.InputObservables;
  public UIApp ParentApp { get; private set; }

  public CancellationToken ObjectCancellationToken => gameObject.GetCancellationTokenOnDestroy();

  public T GetParentApp<T>() where T : UIApp {
    return ParentApp as T;
  }

  public async UniTask Initialize(UIApp parentApp, CancellationToken cancellationToken) {
    ParentApp = parentApp;

    // AnimationIdGetterの生成
    var animationIdGetter = new UIAnimationIdGetter();

    // Animatorの生成
    Animator = new UIAnimator();
    Animator.SetAnimationIdGetter(animationIdGetter);

    // Eventの生成
    var observables = new UIViewEventObservables();
    EventObservables = observables;

    // Controllerの生成
    _controller = new UIViewController(gameObject, observables, Animator, animationIdGetter);

    // UIPartsの初期化
    foreach (var uiParts in RootObject.GetComponentsInChildren<IUIParts>(true)) {
      uiParts.Initialize(this);
    }

    // Defaultアニメーション再生
    if (Animator.Exists(animationIdGetter.DefaultAnimationID)) {
      Animator.PlayImmediate(animationIdGetter.DefaultAnimationID);
    }

    // 初期化時挙動の実行
    switch (_initializeBehaviour) {
    case UIInitializeBehaviour.Hide:
      _controller.Hide(new UIViewHideOption { IsImmediate = true, IsForce = true }, ObjectCancellationToken).RunHandlingError().Forget();
      break;
    case UIInitializeBehaviour.Show:
      _controller.Show(new UIViewShowOption { IsImmediate = true, IsForce = true }, ObjectCancellationToken).RunHandlingError().Forget();
      break;
    default:
      Assertion.Assert(false, _initializeBehaviour);
      break;
    }

    await OnInitialize(cancellationToken);

    IsInitialized = true;
  }

  protected abstract UniTask OnInitialize(CancellationToken cancellationToken);
  public virtual void OnUpdate() { }

  public bool IsEnable() {
    if (_controller == null || !_controller.IsEnable()) return false;

    // 親Appが存在しない場合はfalse
    if (ParentApp == null) return false;
    // 親Appが表示されていない場合はfalse
    if (ParentApp.State != UIAppState.Show && ParentApp.State != UIAppState.Showing) return false;
    // 他のViewがポップアップとして表示されている場合はfalse
    if (ParentApp.DialogController.ExistsDialog() && (UIView) ParentApp.DialogController.GetTopDialog() != this) return false;

    return true;
  }

  public async UniTask Show(CancellationToken cancellationToken) => await Show(new UIViewShowOption(), cancellationToken);
  public async UniTask Show(UIViewShowOption showOption, CancellationToken cancellationToken) => await _controller.Show(showOption, cancellationToken);

  public UniTask Hide(CancellationToken cancellationToken) => Hide(new UIViewHideOption(), cancellationToken);
  public async UniTask Hide(UIViewHideOption hideOption, CancellationToken cancellationToken) => await _controller.Hide(hideOption, cancellationToken);

  public void Focus() => _controller.Focus();
  public void Unfocus() => _controller.Unfocus();

  public void OnDestroyView() => _controller.OnDestroy();

  public void SetPosition(Vector2 position) => _controller.SetPosition(position);
  public void SetAnchoredPosition(Vector2 anchoredPosition) => _controller.SetAnchoredPosition(anchoredPosition);

  public void SetSortingOrderFront() => _controller.SetSortingOrderFront();

  public string GetUIText(string label) => ParentApp.GetUIText(label);
  public string GetText<TDataTable, TData>(TextQuery<TDataTable, TData> textQuery, string label) where TDataTable : ITable<TData> where TData : struct, ITextData => ParentApp.GetText(textQuery, label);

}

}