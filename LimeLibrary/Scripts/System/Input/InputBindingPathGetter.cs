using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LimeLibrary.Input {

[Serializable, CreateAssetMenu(
   fileName = "InputBindingPathGetter",
   menuName = "LimeLibrary/Input/InputBindingPathGetter")]
public class InputBindingPathGetter : ScriptableObject {
  [SerializeField]
  private InputActionAsset _inputActionAsset;

  private Dictionary<InputMode, string> _groupNameDictionary = new();

  public string GetInputBindingPath(InputAction inputAction, InputMode inputMode) {
    return GetInputBindingPaths(inputAction, inputMode).FirstOrDefault();
  }

  public IEnumerable<string> GetInputBindingPaths(InputAction inputAction, InputMode inputMode) {
    string groupName;
    if (_groupNameDictionary.TryGetValue(inputMode, out string value)) {
      groupName = value;
    } else {
      groupName = GetGroupName(inputMode);
      _groupNameDictionary.Add(inputMode, groupName);
    }

    var bindings = inputAction.bindings.Where(binding => ContainsGroup(binding.groups, groupName));
    return bindings.Select(binding => binding.hasOverrides ? binding.overridePath : binding.path);
  }

  private bool ContainsGroup(string groups, string groupName) {
    return groups.Split(";").Any(group => group == groupName);
  }

  private string GetGroupName(InputMode inputMode) {
    string[] controlPaths = GetControlPaths(inputMode);
    foreach (var controlScheme in _inputActionAsset.controlSchemes) {
      bool existsAll = controlPaths.All(controlPath =>
        controlScheme.deviceRequirements.Any(requirement => requirement.controlPath == controlPath));
      if (existsAll) {
        return controlScheme.name;
      }
    }
    return string.Empty;
  }

  private string[] GetControlPaths(InputMode inputMode) {
    return (inputMode switch {
      InputMode.Gamepad => new[] {
        nameof(Gamepad),
      },
      InputMode.MouseKeyboard => new[] {
        nameof(Mouse),
        nameof(Keyboard),
      },
      _ => new[] {
        string.Empty
      }
    }).Select(deviceName => $"<{deviceName}>").ToArray();
  }
}

}