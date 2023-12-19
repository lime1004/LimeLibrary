namespace LimeLibrary.Extensions {

/// <summary>
/// 数学の拡張メソッド
/// </summary>
public static class MathExtensions {
  /// <summary>
  /// Degreeを0 <= degree < 360にして返す
  /// </summary>
  /// <param name="degree">角度</param>
  /// <returns>0から360未満の値</returns>
  public static float NormalizeDegree(this float degree) {
    while (degree is < 0.0f or >= 360.0f) {
      if (degree < 0.0f) degree += 360.0f;
      if (360.0f <= degree) degree -= 360.0f;
    }
    return degree;
  }

  /// <summary>
  /// 値が範囲内かどうか
  /// </summary>
  /// <param name="value">整数値</param>
  /// <param name="min">最小値</param>
  /// <param name="max">最大値</param>
  /// <returns></returns>
  public static bool IsRange(this int value, int min, int max) {
    return min <= value && value <= max;
  }

  /// <summary>
  /// 値が偶数かどうか
  /// </summary>
  /// <param name="value">整数値</param>
  /// <returns></returns>
  public static bool IsEven(this int value) {
    return (value % 2 == 0);
  }

  /// <summary>
  /// 値が奇数かどうか
  /// </summary>
  /// <param name="value">整数値</param>
  /// <returns></returns>
  public static bool IsOdd(this int value) {
    return (value % 2 != 0);
  }
}

}