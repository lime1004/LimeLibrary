using System.Collections.Generic;
using System.Linq;
using LimeLibrary.UI.View;
using UnityEngine.UI;

namespace LimeLibrary.UI.Module.SelectableGroup {

public class SelectableGroupVertical : SelectableGroupVerticalOrHorizontal {
  public SelectableGroupVertical(IUIView parentView, SelectableGroupSelectMode selectMode, IReadOnlyCollection<string> inputBindingPathList = null) :
    base(parentView, selectMode, inputBindingPathList) { }

  public override void AddSelectable(UnityEngine.UI.Selectable selectable, List<object> dataList = null, int column = 0) {
    base.AddSelectable(selectable, dataList, column);
  }

  public override void AddSelectable(IEnumerable<UnityEngine.UI.Selectable> selectables, List<object> dataList = null, int column = 0) {
    base.AddSelectable(selectables, dataList, column);
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

    // 行のNavigation接続
    foreach (var list in _selectableDataDictionary.Values) {
      ConnectNavigationRow(list);
    }
    // 列のNavigation接続
    ConnectNavigationColumn(_selectableDataDictionary);
  }

  private void ConnectNavigationRow(List<SelectableData> selectableDataList) {
    for (int i = 0; i < selectableDataList.Count; i++) {
      UnityEngine.UI.Selectable prevSelectable = null;
      for (int k = i - 1; k >= 0; k--) {
        if (!selectableDataList[k].IsSelectable()) continue;
        prevSelectable = selectableDataList[k].Selectable;
        break;
      }
      UnityEngine.UI.Selectable nextSelectable = null;
      for (int k = i + 1; k < selectableDataList.Count; k++) {
        if (!selectableDataList[k].IsSelectable()) continue;
        nextSelectable = selectableDataList[k].Selectable;
        break;
      }

      var navigation = selectableDataList[i].Selectable.navigation;
      navigation.mode = Navigation.Mode.Explicit;
      navigation.selectOnUp = prevSelectable;
      navigation.selectOnDown = nextSelectable;
      selectableDataList[i].Selectable.navigation = navigation;
    }
  }

  private void ConnectNavigationColumn(SortedDictionary<int, List<SelectableData>> selectableDataDictionary) {
    for (int column = 0; column < selectableDataDictionary.Count; column++) {
      for (int row = 0; row < selectableDataDictionary[column].Count; row++) {
        SelectableData prevSelectableData = null;
        for (int k = column - 1; k >= 0; k--) {
          if (selectableDataDictionary[k] == null) continue;
          if (!selectableDataDictionary[k][row].IsSelectable()) continue;
          prevSelectableData = selectableDataDictionary[k][row];
          break;
        }
        SelectableData nextSelectableData = null;
        for (int k = column + 1; k < selectableDataDictionary.Count; k++) {
          if (selectableDataDictionary[k] == null) continue;
          if (!selectableDataDictionary[k][row].IsSelectable()) continue;
          nextSelectableData = selectableDataDictionary[k][row];
          break;
        }

        var selectable = selectableDataDictionary.Values.ToList()[column][row].Selectable;
        var navigation = selectable.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnLeft = prevSelectableData?.Selectable;
        navigation.selectOnRight = nextSelectableData?.Selectable;
        selectable.navigation = navigation;
      }
    }
  }

  public override void Sort() {
    var dic = new SortedDictionary<int, List<SelectableData>>();
    foreach ((int column, var list) in _selectableDataDictionary) {
      var orderedList = list.OrderByDescending(data => data.Selectable.transform.position.y).ToList();
      dic.Add(column, orderedList);
    }
    _selectableDataDictionary = dic;
  }
}

}