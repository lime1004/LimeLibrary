using System.Collections.Generic;
using FastEnumUtility;
using LimeLibrary.Input;

namespace LimeLibrary.UI.Module.Selectable.SelectableAppearance {

internal class SelectableAppearanceData {
  private readonly List<InputMode> _enableInputModeList = new(FastEnum.GetValues<InputMode>().Count);
  public SelectableAppearance SelectableAppearance { get; private set; }

  public SelectableAppearanceData(SelectableAppearance selectableAppearance, params InputMode[] enableInputModes) {
    SelectableAppearance = selectableAppearance;
    foreach (var inputMode in enableInputModes) {
      _enableInputModeList.Add(inputMode);
    }
  }

  public void Apply() => SelectableAppearance.Apply();
  public void Revert() => SelectableAppearance.Revert();

  public bool IsEnableInputMode(InputMode inputMode) {
    return _enableInputModeList.Contains(inputMode);
  }
}

}