using System;
using System.Collections.Generic;
using System.Linq;
using LimeLibrary.Input.InputMode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LimeLibrary.Input {

[Serializable, CreateAssetMenu(
   fileName = "InputBindingPathGetter",
   menuName = "LimeLibrary/Input/InputBindingPathGetter")]
public class InputBindingPathGetter : ScriptableObject {
  [SerializeField]
  private InputActionAsset _inputActionAsset;

  private Dictionary<string, string> _groupNameDictionary = new();

  public string GetInputBindingPath(InputAction inputAction, IInputMode inputMode) {
    return GetInputBindingPaths(inputAction, inputMode).FirstOrDefault();
  }

  public IEnumerable<string> GetInputBindingPaths(InputAction inputAction, IInputMode inputMode) {
    string groupName;
    if (_groupNameDictionary.TryGetValue(inputMode.Name, out string value)) {
      groupName = value;
    } else {
      groupName = GetGroupName(inputMode);
      _groupNameDictionary.Add(inputMode.Name, groupName);
    }

    var bindings = inputAction.bindings.Where(binding => ContainsGroup(binding.groups, groupName));
    return bindings.Select(binding => binding.hasOverrides ? binding.overridePath : binding.path);
  }

  public string GetCompositePartBindingPath(InputAction inputAction, IInputMode inputMode, string partName) {
    string groupName = GetOrCacheGroupName(inputMode);

    var bindings = inputAction.bindings;
    bool inTargetComposite = false;

    for (int i = 0; i < bindings.Count; i++) {
      var binding = bindings[i];

      // コンポジット親を見つけたらグループをチェック
      if (binding.isComposite) {
        inTargetComposite = ContainsGroup(binding.groups, groupName);
        continue;
      }

      // 対象コンポジット内のパーツで、名前が一致するものを返す
      if (inTargetComposite && binding.isPartOfComposite) {
        if (string.Equals(binding.name, partName, StringComparison.OrdinalIgnoreCase)) {
          return binding.hasOverrides ? binding.overridePath : binding.path;
        }
      }
    }

    return null;
  }

  public IReadOnlyDictionary<string, string> GetCompositePartBindingPaths(InputAction inputAction, IInputMode inputMode) {
    string groupName = GetOrCacheGroupName(inputMode);
    var result = new Dictionary<string, string>();

    var bindings = inputAction.bindings;
    bool inTargetComposite = false;

    for (int i = 0; i < bindings.Count; i++) {
      var binding = bindings[i];

      if (binding.isComposite) {
        inTargetComposite = ContainsGroup(binding.groups, groupName);
        continue;
      }

      if (inTargetComposite && binding.isPartOfComposite) {
        string path = binding.hasOverrides ? binding.overridePath : binding.path;
        result[binding.name.ToLower()] = path;
      }
    }

    return result;
  }

  private string GetOrCacheGroupName(IInputMode inputMode) {
    if (_groupNameDictionary.TryGetValue(inputMode.Name, out string value)) {
      return value;
    }
    string groupName = GetGroupName(inputMode);
    _groupNameDictionary.Add(inputMode.Name, groupName);
    return groupName;
  }

  private bool ContainsGroup(string groups, string groupName) {
    return groups.Split(";").Any(group => group == groupName);
  }

  private string GetGroupName(IInputMode inputMode) {
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

  private string[] GetControlPaths(IInputMode inputMode) {
    return inputMode.GetControlPaths().Select(deviceName => $"<{deviceName}>").ToArray();
  }
}

}