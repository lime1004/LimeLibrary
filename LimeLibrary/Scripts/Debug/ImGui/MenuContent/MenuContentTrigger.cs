using System;

namespace LimeLibrary.Debug.ImGui {

public class MenuContentTrigger : IMenuContent {
  public string Label { get; }
  public string Shortcut { get; set; } = string.Empty;
  public bool Enabled { get; set; } = true;
  public bool Triggered { get; set; }
  public Action<bool> OnTriggered { get; set; }

  public MenuContentTrigger(string label, bool triggered = false) {
    Label = label;
    Triggered = triggered;
  }

  public void Initialize() { }

  public void Execute() {
    if (ImGuiNET.ImGui.MenuItem(Label, Shortcut, Triggered, Enabled)) {
      Triggered = !Triggered;
      OnTriggered?.Invoke(Triggered);
    }
  }

  public void OnDestroy() { }
}

}