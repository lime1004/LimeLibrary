using System;
using System.Collections.Generic;
using UnityEngine;

namespace LimeLibrary.Debug.ImGui {

public class MainMenuBar {
  private class MainMenuInfo {
    public int Order { get; }
    public string MenuName { get; }
    public Action Action { get; }

    public MainMenuInfo(string menuName, Action action, int order = 0) {
      Order = order;
      MenuName = menuName;
      Action = action;
    }
  }

  private readonly List<MainMenuInfo> _mainMenuList = new();
  private bool _isActiveMainMenuBar = false;

  public void Update() {
    // 普段は非表示にしておく
    bool isMouseHoverMainMenuBar = ImGuiNET.ImGui.IsMouseHoveringRect(Vector2.zero, new Vector2(Screen.width, ImGuiNET.ImGui.GetFrameHeight()), false);
    if (!isMouseHoverMainMenuBar && !_isActiveMainMenuBar) return;

    if (ImGuiNET.ImGui.BeginMainMenuBar()) {
      _isActiveMainMenuBar = false;
      foreach (var mainMenu in _mainMenuList) {
        if (ImGuiNET.ImGui.BeginMenu(mainMenu.MenuName)) {
          _isActiveMainMenuBar = true;
          mainMenu.Action?.Invoke();
          ImGuiNET.ImGui.EndMenu();
        }
      }
      ImGuiNET.ImGui.EndMainMenuBar();
    }
  }

  public void AddMainMenu(string menuName, Action action, int order = 0) {
    var mainMenuInfo = new MainMenuInfo(menuName, action, order);
    _mainMenuList.Add(mainMenuInfo);
    _mainMenuList.Sort((a, b) => b.Order - a.Order);
  }
}

}