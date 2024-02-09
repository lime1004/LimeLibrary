using System;
using LimeLibrary.Sound;

namespace LimeLibrary.UI.Module.Selectable.SelectableAppearance {

public class SoundPlayer<T> : SelectableAppearance where T : Enum {
  private readonly string _seId;
  private readonly T _soundKitType;

  public SoundPlayer(string seId, T soundKitType) {
    _seId = seId;
    _soundKitType = soundKitType;
  }

  protected override void OnApplyAppearance() {
    SoundManager.Instance.Play(_soundKitType, _seId);
  }

  protected override void OnRevertAppearance() { }
}

}