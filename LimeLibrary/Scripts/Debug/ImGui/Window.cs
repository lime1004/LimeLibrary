using System;
using ImGuiNET;
using UnityEngine;

namespace LimeLibrary.Debug.ImGui {

public class Window {
  private readonly Action _action;
  private bool _isOpen = true;
  private ImGuiWindowFlags _windowFlags = ImGuiWindowFlags.None;
  private Vector2? _requestPosition;
  private Vector2? _requestSize;

  public string Name { get; }

  public Window(string windowName, Action action) {
    Name = windowName;
    _action = action;
  }

  public void Update() {
    if (!_isOpen) return;

    ImGuiNET.ImGui.Begin(Name, ref _isOpen, _windowFlags);
    if (_requestPosition.HasValue) {
      ImGuiNET.ImGui.SetWindowPos(_requestPosition.Value);
      _requestPosition = null;
    }
    if (_requestSize.HasValue) {
      ImGuiNET.ImGui.SetWindowSize(_requestSize.Value);
      _requestSize = null;
    }
    _action?.Invoke();
    ImGuiNET.ImGui.End();
  }

  public void SetPosition(Vector2 position) => _requestPosition = position;
  public void SetSize(Vector2 size) => _requestSize = size;
  public void SetWindowFlags(ImGuiWindowFlags flags) => _windowFlags = flags;

  public void Open() => _isOpen = true;
  public void Close() => _isOpen = false;
  public bool IsOpen() => _isOpen;
}

}