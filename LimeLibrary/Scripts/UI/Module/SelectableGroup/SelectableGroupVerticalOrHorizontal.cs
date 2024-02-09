using System.Collections.Generic;
using System.Linq;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using UnityEngine.UI;

namespace LimeLibrary.UI.Module.SelectableGroup {

public abstract class SelectableGroupVerticalOrHorizontal : SelectableGroup {
  protected SortedDictionary<int, List<SelectableData>> _selectableDataDictionary = new();

  protected SelectableGroupVerticalOrHorizontal(
    IUIView parentView,
    SelectableGroupSelectMode selectMode,
    IReadOnlyCollection<string> inputBindingPathList = null) :
    base(parentView, selectMode, inputBindingPathList) { }

  public override List<SelectableData> GetSelectableDataList() {
    var selectableDataList = new List<SelectableData>();
    foreach (var list in _selectableDataDictionary.Values) {
      selectableDataList.AddRange(list);
    }
    return selectableDataList;
  }

  public virtual void AddSelectable(UnityEngine.UI.Selectable selectable, List<object> dataList = null, int axis = 0) {
    if (selectable == null) {
      Assertion.Assert(false);
      return;
    }

    var selectableData = new SelectableData(selectable);
    if (dataList != null) selectableData.SetDataList(dataList);

    if (!_selectableDataDictionary.ContainsKey(axis)) {
      _selectableDataDictionary.Add(axis, new List<SelectableData> { selectableData });
    } else {
      _selectableDataDictionary[axis].Add(selectableData);
    }

    RegisterEvent(selectable);
  }

  public override void AddSelectable(UnityEngine.UI.Selectable selectable, List<object> dataList = null) {
    AddSelectable(selectable, dataList);
  }

  public virtual void AddSelectable(IEnumerable<UnityEngine.UI.Selectable> selectables, List<object> dataList = null, int axis = 0) {
    foreach (var selectable in selectables) {
      AddSelectable(selectable, dataList, axis);
    }
  }

  public override void AddSelectable(IEnumerable<UnityEngine.UI.Selectable> selectables, List<object> dataList = null) {
    AddSelectable(selectables, dataList);
  }

  public override void RemoveSelectable(UnityEngine.UI.Selectable selectable) {
    foreach (var list in _selectableDataDictionary.Values) {
      list.RemoveAll(data => data.Selectable == selectable);
    }
    UnregisterEvent(selectable);
  }

  public override bool ExistsSelectable(UnityEngine.UI.Selectable selectable) {
    foreach (var list in _selectableDataDictionary.Values) {
      if (list.Any(data => data.Selectable == selectable)) return true;
    }
    return false;
  }

  public override SelectableData GetSelectableData(UnityEngine.UI.Selectable selectable) {
    if (!ExistsSelectable(selectable)) {
      Assertion.Assert(false);
      return null;
    }
    foreach (var list in _selectableDataDictionary.Values) {
      var selectableData = list.FirstOrDefault(data => data.Selectable == selectable);
      if (selectableData != null) return selectableData;
    }
    return null;
  }

  public override void ClearSelectable() {
    base.ClearSelectable();
    _selectableDataDictionary.Clear();
  }

  protected void ClearNavigation(UnityEngine.UI.Selectable selectable) {
    selectable.navigation = new Navigation {
      mode = Navigation.Mode.None,
    };
  }

  public abstract void Sort();
}

}