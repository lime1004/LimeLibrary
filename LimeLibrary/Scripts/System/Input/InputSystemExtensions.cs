using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace LimeLibrary.Input {

public static class InputSystemExtensions {
  public static IEnumerable<string> GetInputBindingPaths(this InputAction inputAction) {
    return inputAction.bindings.Select(binding => binding.hasOverrides ? binding.overridePath : binding.path);
  }

  public static bool IsActionChangeTarget(this InputAction inputAction, object actionOrMapOrAsset) {
    // NOTE: InputSystem.onActionChangeの通知対象は階層の最上位（アクション単体 / マップ / アセット）が渡る
    if (inputAction == null) return false;
    if (actionOrMapOrAsset == null) return false;
    if (ReferenceEquals(inputAction, actionOrMapOrAsset)) return true;

    var inputActionMap = inputAction.actionMap;
    if (inputActionMap == null) return false;
    if (ReferenceEquals(inputActionMap, actionOrMapOrAsset)) return true;

    var inputActionAsset = inputActionMap.asset;
    return inputActionAsset != null && ReferenceEquals(inputActionAsset, actionOrMapOrAsset);
  }
}

}