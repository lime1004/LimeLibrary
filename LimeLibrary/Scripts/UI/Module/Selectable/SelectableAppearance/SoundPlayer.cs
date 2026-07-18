using System;
using LimeLibrary.Sound;

namespace LimeLibrary.UI.Module.Selectable.SelectableAppearance {

public class SoundPlayer<T> : SelectableAppearance where T : Enum {
  private readonly string _seId;
  private readonly T _soundKitType;

  public Action OnApplyAction { get; set; }
  public Action OnRevertAction { get; set; }

  // 入力モード切替のたびに再生されると連打になるため、再Apply対象から外す。
  public override bool IsReapplyOnInputModeChange => false;

  public SoundPlayer(string seId, T soundKitType) {
    _seId = seId;
    _soundKitType = soundKitType;
  }

  protected override void OnApplyAppearance() {
    SoundManager.Instance.Play(_soundKitType, _seId);
    OnApplyAction?.Invoke();
  }

  protected override void OnRevertAppearance() {
    OnRevertAction?.Invoke();
  }
}

}