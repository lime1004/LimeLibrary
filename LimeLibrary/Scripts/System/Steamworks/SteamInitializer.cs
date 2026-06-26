using System;
using LimeLibrary.Platform;
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
    if (_lifecyclePrefab == null) return;

    var lifecycle = parent != null ?
      Object.Instantiate(_lifecyclePrefab, parent.transform) :
      Object.Instantiate(_lifecyclePrefab);
    lifecycle.Initialize(_steamId);
  }
}

}