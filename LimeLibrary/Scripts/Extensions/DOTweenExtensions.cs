#if LIME_DOTWEEN
using System.Threading;
using DG.Tweening;
#if LIME_UNITASK
using Cysharp.Threading.Tasks;
#endif

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

#if UNITASK_DOTWEEN_SUPPORT
  /// <summary>
  /// TweenのUniTask変換
  /// TweenCancelBehaviourはKillAndCancelAwait指定
  /// </summary>
  /// <param name="t"></param>
  /// <param name="cancellationToken"></param>
  public static async UniTask ToUniTask(this Tween t, CancellationToken cancellationToken = default) {
    await t.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
  }
#endif
}

}
#endif