using UnityEngine;
using Logger = LimeLibrary.Utility.Logger;
using System;
using Steamworks;

namespace LimeLibrary.Steam {

public class SteamLifecycle : MonoBehaviour {
  [SerializeField]
  private uint _appId;

  public bool Initialized { get; private set; }

  private void Update() {
    RunCallbacks();
  }

  private void OnDestroy() {
    Shutdown();
  }

  public bool Initialize(uint appId) {
    if (Initialized) return true;
    try {
      if (SteamAPI.RestartAppIfNecessary(new AppId_t(appId))) {
        Logger.LogWarning("SteamLifecycle: Steam経由での再起動が必要です。アプリを終了します。");
        Application.Quit();
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
  }

  private void RunCallbacks() {
    if (!Initialized) return;
    SteamAPI.RunCallbacks();
  }

  private void Shutdown() {
    if (!Initialized) return;
    try {
      SteamAPI.Shutdown();
    } catch (Exception e) {
      Logger.LogWarning($"SteamLifecycle: Shutdown threw an exception. {e}");
    }
    Initialized = false;
  }
}

}