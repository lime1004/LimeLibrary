using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace LimeLibrary.System {

public static class InputSystemExtensions {
  public static IEnumerable<string> GetInputBindingPaths(this InputAction inputAction) {
    return inputAction.bindings.Select(binding => binding.hasOverrides ? binding.overridePath : binding.path);
  }
}

}