using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.UI.Parts;
using LimeLibrary.Utility;
using R3;
using UnityEngine;

namespace LimeLibrary.UI.View {

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Canvas))]
public class UISingleView : MonoBehaviour, IUIView {
  [SerializeField]
  private UIInitializeBehaviour _initializeBehaviour = UIInitializeBehaviour.Hide;

  private UIViewController _controller;
  protected UIAnimator Animator { get; private set; }

  public GameObject RootObject => _controller.RootObject;
  public RectTransform RectTransform => _controller.RectTransform;
  public Canvas Canvas => _controller.Canvas;
  public CanvasGroup CanvasGroup => _controller.CanvasGroup;

  public UIViewState State => _controller.State;
  public bool IsFocus => _controller.IsFocus;

  public IUIViewEventObservables EventObservables { get; private set; }
  public Observable<Unit> OnShowEndObservable => EventObservables.GetObservable(UIViewEventType.ShowEnd);
  public Observable<Unit> OnHideEndObservable => EventObservables.GetObservable(UIViewEventType.HideEnd);
  public IUIInputObservables InputObservables { get; private set; } = new UIInputObservables();

  public CancellationToken ObjectCancellationToken => gameObject.GetCancellationTokenOnDestroy();

  public void SetInputObservable(IUIInputObservables inputObservables) => InputObservables = inputObservables;

  public async UniTask Initialize(CancellationToken cancellationToken) {
    // AnimationIdGetterの生成
    var animationIdGetter = new UIAnimationIdGetter();

    // Animatorの生成
    Animator = new UIAnimator();
    Animator.SetAnimationIdGetter(animationIdGetter);

    // EventObservablesの生成
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
      Hide(new UIViewHideOption { IsImmediate = true, IsForce = true }, ObjectCancellationToken).RunHandlingError().Forget();
      break;
    case UIInitializeBehaviour.Show:
      Show(new UIViewShowOption { IsImmediate = true, IsForce = true }, ObjectCancellationToken).RunHandlingError().Forget();
      break;
    default:
      Assertion.Assert(false, _initializeBehaviour);
      break;
    }

    await OnInitialize(cancellationToken);
  }

  protected virtual UniTask OnInitialize(CancellationToken cancellationToken) => UniTask.CompletedTask;

  public bool IsEnable() => _controller != null && _controller.IsEnable();

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
  
  public void Destroy() => OnDestroyView();
}

}