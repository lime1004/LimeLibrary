#if LIME_UNITASK
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.Utility;
using UnityEngine;

namespace LimeLibrary.Sound {

public class SoundData {
  private enum StartState {
    None,
    Pending,
    Started,
  }

  private readonly ISoundSource _soundSource;
  private CancellationTokenSource _fadeCancellationTokenSource;
  private int _requestVersion;
  private StartState _startState;

  internal float MasterVolume { get; set; } = 1f;
  public float FadeDuration { get; set; }
  public float DelaySeconds { get; set; }
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
    if (_startState == StartState.None) _startState = StartState.Pending;
    PlayAsync(++_requestVersion).RunHandlingError().Forget();
  }

  private async UniTask PlayAsync(int requestVersion) {
    if (_soundSource == null) return;

    await UniTask.Delay(TimeSpan.FromSeconds(DelaySeconds), cancellationToken: _soundSource.CancellationToken);

    // 待機中に後から別の操作が来ていれば再生しない
    if (requestVersion != _requestVersion) return;

    bool isPaused = _soundSource.IsPaused();
    if (isPaused) _soundSource.SetPause(false);
    // 再生開始前に一時停止されていた場合も、一時停止の解除だけでは鳴らないので再生を始める
    if (_startState == StartState.Pending || (!isPaused && !_soundSource.IsPlaying())) {
      _startState = StartState.Started;
      _soundSource.Play();
    }
    await FadeIn();
  }

  public void Pause() {
    PauseAsync(++_requestVersion).RunHandlingError().Forget();
  }

  private async UniTask PauseAsync(int requestVersion) {
    if (!await FadeOutForRequest(requestVersion)) return;
    _soundSource.SetPause(true);
  }

  public void Resume() {
    ++_requestVersion;
    ResumeAsync().RunHandlingError().Forget();
  }

  private async UniTask ResumeAsync() {
    if (_soundSource == null) return;

    _soundSource.SetPause(false);
    // 再生開始の待機中に一時停止されていた場合は、ここで再生を始める
    if (_startState == StartState.Pending) {
      _startState = StartState.Started;
      _soundSource.Play();
    }
    await FadeIn();
  }

  public void Stop() {
    if (_startState == StartState.Pending) _startState = StartState.None;
    StopAsync(++_requestVersion).RunHandlingError().Forget();
  }

  private async UniTask StopAsync(int requestVersion) {
    if (!await FadeOutForRequest(requestVersion)) return;
    _soundSource.Stop();
    _startState = StartState.None;
  }

  private async UniTask<bool> FadeOutForRequest(int requestVersion) {
    if (_soundSource == null) return false;

    // 音量変更でフェードが中断されても、後から別の操作が来ていなければ止める
    try {
      await FadeOut();
    } catch (OperationCanceledException) {
    }

    if (_soundSource.CancellationToken.IsCancellationRequested) return false;
    return requestVersion == _requestVersion;
  }

  public void Destroy() {
    ++_requestVersion;
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