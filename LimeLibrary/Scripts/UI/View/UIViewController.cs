using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace LimeLibrary.UI.View {

internal class UIViewController {
  private readonly IUIViewEventNotifier _eventNotifier;
  private readonly UIAnimator _animator;
  private readonly UIAnimationIdGetter _animationIdGetter;

  public GameObject RootObject { get; }
  public RectTransform RectTransform { get; }
  public Canvas Canvas { get; }
  public CanvasGroup CanvasGroup { get; }
  public CanvasScaler CanvasScaler { get; }

  public UIViewState State { get; private set; } = UIViewState.Hide;
  public bool IsFocus { get; private set; } = false;

  public UIViewController(GameObject rootObject, IUIViewEventNotifier eventNotifier, UIAnimator animator, UIAnimationIdGetter animationIdGetter) {
    RootObject = rootObject;
    RectTransform = rootObject.transform.AsRectTransform();
    Canvas = rootObject.GetComponent<Canvas>();
    CanvasGroup = rootObject.GetComponent<CanvasGroup>();
    CanvasScaler = rootObject.GetComponent<CanvasScaler>();
    _eventNotifier = eventNotifier;
    _animator = animator;
    _animationIdGetter = animationIdGetter;
  }

  public bool IsEnable() {
    // GameObjectが非アクティブの場合はfalse
    if (!RootObject.activeInHierarchy) return false;
    // 表示し切っていない場合はfalse
    if (State != UIViewState.Show) return false;
    // フォーカスされていない場合はfalse
    if (!IsFocus) return false;

    return true;
  }

  public async UniTask Show(UIViewShowOption showOption, CancellationToken cancellationToken) {
    if (State is UIViewState.Show or UIViewState.Showing && !showOption.IsForce) {
      if (showOption.IsFocus) Focus();
      return;
    }

    var mergedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, RootObject.GetCancellationTokenOnDestroy());

    State = UIViewState.Showing;
    RootObject.SetActive(true);

    _eventNotifier.Notify(UIViewEventType.ShowStart);

    if (_animator.Exists(_animationIdGetter.ShowAnimationID)) {
      _animator.Stop(_animationIdGetter.HideAnimationID);
      if (showOption.IsImmediate) {
        _animator.PlayImmediate(_animationIdGetter.ShowAnimationID);
      } else {
        await _animator.Play(_animationIdGetter.ShowAnimationID, mergedTokenSource.Token);
      }
    }

    State = UIViewState.Show;

    if (showOption.IsFocus) Focus();

    _eventNotifier.Notify(UIViewEventType.ShowEnd);
  }

  public async UniTask Hide(UIViewHideOption hideOption, CancellationToken cancellationToken) {
    if (State is UIViewState.Hide or UIViewState.Hiding && !hideOption.IsForce) return;

    var mergedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, RootObject.GetCancellationTokenOnDestroy());

    State = UIViewState.Hiding;
    Unfocus();

    _eventNotifier.Notify(UIViewEventType.HideStart);

    if (_animator.Exists(_animationIdGetter.HideAnimationID)) {
      _animator.Stop(_animationIdGetter.ShowAnimationID);
      if (hideOption.IsImmediate) {
        _animator.PlayImmediate(_animationIdGetter.HideAnimationID);
      } else {
        await _animator.Play(_animationIdGetter.HideAnimationID, mergedTokenSource.Token);
      }
    }

    RootObject.SetActive(false);

    State = UIViewState.Hide;
    _eventNotifier.Notify(UIViewEventType.HideEnd);
  }

  public void Focus() {
    IsFocus = true;
    _eventNotifier.Notify(UIViewEventType.Focus);
  }

  public void Unfocus() {
    IsFocus = false;
    _eventNotifier.Notify(UIViewEventType.Unfocus);
  }

  public void OnDestroy() {
    _eventNotifier.Notify(UIViewEventType.Destroy);
  }

  public void SetPosition(Vector2 position) {
    RectTransform.position = position.ToVector3(RectTransform.position.z);
  }

  public void SetAnchoredPosition(Vector2 anchoredPosition) {
    RectTransform.anchoredPosition = anchoredPosition;
  }

  public void SetSortingOrderFront() {
    RootObject.transform.SetAsLastSibling();
  }
}

}