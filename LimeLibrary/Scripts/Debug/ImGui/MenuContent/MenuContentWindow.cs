using System;
using ImGuiNET;

namespace LimeLibrary.Debug.ImGui {

public class MenuContentWindow : IMenuContent {
  private readonly WindowManager _windowManager;
  private readonly string _windowName;
  private readonly Action _action;
  private readonly IWindowBehaviour _windowBehaviour;
  private readonly ImGuiWindowFlags _flags;

  public string Label { get; }
  public string Shortcut { get; set; } = string.Empty;
  public bool Enabled { get; set; } = true;

  public MenuContentWindow(string label, WindowManager windowManager, string windowName, Action action, ImGuiWindowFlags flags = ImGuiWindowFlags.None) :
    this(label, windowManager, windowName, flags) {
    _action = action;
  }

  public MenuContentWindow(string label, WindowManager windowManager, string windowName, IWindowBehaviour windowBehaviour, ImGuiWindowFlags flags = ImGuiWindowFlags.None) :
    this(label, windowManager, windowName, flags) {
    _windowBehaviour = windowBehaviour;
  }

  private MenuContentWindow(string label, WindowManager windowManager, string windowName, ImGuiWindowFlags flags = ImGuiWindowFlags.None) {
    Label = label;
    _windowManager = windowManager;
    _windowName = windowName;
    _flags = flags;
  }

  public void Initialize() { }

  public void Execute() {
    if (ImGuiNET.ImGui.MenuItem(Label, Shortcut, false, Enabled)) {
      _windowManager.CreateWindow(_windowName, GetWindowAction(), _flags);
    }
  }

  private Action GetWindowAction() {
    return _windowBehaviour != null ? () => _windowBehaviour.Window() : _action;
  }

  public void OnDestroy() { }
}

}