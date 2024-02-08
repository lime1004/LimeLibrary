using System.Collections.Generic;
using System.Linq;
using LimeLibrary.UI.View;
using UnityEngine;
using UnityEngine.UI;

namespace LimeLibrary.UI.Module.SelectableGroup {

public class SelectableGroupHorizontal : SelectableGroupVerticalOrHorizontal {
  public SelectableGroupHorizontal(IUIView parentView, SelectableGroupSelectMode selectMode, IReadOnlyCollection<string> inputBindingPathList = null) :
    base(parentView, selectMode, inputBindingPathList) { }

  public override void AddSelectable(UnityEngine.UI.Selectable selectable, List<object> dataList = null, int row = 0) {
    base.AddSelectable(selectable, dataList, row);
  }

  public override void AddSelectable(IEnumerable<UnityEngine.UI.Selectable> selectables, List<object> dataList = null, int row = 0) {
    base.AddSelectable(selectables, dataList, row);
  }

  public override void SetupNavigation() {
    // ソートしておく
    Sort();

    // Navigationをクリア
    foreach (var list in _selectableDataDictionary.Values) {
      foreach (var selectableData in list) {
        selectableData.ClearNavigation();
      }
    }

    // 列のNavigation接続
    foreach (var list in _selectableDataDictionary.Values) {
      ConnectNavigationColumn(list);
    }
    // 行のNavigation接続
    ConnectNavigationRow(_selectableDataDictionary);
  }

  private void ConnectNavigationColumn(List<SelectableData> selectableDataList) {
    for (int i = 0; i < selectableDataList.Count; i++) {
      UnityEngine.UI.Selectable prevSelectable = null;
      if (IsLoop) {
        for (int k = 1; k < selectableDataList.Count; k++) {
          int index = (int) Mathf.Repeat(i - k, selectableDataList.Count);
          if (!selectableDataList[index].IsSelectable()) continue;
          prevSelectable = selectableDataList[index].Selectable;
          break;
        }
      } else {
        for (int k = i - 1; k >= 0; k--) {
          if (!selectableDataList[k].IsSelectable()) continue;
          prevSelectable = selectableDataList[k].Selectable;
          break;
        }
      }
      UnityEngine.UI.Selectable nextSelectable = null;
      if (IsLoop) {
        for (int k = 1; k < selectableDataList.Count; k++) {
          int index = (int) Mathf.Repeat(i + k, selectableDataList.Count);
          if (!selectableDataList[index].IsSelectable()) continue;
          nextSelectable = selectableDataList[index].Selectable;
          break;
        }
      } else {
        for (int k = i + 1; k < selectableDataList.Count; k++) {
          if (!selectableDataList[k].IsSelectable()) continue;
          nextSelectable = selectableDataList[k].Selectable;
          break;
        }
      }

      var navigation = selectableDataList[i].Selectable.navigation;
      navigation.mode = Navigation.Mode.Explicit;
      navigation.selectOnLeft = prevSelectable;
      navigation.selectOnRight = nextSelectable;
      selectableDataList[i].Selectable.navigation = navigation;
    }
  }

  private void ConnectNavigationRow(SortedDictionary<int, List<SelectableData>> selectableDataDictionary) {
    for (int row = 0; row < selectableDataDictionary.Count; row++) {
      for (int column = 0; column < selectableDataDictionary[row].Count; column++) {
        SelectableData prevSelectableData = null;
        for (int k = row - 1; k >= 0; k--) {
          if (selectableDataDictionary[k] == null) continue;
          int c = Mathf.Clamp(column, 0, selectableDataDictionary[k].Count - 1);
          if (!selectableDataDictionary[k][c].IsSelectable()) continue;
          prevSelectableData = selectableDataDictionary[k][c];
          break;
        }
        SelectableData nextSelectableData = null;
        for (int k = row + 1; k < selectableDataDictionary.Count; k++) {
          if (selectableDataDictionary[k] == null) continue;
          int c = Mathf.Clamp(column, 0, selectableDataDictionary[k].Count - 1);
          if (!selectableDataDictionary[k][c].IsSelectable()) continue;
          nextSelectableData = selectableDataDictionary[k][c];
          break;
        }

        var selectable = selectableDataDictionary.Values.ToList()[row][column].Selectable;
        var navigation = selectable.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnUp = prevSelectableData?.Selectable;
        navigation.selectOnDown = nextSelectableData?.Selectable;
        selectable.navigation = navigation;
      }
    }
  }

  public override void Sort() {
    var dic = new SortedDictionary<int, List<SelectableData>>();
    foreach ((int row, var list) in _selectableDataDictionary) {
      var orderedList = list.OrderBy(data => data.Selectable.transform.position.x).ToList();
      dic.Add(row, orderedList);
    }
    _selectableDataDictionary = dic;
  }
}

}