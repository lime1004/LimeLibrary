#if LIME_DOTWEEN
using DG.Tweening;

namespace LimeLibrary.Extensions {

/// <summary>
/// DOTweenの拡張メソッド
/// </summary>
public static class DOTweenExtensions {
  /// <summary>
  /// ActiveならKillする
  /// ActiveじゃないTweenをKillすると警告が出るのでこちらを使用する
  /// </summary>
  public static void KillIfActive(this Tween t, bool complete = false) {
    if (t.IsActive()) t.Kill(complete);
  }
}

}
#endif