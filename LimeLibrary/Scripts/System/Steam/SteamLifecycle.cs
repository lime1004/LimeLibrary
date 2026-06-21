#if LIME_STEAMWORKS
using System;
using Steamworks;
using LimeLibrary.Utility;
#endif

namespace LimeLibrary.Steam {

/// <summary>
/// Steam の初期化ライフサイクルを担う受動メカニズム（静的クラス）.
/// init するタイミング（いつ・どの AppID で）と RunCallbacks を回す責任は呼び出し側が握る.
/// 自動初期化はしない（Awake シングルトンにしない）.
/// 初期化失敗は握り潰し, ゲームを止めない（best-effort）.
/// </summary>
public static class SteamLifecycle {
  /// <summary>
  /// 初期化済みかどうか. SDK 型に触れず bool を返すだけなので, LIME_STEAMWORKS 無効時もコンパイルできる.
  /// </summary>
  public static bool Initialized { get; private set; }

  /// <summary>
  /// Steam を初期化する. 二重初期化はガードする.
  /// RestartAppIfNecessary が true（Steam クライアント経由での再起動が必要）な場合,
  /// このメソッドはアプリの終了を要求し（Application.Quit）, false を返す.
  /// この終了要求が, 受動メカニズムである本クラスが唯一持つ副作用.
  /// 失敗（Steam 未起動・AppID 不一致・native dll 不在等）は全例外を握り潰して false を返す.
  /// </summary>
  /// <param name="appId">Steam の AppID.</param>
  /// <returns>初期化に成功したら true. 再起動要求時・初期化失敗時は false.</returns>
  public static bool Initialize(uint appId) {
#if LIME_STEAMWORKS
    if (Initialized) return true;
    try {
      if (SteamAPI.RestartAppIfNecessary(new AppId_t(appId))) {
        Logger.LogWarning("SteamLifecycle: Steam 経由での再起動が必要. アプリを終了する.");
        UnityEngine.Application.Quit();
        return false;
      }
      if (!SteamAPI.Init()) {
        Logger.LogWarning("SteamLifecycle: SteamAPI.Init() failed.");
        Initialized = false;
        return false;
      }
      Initialized = true;
      return true;
    } catch (Exception e) {
      Logger.LogWarning($"SteamLifecycle: Initialize threw an exception. {e}");
      Initialized = false;
      return false;
    }
#else
    return false;
#endif
  }

  /// <summary>
  /// Steam のコールバックを処理する. 毎フレーム呼び出し側が回す（ポンプの駆動は呼び出し側の責任）.
  /// 未初期化なら何もしない.
  /// </summary>
  public static void RunCallbacks() {
#if LIME_STEAMWORKS
    if (!Initialized) return;
    SteamAPI.RunCallbacks();
#endif
  }

  /// <summary>
  /// Steam を終了する. 未初期化なら何もしない. 失敗は握り潰す.
  /// </summary>
  public static void Shutdown() {
#if LIME_STEAMWORKS
    if (!Initialized) return;
    try {
      SteamAPI.Shutdown();
    } catch (Exception e) {
      Logger.LogWarning($"SteamLifecycle: Shutdown threw an exception. {e}");
    }
    Initialized = false;
#endif
  }
}

}