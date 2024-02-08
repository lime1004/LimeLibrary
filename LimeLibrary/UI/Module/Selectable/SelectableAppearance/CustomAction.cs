using System;

namespace LimeLibrary.UI.Module.Selectable.SelectableAppearance {

public class CustomAction : SelectableAppearance {
  public Action OnApplyAction { get; set; }
  public Action OnRevertAction { get; set; }

  protected override void OnApplyAppearance() {
    OnApplyAction?.Invoke();
  }

  protected override void OnRevertAppearance() {
    OnRevertAction?.Invoke();
  }
}

}