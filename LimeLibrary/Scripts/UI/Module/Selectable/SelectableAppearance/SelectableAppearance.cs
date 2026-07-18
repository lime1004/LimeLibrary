namespace LimeLibrary.UI.Module.Selectable.SelectableAppearance {

public abstract class SelectableAppearance {
  public bool IsEnable { get; set; } = true;
  // 入力モード変更時、SelectableExtender は現在状態の演出を再Applyし直す（カーソル復帰等の前提のため既定はtrue）。
  // SEのように再生が副作用を持つ演出はfalseで上書きしてApply対象から外す（Revertは後始末のため常に実行される）。
  public virtual bool IsReapplyOnInputModeChange => true;

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