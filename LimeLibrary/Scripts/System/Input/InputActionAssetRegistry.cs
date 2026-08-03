using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LimeLibrary.Input {

public static class InputActionAssetRegistry {
  private static readonly List<InputActionAsset> s_inputActionAssetList = new();

  /// <summary>
  /// ランタイムで使用するInputActionAssetを登録する
  /// </summary>
  public static void Register(InputActionAsset inputActionAsset) {
    if (inputActionAsset == null) return;
    if (s_inputActionAssetList.Contains(inputActionAsset)) return;
    s_inputActionAssetList.Add(inputActionAsset);
  }

  /// <summary>
  /// 登録済みのInputActionAssetを登録解除する
  /// </summary>
  public static void Unregister(InputActionAsset inputActionAsset) {
    if (inputActionAsset == null) return;
    s_inputActionAssetList.Remove(inputActionAsset);
  }

  /// <summary>
  /// 登録済みのInputActionAssetから同じIDのInputActionを引き直す
  /// </summary>
  public static InputAction Resolve(InputAction inputAction) {
    if (inputAction == null) return null;
    if (s_inputActionAssetList.Count == 0) return inputAction;

    // 登録済みアセットのアクションなら引き直す必要がない
    var ownerInputActionAsset = inputAction.actionMap?.asset;
    if (ownerInputActionAsset != null && s_inputActionAssetList.Contains(ownerInputActionAsset)) return inputAction;

    foreach (var inputActionAsset in s_inputActionAssetList) {
      if (inputActionAsset == null) continue;
      var resolvedInputAction = inputActionAsset.FindAction(inputAction.id);
      if (resolvedInputAction != null) return resolvedInputAction;
    }

    return inputAction;
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  private static void ResetStaticState() {
    s_inputActionAssetList.Clear();
  }
}

}