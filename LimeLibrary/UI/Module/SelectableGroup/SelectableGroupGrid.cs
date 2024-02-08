using System.Collections.Generic;
using System.Linq;
using FastEnumUtility;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace LimeLibrary.UI.Module.SelectableGroup {

public class SelectableGroupGrid : SelectableGroup {
  private readonly Dictionary<int, List<SelectableData>> _selectableDataDictionary = new();
  private readonly int _firstAxisCount;
  private readonly GridLayoutGroup.Axis _startAxis;
  private readonly GridLayoutGroup.Corner _startCorner;

  public int FirstAxisCount => _firstAxisCount;
  public int NumRowOrColumn => _selectableDataDictionary.Count;
  public bool IsAllowDifferentIndex { get; set; }
  public bool IsAllowPrevNextRowOrColumn { get; set; }

  private enum NavigationDirection {
    Up,
    Down,
    Left,
    Right,
  }

  public SelectableGroupGrid(
    IUIView parentView,
    int firstAxisCount,
    GridLayoutGroup.Axis startAxis,
    GridLayoutGroup.Corner startCorner,
    SelectableGroupSelectMode selectMode,
    bool isAllowDifferentIndex = false,
    IReadOnlyCollection<string> inputBindingPathList = null) :
    base(parentView, selectMode, inputBindingPathList) {
    _firstAxisCount = firstAxisCount;
    _startAxis = startAxis;
    _startCorner = startCorner;
    IsAllowDifferentIndex = isAllowDifferentIndex;
  }

  public override List<SelectableData> GetSelectableDataList() {
    return _selectableDataDictionary.OrderBy(pair => pair.Key).SelectMany(pair => pair.Value.Select(data => data)).ToList();
  }

  public override void AddSelectable(UnityEngine.UI.Selectable selectable, List<object> dataList = null) {
    AddSelectable(0, selectable, dataList);
  }

  public void AddSelectable(int rowOrColumn, UnityEngine.UI.Selectable selectable, List<object> dataList = null, bool isSetupNavigation = false) {
    if (!_selectableDataDictionary.ContainsKey(rowOrColumn)) {
      _selectableDataDictionary.Add(rowOrColumn, new List<SelectableData>());
    }

    if (selectable == null) {
      Assertion.Assert(false);
      return;
    }

    var selectableData = new SelectableData(selectable);
    if (dataList != null) selectableData.SetDataList(dataList);

    _selectableDataDictionary[rowOrColumn].Add(selectableData);

    UpdateSelectedIndex();

    if (isSetupNavigation) {
      SetupNavigation();
    }

    RegisterEvent(selectable);
  }

  public override void AddSelectable(IEnumerable<UnityEngine.UI.Selectable> selectables, List<object> dataList = null) {
    foreach (var selectable in selectables) {
      AddSelectable(selectable, dataList);
    }
  }

  public override void RemoveSelectable(UnityEngine.UI.Selectable selectable) {
    foreach (var (_, list) in _selectableDataDictionary) {
      list.RemoveAll(data => data.Selectable == selectable);
    }
    UpdateSelectedIndex();
    UnregisterEvent(selectable);
  }

  public override bool ExistsSelectable(UnityEngine.UI.Selectable selectable) {
    foreach (var (_, list) in _selectableDataDictionary) {
      if (list.Any(data => data.Selectable == selectable)) {
        return true;
      }
    }

    return false;
  }

  public override SelectableData GetSelectableData(UnityEngine.UI.Selectable selectable) {
    if (!ExistsSelectable(selectable)) {
      Assertion.Assert(false);
      return null;
    }

    foreach (var (_, list) in _selectableDataDictionary) {
      if (list.Any(data => data.Selectable == selectable)) {
        return list.First(data => data.Selectable == selectable);
      }
    }

    return null;
  }

  public override void ClearSelectable() {
    base.ClearSelectable();
    _selectableDataDictionary.Clear();
  }

  public override void SetupNavigation() {
    // グリッドのNavigationのセットアップ
    SetupNavigationGrid(_selectableDataDictionary, _startAxis, _startCorner, IsAllowDifferentIndex, IsAllowPrevNextRowOrColumn);
  }

  public void SetupNavigationAuto() {
    foreach (var (_, selectableList) in _selectableDataDictionary) {
      foreach (var selectableData in selectableList) {
        selectableData.Selectable.navigation = new Navigation {
          mode = Navigation.Mode.Automatic,
        };
      }
    }
  }

  private void ClearNavigation(UnityEngine.UI.Selectable selectable) {
    selectable.navigation = new Navigation {
      mode = Navigation.Mode.None,
    };
  }

  private void SetupNavigationGrid(
    IReadOnlyDictionary<int, List<SelectableData>> selectableDataDictionary,
    GridLayoutGroup.Axis startAxis,
    GridLayoutGroup.Corner startCorner,
    bool isAllowDifferentIndex,
    bool isAllowPrevNextRowOrColumn) {
    foreach ((int rowOrColumn, var list) in selectableDataDictionary) {
      for (int i = 0; i < list.Count; i++) {
        var selectableData = list[i];
        if (selectableData.Selectable == null) continue;

        selectableData.ClearNavigation();

        var navigation = selectableData.Selectable.navigation;
        navigation.mode = Navigation.Mode.Explicit;

        var values = FastEnum.GetValues<NavigationDirection>();
        for (int k = 0; k < values.Count; k++) {
          var navigationDirection = values[k];
          var selectable = GetNavigationSelectable(selectableDataDictionary, rowOrColumn, i, startAxis, startCorner, navigationDirection, isAllowDifferentIndex,
            isAllowPrevNextRowOrColumn);
          if (selectable == selectableData.Selectable) continue;
          switch (navigationDirection) {
          case NavigationDirection.Up:
            navigation.selectOnUp = selectable;
            break;
          case NavigationDirection.Down:
            navigation.selectOnDown = selectable;
            break;
          case NavigationDirection.Left:
            navigation.selectOnLeft = selectable;
            break;
          case NavigationDirection.Right:
            navigation.selectOnRight = selectable;
            break;
          }
        }

        selectableData.Selectable.navigation = navigation;
      }
    }
  }

  private UnityEngine.UI.Selectable GetNavigationSelectable(
    IReadOnlyDictionary<int, List<SelectableData>> selectableDataDictionary,
    int rowOrColumn,
    int listIndex,
    GridLayoutGroup.Axis startAxis,
    GridLayoutGroup.Corner startCorner,
    NavigationDirection navigationDirection,
    bool isAllowDifferentIndex,
    bool isAllowPrevNextRowOrColumn) {
    bool isRight = startCorner switch {
      GridLayoutGroup.Corner.UpperLeft => true,
      GridLayoutGroup.Corner.UpperRight => false,
      GridLayoutGroup.Corner.LowerLeft => true,
      GridLayoutGroup.Corner.LowerRight => false,
      _ => true
    };
    bool isBottom = startCorner switch {
      GridLayoutGroup.Corner.UpperLeft => true,
      GridLayoutGroup.Corner.UpperRight => true,
      GridLayoutGroup.Corner.LowerLeft => false,
      GridLayoutGroup.Corner.LowerRight => false,
      _ => true
    };

    switch (navigationDirection) {
    case NavigationDirection.Up:
      switch (startAxis) {
      case GridLayoutGroup.Axis.Horizontal:
        return isBottom ?
          GetSelectable(selectableDataDictionary, rowOrColumn - 1, listIndex, isAllowDifferentIndex, isAllowPrevNextRowOrColumn) :
          GetSelectable(selectableDataDictionary, rowOrColumn + 1, listIndex, isAllowDifferentIndex, isAllowPrevNextRowOrColumn);
      case GridLayoutGroup.Axis.Vertical: {
        return isBottom ?
          GetSelectable(selectableDataDictionary, rowOrColumn, listIndex - 1, isAllowDifferentIndex, isAllowPrevNextRowOrColumn) :
          GetSelectable(selectableDataDictionary, rowOrColumn, listIndex + 1, isAllowDifferentIndex, isAllowPrevNextRowOrColumn);
      }
      }
      break;
    case NavigationDirection.Down:
      switch (startAxis) {
      case GridLayoutGroup.Axis.Horizontal:
        return isBottom ?
          GetSelectable(selectableDataDictionary, rowOrColumn + 1, listIndex, isAllowDifferentIndex, isAllowPrevNextRowOrColumn) :
          GetSelectable(selectableDataDictionary, rowOrColumn - 1, listIndex, isAllowDifferentIndex, isAllowPrevNextRowOrColumn);
      case GridLayoutGroup.Axis.Vertical: {
        return isBottom ?
          GetSelectable(selectableDataDictionary, rowOrColumn, listIndex + 1, isAllowDifferentIndex, isAllowPrevNextRowOrColumn) :
          GetSelectable(selectableDataDictionary, rowOrColumn, listIndex - 1, isAllowDifferentIndex, isAllowPrevNextRowOrColumn);
      }
      }
      break;
    case NavigationDirection.Left:
      switch (startAxis) {
      case GridLayoutGroup.Axis.Horizontal:
        return isRight ?
          GetSelectable(selectableDataDictionary, rowOrColumn, listIndex - 1, isAllowDifferentIndex, isAllowPrevNextRowOrColumn) :
          GetSelectable(selectableDataDictionary, rowOrColumn, listIndex + 1, isAllowDifferentIndex, isAllowPrevNextRowOrColumn);
      case GridLayoutGroup.Axis.Vertical: {
        return isRight ?
          GetSelectable(selectableDataDictionary, rowOrColumn - 1, listIndex, isAllowDifferentIndex, isAllowPrevNextRowOrColumn) :
          GetSelectable(selectableDataDictionary, rowOrColumn + 1, listIndex, isAllowDifferentIndex, isAllowPrevNextRowOrColumn);
      }
      }
      break;
    case NavigationDirection.Right:
      switch (startAxis) {
      case GridLayoutGroup.Axis.Horizontal:
        return isRight ?
          GetSelectable(selectableDataDictionary, rowOrColumn, listIndex + 1, isAllowDifferentIndex, isAllowPrevNextRowOrColumn) :
          GetSelectable(selectableDataDictionary, rowOrColumn, listIndex - 1, isAllowDifferentIndex, isAllowPrevNextRowOrColumn);
      case GridLayoutGroup.Axis.Vertical: {
        return isRight ?
          GetSelectable(selectableDataDictionary, rowOrColumn + 1, listIndex, isAllowDifferentIndex, isAllowPrevNextRowOrColumn) :
          GetSelectable(selectableDataDictionary, rowOrColumn - 1, listIndex, isAllowDifferentIndex, isAllowPrevNextRowOrColumn);
      }
      }
      break;
    }
    return null;
  }

  private UnityEngine.UI.Selectable GetSelectable(
    IReadOnlyDictionary<int, List<SelectableData>> selectableDataDictionary,
    int rowOrColumn,
    int listIndex,
    bool isArrowDifferentIndex,
    bool isIncludePrevNextRowOrColumn) {
    if (selectableDataDictionary.Count <= 0) return null;
    if (!selectableDataDictionary.ContainsKey(rowOrColumn)) return null;

    if (isIncludePrevNextRowOrColumn) {
      if (listIndex < 0) {
        rowOrColumn--;
        if (!selectableDataDictionary.ContainsKey(rowOrColumn)) return null;
        listIndex = selectableDataDictionary[rowOrColumn].Count - 1;
      }
      if (listIndex >= selectableDataDictionary[rowOrColumn].Count) {
        rowOrColumn++;
        if (!selectableDataDictionary.ContainsKey(rowOrColumn)) return null;
        listIndex = 0;
      }
    }

    if (listIndex < 0) return null;

    if (isArrowDifferentIndex) {
      int max = Mathf.Max(selectableDataDictionary[rowOrColumn].Count - 1, 0);
      listIndex = Mathf.Clamp(listIndex, 0, max);
    }
    if (listIndex >= selectableDataDictionary[rowOrColumn].Count) return null;

    return selectableDataDictionary[rowOrColumn][listIndex].Selectable;
  }
}

}