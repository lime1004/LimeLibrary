using UnityEngine;

namespace LimeLibrary.UI.Module.Selectable.SelectableAppearance {

public class CursorMover : SelectableAppearance {
  private readonly GameObject _rootObject;
  private readonly SelectCursor.SelectCursor _selectCursor;

  public CursorMover(GameObject rootObject, SelectCursor.SelectCursor selectCursor) {
    _rootObject = rootObject;
    _selectCursor = selectCursor;
  }

  protected override void OnApplyAppearance() {
    _selectCursor?.EnableCursor();
    _selectCursor?.MoveCursor(_rootObject);
  }

  protected override void OnRevertAppearance() { }
}

}