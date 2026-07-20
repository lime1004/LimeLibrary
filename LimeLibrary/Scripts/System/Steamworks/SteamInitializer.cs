using System;
using LimeLibrary.Platform;
using LimeLibrary.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LimeLibrary.Steam {

[Serializable]
public class SteamInitializer : IPlatformInitializer {
  [SerializeField]
  private uint _steamId;
  [SerializeField]
  private SteamLifecycle _lifecyclePrefab;

  public void Initialize(GameObject parent) {
    // LifecycleObjectの生成
    if (_lifecyclePrefab) {
      var lifecycle = parent != null ?
        Object.Instantiate(_lifecyclePrefab, parent.transform) :
        Object.Instantiate(_lifecyclePrefab);
      lifecycle.Initialize(_steamId);
    }

    // プラットフォーム言語取得処理の登録
    LanguageUtility.SetPlatformLanguageResolver(SteamLanguage.GetLanguage);
  }
}

}