using UnityEngine;

namespace LimeLibrary.UI.Dialog {

public class UIDialogOption {
  public bool IsImmediate { get; set; } = false;
  public bool IsHideOnClickBackground { get; set; }
  public Color BackgroundColor { get; set; } = new Color(0, 0, 0, 0);

  public UIDialogOption(bool isHideOnClickBackground) {
    IsHideOnClickBackground = isHideOnClickBackground;
  }
}

}