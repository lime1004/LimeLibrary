using System.Collections.Generic;
using System.Text;
using LimeLibrary.Input;
using UnityEngine.InputSystem;

namespace LimeLibrary.UI.Module.Input {

internal sealed class UIInputBindingSet {
  private readonly List<Registration> _registrationList = new();

  public InputActionType ActionType { get; }
  public string Interactions { get; }
  public IReadOnlyList<Registration> Registrations => _registrationList;
  public bool IsEmpty => _registrationList.Count == 0;

  public UIInputBindingSet(InputActionType actionType, string interactions) {
    ActionType = actionType;
    Interactions = interactions ?? string.Empty;
  }

  public UIInputBindingSet Clone() {
    var clone = new UIInputBindingSet(ActionType, Interactions);
    clone._registrationList.AddRange(_registrationList);
    return clone;
  }

  public void AddPaths(IReadOnlyList<string> paths, string interactions) {
    // 呼び出し側があとで配列を書き換えても影響を受けないよう写して持つ
    _registrationList.Add(new Registration(null, new List<string>(paths), interactions ?? string.Empty));
  }

  public void AddSourceAction(InputAction sourceInputAction, string interactions) {
    _registrationList.Add(new Registration(InputActionAssetRegistry.Resolve(sourceInputAction), null, interactions ?? string.Empty));
  }

  public void Clear() => _registrationList.Clear();

  public bool ContainsSource(object actionOrMapOrAsset) {
    foreach (var registration in _registrationList) {
      if (registration.SourceInputAction == null) continue;
      if (registration.SourceInputAction.IsActionChangeTarget(actionOrMapOrAsset)) return true;
    }
    return false;
  }

  public string CreateKey() {
    // NOTE: 元のアクションは割り当てが変わっても同じ写しを使い続けるため、パスでなくIDで区別する
    var builder = new StringBuilder(128);
    builder.Append(ActionType).Append('|').Append(Interactions);
    foreach (var registration in _registrationList) {
      builder.Append('|');
      if (registration.SourceInputAction != null) {
        builder.Append("action:").Append(registration.SourceInputAction.id);
      } else {
        builder.Append("path:").Append(string.Join(",", registration.BindingPaths));
      }
      builder.Append('#').Append(registration.Interactions);
    }
    return builder.ToString();
  }

  internal readonly struct Registration {
    public InputAction SourceInputAction { get; }
    public IReadOnlyList<string> BindingPaths { get; }
    public string Interactions { get; }

    public Registration(InputAction sourceInputAction, IReadOnlyList<string> bindingPaths, string interactions) {
      SourceInputAction = sourceInputAction;
      BindingPaths = bindingPaths;
      Interactions = interactions;
    }
  }
}

}
