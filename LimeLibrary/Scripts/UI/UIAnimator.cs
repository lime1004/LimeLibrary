using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using UnityEngine;

namespace LimeLibrary.UI {

public class UIAnimator {
  private readonly List<Animation> _animationList = new(32);

  public async UniTask Play(string id, CancellationToken cancellationToken) {
    await _animationList.Where(animation => animation.ID == id).Select(animation => animation.Play(cancellationToken));
  }

  public void PlayImmediate(string id) {
    foreach (var animation in _animationList) {
      if (animation.ID != id) continue;
      animation.PlayImmediate();
    }
  }

  public void Stop(string id) {
    foreach (var animation in _animationList) {
      if (animation.ID != id) continue;
      animation.Stop();
    }
  }

  public bool Exists(string id) {
    return _animationList.Any(animation => animation.ID == id);
  }

  public void Register(string id, Func<CancellationToken, UniTask> animationFunc, Action animationImmediate = null, bool isOverwrite = false) {
    _animationList.Add(new Animation(id, animationFunc, animationImmediate));
  }

  public void RegisterDefaultAnimation(Action animation, bool isOverwrite = false) {
    Register(_animationIdGetter.DefaultAnimationID, null, animation, isOverwrite);
  }

  public void RegisterShowAnimation(Func<CancellationToken, UniTask> animationFunc, Action animationImmediate = null, bool isOverwrite = false) {
    Register(_animationIdGetter.ShowAnimationID, animationFunc, animationImmediate, isOverwrite);
  }

  public void RegisterHideAnimation(Func<CancellationToken, UniTask> animationFunc, Action animationImmediate = null, bool isOverwrite = false) {
    Register(_animationIdGetter.HideAnimationID, animationFunc, animationImmediate, isOverwrite);
  }

  public void RegisterShowHideFadeAnimation(CanvasGroup canvasGroup, float duration) {
    RegisterShowAnimation(async ct => {
      await canvasGroup.PlayFadeTween(1f, duration, ct);
    }, () => canvasGroup.alpha = 1f);
    RegisterHideAnimation(async ct => {
      await canvasGroup.PlayFadeTween(0f, duration, ct);
    }, () => canvasGroup.alpha = 0f);
  }

  private UIAnimationIdGetter _animationIdGetter;

  internal void SetAnimationIdGetter(UIAnimationIdGetter animationIdGetter) {
    _animationIdGetter = animationIdGetter;
  }

  private class Animation {
    public string ID { get; }
    public Func<CancellationToken, UniTask> Func { get; set; }
    public Action AnimationImmediate { get; set; }
    private CancellationTokenSource _cancellationTokenSource;

    public Animation(string id, Func<CancellationToken, UniTask> func, Action animationImmediate = null) {
      ID = id;
      Func = func;
      AnimationImmediate = animationImmediate;
    }

    public async UniTask Play(CancellationToken cancellationToken) {
      _cancellationTokenSource?.Cancel();
      _cancellationTokenSource = new CancellationTokenSource();

      var mergedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
      if (Func != null) await Func(mergedTokenSource.Token);
    }

    public void PlayImmediate() {
      AnimationImmediate?.Invoke();
    }

    public void Stop() {
      _cancellationTokenSource?.Cancel();
    }
  }
}

}