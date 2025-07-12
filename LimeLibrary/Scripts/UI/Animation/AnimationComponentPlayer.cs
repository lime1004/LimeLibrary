using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LimeLibrary.UI.Animation {

[Serializable]
public class AnimationComponentPlayer : IUIAnimationPlayer {
  [SerializeField]
  private string _id;
  public string Id => _id;

  [SerializeField]
  private UnityEngine.Animation _animation;
  [SerializeField]
  private AnimationClip _animationClip;

  public void Play() {
    var state = _animation[_animation.name];
    if (state != null && state.speed == 0f && state.time > 0f && state.time < state.length) {
      state.speed = 1f;
    } else {
      _animationClip.legacy = true;
      _animation.clip = _animationClip;
      _animation.Play(_animationClip.name);
    }
  }

  public void PlayImmediate() {
    _animation.Play(_animation.clip.name);
    _animation[_animation.clip.name].normalizedTime = 1f;
  }

  public void Pause() {
    _animation[_animation.clip.name].speed = 0f;
  }

  public void Stop() {
    _animation.Rewind();
    _animation.Stop();
  }

  public UniTask WaitPlaying(CancellationToken cancellationToken) {
    return UniTask.WaitWhile(() => _animation.isPlaying, cancellationToken: cancellationToken);
  }
}

}