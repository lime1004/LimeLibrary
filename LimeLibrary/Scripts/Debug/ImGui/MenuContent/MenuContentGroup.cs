using System.Collections.Generic;

namespace LimeLibrary.Debug.ImGui {

public class MenuContentGroup : IMenuContent {
  private readonly List<IMenuContent> _menuContents = new();
  public IReadOnlyList<IMenuContent> MenuContents => _menuContents;

  public string Label { get; }

  public MenuContentGroup(string label) {
    Label = label;
  }

  public void Initialize() {
    foreach (var menuContent in _menuContents) {
      menuContent.Initialize();
    }
  }

  public void Execute() {
    if (ImGuiNET.ImGui.BeginMenu(Label)) {
      foreach (var menuContent in _menuContents) {
        menuContent.Execute();
      }
      ImGuiNET.ImGui.EndMenu();
    }
  }

  public void OnDestroy() {
    foreach (var menuContent in _menuContents) {
      menuContent.OnDestroy();
    }
  }

  public void AddMenuContent(IMenuContent menuContent) {
    _menuContents.Add(menuContent);
  }

  public void RemoveMenuContent(IMenuContent menuContent) {
    _menuContents.Remove(menuContent);
  }
}

}