#if LIME_UNITASK
using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LimeLibrary.Extensions {

/// <summary>
/// 外部ライブラリに依存しないための簡易Tween機能
/// </summary>
public static class TweenExtensions {
  public static async UniTask To(Func<float> getter, Action<float> setter, float endValue, float duration, CancellationToken cancellationToken, EasingType easingType = EasingType.Linear) {
    await To(setter, duration, t => Mathf.Lerp(getter(), endValue, t), easingType, cancellationToken);
  }

  public static async UniTask To(Func<Vector2> getter, Action<Vector2> setter, Vector2 endValue, float duration, CancellationToken cancellationToken, EasingType easingType = EasingType.Linear) {
    await To(setter, duration, t => Vector2.Lerp(getter(), endValue, t), easingType, cancellationToken);
  }

  public static async UniTask To(Func<Color> getter, Action<Color> setter, Color endValue, float duration, CancellationToken cancellationToken, EasingType easingType = EasingType.Linear) {
    await To(value => setter(value.ToColor()), duration, t => Vector4.Lerp(getter().ToVector4(), endValue.ToVector4(), t), easingType, cancellationToken);
  }

  public static async UniTask To(Func<string> getter, Action<string> setter, string endValue, float duration, CancellationToken cancellationToken, EasingType easingType = EasingType.Linear) {
    await To(setter, duration, t => {
      string from = getter();
      int count = (int) (endValue.Length * t);
      var builder = new StringBuilder(from);
      for (int i = 0; i < count; i++) {
        if (i < from.Length) {
          builder[i] = endValue[i];
        } else {
          builder.Append(endValue[i]);
        }
      }
      return builder.ToString();
    }, easingType, cancellationToken);
  }

  private static async UniTask To<T>(Action<T> setter, float duration, Func<float, T> lerpFunc, EasingType easingType, CancellationToken cancellationToken) {
    float startTime = Time.time;
    float endTime = startTime + Mathf.Max(duration, float.MinValue);

    while (Time.time < endTime) {
      if (cancellationToken.IsCancellationRequested) break;

      float elapsed = Time.time - startTime;
      float t = Easing.Ease(easingType, elapsed / duration);
      var value = lerpFunc(t);
      setter(value);

      await UniTask.NextFrame(PlayerLoopTiming.Update, cancellationToken);
    }
  }

  public static async UniTask PlayFadeTween(this CanvasGroup canvasGroup, float endValue, float duration, CancellationToken cancellationToken) {
    await To(() => canvasGroup.alpha, value => canvasGroup.alpha = value, endValue, duration, cancellationToken);
  }

  public static async UniTask PlayFadeTween(this Image image, float endValue, float duration, CancellationToken cancellationToken) {
    await To(() => image.color.a, image.SetAlpha, endValue, duration, cancellationToken);
  }

  public static async UniTask PlayColorTween(this Image image, Color endValue, float duration, CancellationToken cancellationToken) {
    await To(() => image.color, value => image.color = value, endValue, duration, cancellationToken);
  }

  public static async UniTask PlayAnchorPosTween(this RectTransform rectTransform, Vector2 endValue, float duration, CancellationToken cancellationToken) {
    await To(() => rectTransform.anchoredPosition, value => rectTransform.anchoredPosition = value, endValue, duration, cancellationToken);
  }

  public static async UniTask PlayTextTween(this TMP_Text text, string endValue, float duration, CancellationToken cancellationToken) {
    await To(() => text.text, value => text.text = value, endValue, duration, cancellationToken);
  }
}

}
#endif