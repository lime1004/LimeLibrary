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

  private bool _isPaused;

  private string ClipName => _animationClip.name;

  public void Play() {
    if (_animation == null || _animationClip == null) return;

    if (_isPaused) {
      _animation[ClipName].speed = 1f;
      _isPaused = false;
      return;
    }

    EnsureLegacyAndAssign();
    _animation.Play(ClipName);
  }

  public void PlayImmediate() {
    if (_animation == null || _animationClip == null) return;

    EnsureLegacyAndAssign();
    _animation.Play(ClipName);
    var state = _animation[ClipName];
    state.normalizedTime = 1f;
    _animation.Sample();
    _animation.Stop();
  }

  public void Pause() {
    if (_animation == null || _animationClip == null) return;

    var state = _animation[ClipName];
    if (state != null && _animation.isPlaying) {
      state.speed = 0f;
      _isPaused = true;
    }
  }

  public void Stop() {
    if (_animation == null) return;

    _animation.Rewind();
    _animation.Stop();
    _isPaused = false;
  }

  public UniTask WaitPlaying(CancellationToken cancellationToken) {
    return UniTask.WaitWhile(
      () => _animation != null && _animation.isPlaying,
      cancellationToken: cancellationToken
    );
  }

  private void EnsureLegacyAndAssign() {
    if (!_animationClip.legacy) {
      _animationClip.legacy = true;
    }
    _animation.AddClip(_animationClip, _animationClip.name);
    _animation.clip = _animationClip;
  }
}

}