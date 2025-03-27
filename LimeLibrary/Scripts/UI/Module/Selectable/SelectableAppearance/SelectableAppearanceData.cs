using System.Collections.Generic;
using FastEnumUtility;
using LimeLibrary.Input;

namespace LimeLibrary.UI.Module.Selectable.SelectableAppearance {

internal class SelectableAppearanceData {
  private readonly List<string> _enableInputModeList = new(8);
  public SelectableAppearance SelectableAppearance { get; }

  public SelectableAppearanceData(SelectableAppearance selectableAppearance, params string[] enableInputModes) {
    SelectableAppearance = selectableAppearance;
    foreach (string inputMode in enableInputModes) {
      _enableInputModeList.Add(inputMode);
    }
  }

  public void Apply() => SelectableAppearance.Apply();
  public void Revert() => SelectableAppearance.Revert();

  public bool IsEnableInputMode(string inputMode) {
    return _enableInputModeList.Contains(inputMode);
  }
}

}