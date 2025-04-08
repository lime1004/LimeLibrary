namespace LimeLibrary.UI.View {

public class UIViewShowOption {
  public UIViewFocusMode FocusMode { get; set; } = UIViewFocusMode.FocusNextFrame;
  public bool IsImmediate { get; set; } = false;
  public bool IsForce { get; set; } = false;
  public bool IsActiveGameObject { get; set; } = true;
}

}