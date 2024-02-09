using System;
using UnityEngine;

namespace LimeLibrary.Utility {

public enum EasingType {
  Constant,
  InQuad,
  OutQuad,
  InOutQuad,
  InCubic,
  OutCubic,
  InOutCubic,
  InQuart,
  OutQuart,
  InOutQuart,
  InQuint,
  OutQuint,
  InOutQuint,
  InSine,
  OutSine,
  InOutSine,
  InExp,
  OutExp,
  InOutExp,
  InCirc,
  OutCirc,
  InOutCirc,
  InElastic,
  OutElastic,
  InOutElastic,
  InBack,
  OutBack,
  InOutBack,
  InBounce,
  OutBounce,
  InOutBounce,
  Linear,
}

public static class Easing {
  public static float Ease(EasingType type, float t) {
    return type switch {
      EasingType.Constant => Unsupported(t),
      EasingType.InQuad => InQuad(t),
      EasingType.OutQuad => OutQuad(t),
      EasingType.InOutQuad => InOutQuad(t),
      EasingType.InCubic => Unsupported(t),
      EasingType.OutCubic => Unsupported(t),
      EasingType.InOutCubic => Unsupported(t),
      EasingType.InQuart => InQuart(t),
      EasingType.OutQuart => OutQuart(t),
      EasingType.InOutQuart => InOutQuart(t),
      EasingType.InQuint => Unsupported(t),
      EasingType.OutQuint => Unsupported(t),
      EasingType.InOutQuint => Unsupported(t),
      EasingType.InSine => InSine(t),
      EasingType.OutSine => OutSine(t),
      EasingType.InOutSine => InOutSine(t),
      EasingType.InExp => Unsupported(t),
      EasingType.OutExp => Unsupported(t),
      EasingType.InOutExp => Unsupported(t),
      EasingType.InCirc => Unsupported(t),
      EasingType.OutCirc => Unsupported(t),
      EasingType.InOutCirc => Unsupported(t),
      EasingType.InElastic => Unsupported(t),
      EasingType.OutElastic => Unsupported(t),
      EasingType.InOutElastic => Unsupported(t),
      EasingType.InBack => Unsupported(t),
      EasingType.OutBack => Unsupported(t),
      EasingType.InOutBack => Unsupported(t),
      EasingType.InBounce => Unsupported(t),
      EasingType.OutBounce => Unsupported(t),
      EasingType.InOutBounce => Unsupported(t),
      EasingType.Linear => t,
      _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
  }

  private static float Unsupported(float t) {
    Assertion.Assert(false, "未対応のEasingTypeです.");
    return t;
  }

  public static float InSine(float t) {
    return 1 - Mathf.Cos((t * Mathf.PI) / 2);
  }

  public static float OutSine(float t) {
    return Mathf.Sin((t * Mathf.PI) / 2);
  }

  public static float InOutSine(float t) {
    // return Mathf.Cos(t * Mathf.PI) - 1;
    return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
  }

  public static float InQuart(float t) {
    return t * t * t * t;
  }

  public static float OutQuart(float t) {
    return 1 - Mathf.Pow(1 - t, 4);
  }

  public static float InOutQuart(float t) {
    return t < 0.5 ? 8 * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 4) / 2;
  }
  
  public static float InQuad(float t) {
    return t * t;
  }

  public static float OutQuad(float t) {
    return 1 - (1 - t) * (1 - t);
  }

  public static float InOutQuad(float t) {
    return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
  }
}

}