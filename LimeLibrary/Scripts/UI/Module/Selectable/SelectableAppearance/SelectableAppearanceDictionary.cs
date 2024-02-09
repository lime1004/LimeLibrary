using System.Collections.Generic;
using FastEnumUtility;
using LimeLibrary.System;

namespace LimeLibrary.UI.Module.Selectable.SelectableAppearance {

public class SelectableAppearanceDictionary {
  private readonly Dictionary<ExtendSelectionState, List<SelectableAppearanceData>> _dictionary = new();

  public SelectableAppearanceDictionary() {
    var list = FastEnum.GetValues<ExtendSelectionState>();
    for (int i = 0; i < list.Count; i++) {
      var extendSelectionState = list[i];
      _dictionary.Add(extendSelectionState, new List<SelectableAppearanceData>());
    }
  }

  public void AddAppearance(ExtendSelectionState extendSelectionState, SelectableAppearance selectableAppearance, params InputMode[] enableInputModes) {
    _dictionary[extendSelectionState].Add(new SelectableAppearanceData(selectableAppearance, enableInputModes));
  }

  internal IReadOnlyList<SelectableAppearanceData> GetSelectableAppearanceDataList(ExtendSelectionState extendSelectionState) {
    return _dictionary[extendSelectionState];
  }

  public void ClearAppearance() {
    foreach (var (_, list) in _dictionary) {
      list.Clear();
    }
  }
}

}