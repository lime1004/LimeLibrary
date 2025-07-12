using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.UI.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace LimeLibrary.UI.View {

internal class UIViewController {
  private readonly IUIViewEventNotifier _eventNotifier;
  private readonly UIAnimator _animator;

  public GameObject RootObject { get; }
  public RectTransform RectTransform { get; }
  public Canvas Canvas { get; }
  public CanvasGroup CanvasGroup { get; }
  public CanvasScaler CanvasScaler { get; }

  public UIViewState State { get; private set; } = UIViewState.Hide;
  public bool IsFocus { get; private set; } = false;

  public string ShowAnimationId { get; set; } = "Show";
  public string HideAnimationId { get; set; } = "Hide";

  public UIViewController(GameObject rootObject, IUIViewEventNotifier eventNotifier, UIAnimator animator) {
    RootObject = rootObject;
    RectTransform = rootObject.transform.AsRectTransform();
    Canvas = rootObject.GetComponent<Canvas>();
    CanvasGroup = rootObject.GetComponent<CanvasGroup>();
    CanvasScaler = rootObject.GetComponent<CanvasScaler>();
    _eventNotifier = eventNotifier;
    _animator = animator;
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
      if (showOption.FocusMode is not UIFocusMode.None) Focus();
      return;
    }

    var mergedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, RootObject.GetCancellationTokenOnDestroy());

    State = UIViewState.Showing;
    if (showOption.IsActiveGameObject) RootObject.SetActive(true);

    _eventNotifier.Notify(UIViewEventType.ShowStart);

    if (_animator) {
      if (_animator.Exists(ShowAnimationId)) {
        _animator.Stop(HideAnimationId);
        if (showOption.IsImmediate) {
          _animator.PlayImmediate(ShowAnimationId);
        } else {
          await _animator.Play(ShowAnimationId, mergedTokenSource.Token);
        }
      }
    }

    State = UIViewState.Show;

    if (showOption.FocusMode is not UIFocusMode.None) {
      if (showOption.FocusMode is UIFocusMode.FocusNextFrame) await UniTask.NextFrame(cancellationToken: mergedTokenSource.Token);
      Focus();
    }

    _eventNotifier.Notify(UIViewEventType.ShowEnd);
  }

  public async UniTask Hide(UIViewHideOption hideOption, CancellationToken cancellationToken) {
    if (State is UIViewState.Hide or UIViewState.Hiding && !hideOption.IsForce) return;

    var mergedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, RootObject.GetCancellationTokenOnDestroy());

    State = UIViewState.Hiding;
    Unfocus();

    _eventNotifier.Notify(UIViewEventType.HideStart);

    if (_animator) {
      if (_animator.Exists(HideAnimationId)) {
        _animator.Stop(ShowAnimationId);
        if (hideOption.IsImmediate) {
          _animator.PlayImmediate(HideAnimationId);
        } else {
          await _animator.Play(HideAnimationId, mergedTokenSource.Token);
        }
      }
    }

    if (hideOption.IsDeactivateGameObject) RootObject.SetActive(false);

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