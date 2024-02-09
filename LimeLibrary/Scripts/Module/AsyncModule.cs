#if LIME_UNITASK
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Utility;

namespace LimeLibrary.Module {

/// <summary>
/// Asyncな処理が可能なクラス
/// </summary>
public class AsyncModule {
  public async UniTask Wait(float seconds, SecondsCounter secondsCounter, CancellationToken cancellationToken) {
    // NOTE: UniTask.Delayを使うと一時停止ができないので、SecondsCounterを監視する形にする
    while (secondsCounter.Get() < seconds) await UniTask.NextFrame(cancellationToken: cancellationToken);
  }

  public async UniTask DoAsync(float duration, SecondsCounter secondsCounter, Action<float> action, CancellationToken cancellationToken) {
    await DoAsync(duration, EasingType.Linear, secondsCounter, action, cancellationToken);
  }

  public async UniTask DoAsync(float duration, EasingType easingType, SecondsCounter secondsCounter, Action<float> action, CancellationToken cancellationToken) {
    float seconds;
    do {
      seconds = secondsCounter.Get();
      float t = duration == 0f ? 1f : seconds / duration;
      float easingTime = Easing.Ease(easingType, t);
      action?.Invoke(easingTime);

      await UniTask.NextFrame(cancellationToken: cancellationToken);
    } while (seconds < duration);
  }
}

}
#endif