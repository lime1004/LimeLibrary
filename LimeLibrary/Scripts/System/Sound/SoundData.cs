#if LIME_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.Utility;
using UnityEngine;

namespace LimeLibrary.Sound {

public class SoundData {
  private readonly ISoundSource _soundSource;
  private CancellationTokenSource _fadeCancellationTokenSource;

  internal float MasterVolume { get; set; } = 1f;
  public float FadeDuration { get; set; }
  public float Volume { get; private set; } = 1f;

  public SoundData(ISoundSource soundSource) {
    if (soundSource == null) {
      Assertion.Assert(false, "SoundSource is null");
      return;
    }

    _soundSource = soundSource;
    _soundSource.Volume = 0f;
  }

  public void Play() {
    PlayAsync().RunHandlingError().Forget();
  }

  private async UniTask PlayAsync(CancellationToken cancellationToken = default) {
    if (_soundSource == null) return;

    if (_soundSource.IsPaused()) {
      _soundSource.SetPause(false);
    } else {
      if (!_soundSource.IsPlaying()) {
        _soundSource.Play();
      }
    }
    await FadeIn(cancellationToken);
  }

  public void Pause() {
    PauseAsync().RunHandlingError().Forget();
  }

  private async UniTask PauseAsync(CancellationToken cancellationToken = default) {
    await FadeOut(cancellationToken);
    _soundSource?.SetPause(true);
  }

  public void Resume() {
    ResumeAsync().RunHandlingError().Forget();
  }

  private async UniTask ResumeAsync() {
    _soundSource?.SetPause(false);
    await FadeIn();
  }

  public void Stop() {
    StopAsync().RunHandlingError().Forget();
  }

  private async UniTask StopAsync() {
    await FadeOut();
    _soundSource?.Stop();
  }

  public void Destroy() {
    _soundSource?.Destroy();
  }

  public bool IsAlive() => _soundSource != null;

  public async UniTask WaitPlayEndOrStop(CancellationToken cancellationToken) {
    await UniTask.WaitUntil(() => _soundSource.IsPlayEnd() || _soundSource.IsStopped(), cancellationToken: cancellationToken);
  }

  public void SetVolume(float volume, float duration = 0f) {
    Volume = volume;
    Fade(Volume, _soundSource.Volume, duration).RunHandlingError().Forget();
  }

  public void UpdateVolume() {
    _soundSource.Volume = Volume * MasterVolume;
  }

  private async UniTask FadeOut(CancellationToken cancellationToken = default) {
    await Fade(0f, _soundSource.Volume, FadeDuration, cancellationToken);
  }

  private async UniTask FadeIn(CancellationToken cancellationToken = default) {
    await Fade(Volume, _soundSource.Volume, FadeDuration, cancellationToken);
  }

  private async UniTask Fade(float endValue, float fromValue, float duration, CancellationToken cancellationToken = default) {
    _fadeCancellationTokenSource?.Cancel();
    _fadeCancellationTokenSource = new CancellationTokenSource();

    var mergedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
      _fadeCancellationTokenSource.Token,
      _soundSource.CancellationToken,
      cancellationToken).Token;

    if (duration <= 0f) {
      _soundSource.Volume = endValue * MasterVolume;
    } else {
      await FadeVolumeAsync(_soundSource, fromValue, endValue, duration, mergedCancellationToken);
    }
  }

  private async UniTask FadeVolumeAsync(ISoundSource soundSource, float fromValue, float endValue, float duration, CancellationToken cancellationToken) {
    float startTime = Time.time;
    float endTime = startTime + Mathf.Max(duration, float.MinValue);

    while (Time.time < endTime) {
      if (cancellationToken.IsCancellationRequested) break;

      float elapsed = Time.time - startTime;
      float newVolume = Mathf.Lerp(fromValue, endValue, elapsed / duration);
      soundSource.Volume = newVolume * MasterVolume;

      await UniTask.NextFrame(PlayerLoopTiming.Update, cancellationToken);
    }
  }
}

}
#endif