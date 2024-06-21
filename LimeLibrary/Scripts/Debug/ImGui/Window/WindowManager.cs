using System;
using System.Collections.Generic;
using ImGuiNET;

namespace LimeLibrary.Debug.ImGui {

public class WindowManager {
  private readonly List<Window> _windowList = new();

  public void UpdateWindow() {
    // 開いているもののみ更新
    foreach (var window in _windowList) {
      if (!window.IsOpen()) continue;
      window.Update();
    }
    // 開いていないものはリストから削除
    foreach (var window in _windowList) {
      if (window.IsOpen()) continue;
      window.Close();
    }
    _windowList.RemoveAll(window => !window.IsOpen());
  }

  public Window CreateWindow(string windowName, Action action, ImGuiWindowFlags windowFlags = ImGuiWindowFlags.None) {
    // 同じ名前ならすでに出ているウィンドウと重複扱い
    var duplicateWindow = _windowList.Find(window => window.Name == windowName);
    if (duplicateWindow != null) return duplicateWindow;

    var window = new Window(windowName, action);
    window.SetWindowFlags(windowFlags);
    _windowList.Add(window);
    return window;
  }
}

}