using System;

namespace LimeLibrary.Debug.ImGui {

public class MenuContentAction : IMenuContent {
  private readonly Action _action;

  public string Label { get; }
  public string Shortcut { get; set; } = string.Empty;
  public bool Enabled { get; set; } = true;

  public MenuContentAction(string label, Action action) {
    Label = label;
    _action = action;
  }

  public void Initialize() { }

  public void Execute() {
    if (ImGuiNET.ImGui.MenuItem(Label, Shortcut, false, Enabled)) _action?.Invoke();
  }

  public void OnDestroy() { }
}

}