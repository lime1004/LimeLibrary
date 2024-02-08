namespace LimeLibrary.UI.Module.Selectable.SelectableAppearance {

public abstract class SelectableAppearance {
  public bool IsEnable { get; set; } = true;

  protected abstract void OnApplyAppearance();
  protected abstract void OnRevertAppearance();

  public void Apply() {
    if (!IsEnable) return;
    OnApplyAppearance();
  }

  public void Revert() {
    if (!IsEnable) return;
    OnRevertAppearance();
  }
}

}