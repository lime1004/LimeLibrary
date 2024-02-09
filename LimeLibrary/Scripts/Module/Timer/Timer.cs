namespace LimeLibrary.Utility {

/// <summary>
/// 時間測定用クラス
/// </summary>
public static class Timer {
  /// <summary>
  /// 測定開始
  /// </summary>
  public static TimerInstance Start() {
    return new TimerInstance();
  }
}

}