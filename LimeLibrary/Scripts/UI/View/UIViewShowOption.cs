namespace LimeLibrary.UI.View {

public class UIViewShowOption {
  public UIFocusMode FocusMode { get; set; } = UIFocusMode.FocusNextFrame;
  public bool IsImmediate { get; set; } = false;
  public bool IsForce { get; set; } = false;
  public bool IsActiveGameObject { get; set; } = true;
}

}