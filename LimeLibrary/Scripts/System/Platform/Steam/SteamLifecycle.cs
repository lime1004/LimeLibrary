using UnityEngine;
using Logger = LimeLibrary.Utility.Logger;
using System;
using Steamworks;

namespace LimeLibrary.Steam {

public class SteamLifecycle : MonoBehaviour {
  [SerializeField]
  private uint _appId;

  public static bool IsInitialized { get; private set; }
  public bool Initialized => IsInitialized;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  private static void ResetStaticState() {
    IsInitialized = false;
  }

  private void Update() {
    RunCallbacks();
  }

  private void OnDestroy() {
    Shutdown();
  }

  private void OnApplicationQuit() {
    Shutdown();
  }

  public bool Initialize(uint appId) {
    if (IsInitialized) return true;
    try {
      if (SteamAPI.RestartAppIfNecessary(new AppId_t(appId))) {
        Logger.LogWarning("SteamLifecycle: Steam経由での再起動が必要です。アプリを終了します。");
        Application.Quit();
        return false;
      }
      if (!SteamAPI.Init()) {
        Logger.LogWarning("SteamLifecycle: SteamAPI.Init() failed.");
        IsInitialized = false;
        return false;
      }
      IsInitialized = true;
      return true;
    } catch (Exception e) {
      Logger.LogWarning($"SteamLifecycle: Initialize threw an exception. {e}");
      IsInitialized = false;
      return false;
    }
  }

  private void RunCallbacks() {
    if (!IsInitialized) return;
    SteamAPI.RunCallbacks();
  }

  private void Shutdown() {
    if (!IsInitialized) return;
    try {
      SteamAPI.Shutdown();
    } catch (Exception e) {
      Logger.LogWarning($"SteamLifecycle: Shutdown threw an exception. {e}");
    } finally {
      IsInitialized = false;
    }
  }
}

}
