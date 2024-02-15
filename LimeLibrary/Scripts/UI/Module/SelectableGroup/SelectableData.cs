using System.Collections.Generic;
using UnityEngine.UI;

namespace LimeLibrary.UI.Module.SelectableGroup {

public class SelectableData {
  private readonly List<object> _dataList = new();

  public UnityEngine.UI.Selectable Selectable { get; }

  public SelectableData(UnityEngine.UI.Selectable selectable) {
    Selectable = selectable;
  }

  public bool IsSelectable() {
    if (Selectable == null) return false;
    if (!Selectable.gameObject.activeSelf) return false;
    if (!Selectable.interactable) return false;

    return true;
  }

  public void ClearNavigation() {
    Selectable.navigation = new Navigation {
      mode = Navigation.Mode.None,
    };
  }

  public void AddData(object data) {
    _dataList.Add(data);
  }

  public T GetData<T>(int index) {
    return (T) _dataList[index];
  }

  public void SetDataList(IEnumerable<object> dataList) {
    _dataList.Clear();
    _dataList.AddRange(dataList);
  }
}

}