using System.Diagnostics;

namespace LimeLibrary.Utility {

/// <summary>
/// Timerの内部クラス
/// </summary>
public class TimerInstance {
  private readonly Stopwatch _stopwatch;

  public TimerInstance() {
    _stopwatch = new Stopwatch();
    _stopwatch.Start();
  }

  /// <summary>
  /// タイム表示＆計測終了
  /// </summary>
  public void StopLog(string str) {
    Logger.Log($"{str} : {_stopwatch.ElapsedMilliseconds}ms");
    _stopwatch.Stop();
  }

  /// <summary>
  /// タイム表示＆計測リセット
  /// </summary>
  public void RestartLog(string str) {
    Logger.Log($"{str} : {_stopwatch.ElapsedMilliseconds}ms");
    _stopwatch.Restart();
  }
}

}