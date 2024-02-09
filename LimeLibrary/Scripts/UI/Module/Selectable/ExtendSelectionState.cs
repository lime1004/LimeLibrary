namespace LimeLibrary.UI.Module.Selectable {

public enum ExtendSelectionState {
  Normal,
  Highlighted,
  Pressed,
  Selected,
  Disabled,
  Unclickable,
}

public static class ExtendSelectionStateUtility {
  public static ExtendSelectionState ToExtendSelectionState(int selectionState) {
    return selectionState switch {
      0 => ExtendSelectionState.Normal,
      1 => ExtendSelectionState.Highlighted,
      2 => ExtendSelectionState.Pressed,
      3 => ExtendSelectionState.Selected,
      4 => ExtendSelectionState.Disabled,
      _ => ExtendSelectionState.Normal,
    };
  }

  public static int ToSelectionState(ExtendSelectionState extendSelectionState) {
    return extendSelectionState switch {
      ExtendSelectionState.Normal => 0,
      ExtendSelectionState.Highlighted => 1,
      ExtendSelectionState.Pressed => 2,
      ExtendSelectionState.Selected => 3,
      ExtendSelectionState.Disabled => 4,
      _ => 0,
    };
  }
}

}