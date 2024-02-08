using Cysharp.Threading.Tasks;
using DG.Tweening;
using LimeLibrary.Extensions;
using LimeLibrary.UI.View;
using UniRx;
using UnityEngine;

namespace LimeLibrary.UI.MessageWindow {

public class KeyWait : UISingleView {
  [SerializeField]
  private Vector2 _animationOffset = new Vector2(0, 10f);
  [SerializeField]
  private float _animationDuration = 0.5f;

  private Vector2 _keyWaitPosition;
  private Tween _moveTween;

  protected UniTask OnInitialize() {
    Animator.RegisterShowHideFadeAnimation(CanvasGroup, 0.1f);

    EventObservables.GetObservable(UIViewEventType.ShowStart).Subscribe(_ => {
      _moveTween?.KillIfActive();
      _moveTween = SetupMoveAnimation();
    }).AddTo(gameObject);

    _keyWaitPosition = transform.AsRectTransform().anchoredPosition;

    return UniTask.CompletedTask;
  }

  private Tween SetupMoveAnimation() {
    transform.AsRectTransform().anchoredPosition = _keyWaitPosition;
    return transform.AsRectTransform().DOAnchorPos(_keyWaitPosition + _animationOffset, _animationDuration).
      SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetLink(gameObject);
  }
}

}