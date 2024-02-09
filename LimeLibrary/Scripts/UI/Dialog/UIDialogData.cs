using LimeLibrary.UI.View;

namespace LimeLibrary.UI.Dialog {

public class UIDialogData {
  public IUIView UIView { get; set; }
  public UIDialogOption DialogOption { get; }

  public UIDialogData(IUIView uiView, UIDialogOption dialogOption) {
    UIView = uiView;
    DialogOption = dialogOption;
  }
}

}