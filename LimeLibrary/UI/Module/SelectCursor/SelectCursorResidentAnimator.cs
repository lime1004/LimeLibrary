using System.Collections.Generic;
using DG.Tweening;
using LimeLibrary.Extensions;
using LimeLibrary.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace LimeLibrary.UI.Module.SelectCursor {

public class SelectCursorResidentAnimator {
  private class AnimationData {
    private enum AnimationType {
      Move,
      Fade,
    }

    private AnimationType Type { get; }
    private float Duration { get; }
    private Ease Ease { get; }
    private float FloatValue { get; set; }
    private Vector2 Vector2Value { get; set; }

    private Tween _tween;

    private AnimationData(AnimationType type, float duration, Ease ease) {
      Type = type;
      Duration = duration;
      Ease = ease;
    }

    public static AnimationData CreateMoveAnimationData(float duration, Ease ease, Vector2 anchoredPosition) {
      return new AnimationData(AnimationType.Move, duration, ease) { Vector2Value = anchoredPosition };
    }

    public static AnimationData CreateFadeAnimationData(float duration, Ease ease, float alpha) {
      return new AnimationData(AnimationType.Fade, duration, ease) { FloatValue = alpha };
    }

    public void Play(GameObject animationObject) {
      Stop();

      switch (Type) {
      case AnimationType.Move: {
        var rectTransform = animationObject.transform.AsRectTransform();
        if (rectTransform == null) return;
        _tween = rectTransform.DOAnchorPos(Vector2Value, Duration);
        break;
      }

      case AnimationType.Fade: {
        var image = animationObject.GetComponent<Image>();
        var canvasGroup = animationObject.GetComponent<CanvasGroup>();
        if (canvasGroup) {
          _tween = canvasGroup.DOFade(FloatValue, Duration);
        } else if (image) {
          _tween = image.DOFade(FloatValue, Duration);
        }
        break;
      }

      default:
        Assertion.Assert(false);
        break;
      }

      _tween.SetLoops(-1, LoopType.Yoyo).SetEase(Ease).SetLink(animationObject);
    }

    public void Stop() {
      _tween?.KillIfActive();
    }
  }

  private readonly GameObject _animationObject;
  private readonly List<AnimationData> _animationDataList = new(4);

  public SelectCursorResidentAnimator(GameObject animationObject) {
    _animationObject = animationObject;
  }

  public void AddMoveAnimation(float duration, Ease ease, Vector2 anchoredPosition, bool isAutoPlay = true) {
    var animationData = AnimationData.CreateMoveAnimationData(duration, ease, anchoredPosition);
    _animationDataList.Add(animationData);
    if (isAutoPlay) Play();
  }

  public void AddFadeAnimation(float duration, Ease ease, float alpha, bool isAutoPlay = true) {
    var animationData = AnimationData.CreateFadeAnimationData(duration, ease, alpha);
    _animationDataList.Add(animationData);
    if (isAutoPlay) Play();
  }

  public void Play() {
    foreach (var animationData in _animationDataList) {
      animationData.Play(_animationObject);
    }
  }

  public void Stop() {
    foreach (var animationData in _animationDataList) {
      animationData.Stop();
    }
  }
}

}