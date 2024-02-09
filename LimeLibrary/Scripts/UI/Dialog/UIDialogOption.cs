namespace LimeLibrary.UI.Dialog {

public class UIDialogOption {
  public bool IsImmediate { get; set; } = false;
  public bool IsHideOnClickBackground { get; set; }
  public bool IsDarkenBackground { get; set; }

  public UIDialogOption(bool isHideOnClickBackground, bool isDarkenBackground) {
    IsHideOnClickBackground = isHideOnClickBackground;
    IsDarkenBackground = isDarkenBackground;
  }
}

}