using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LimeLibrary.Debug.ImGui {

public class ImGuiSearchBox {
  private readonly string _label;
  private readonly List<string> _targetList;
  private readonly List<string> _resultList = new();

  private string _inputText = string.Empty;

  private const float DefaultBoxWidth = 500f;
  private const float DefaultRow = 10;

  public Vector2 ListBoxSize { get; set; }
  public string InputText => _inputText;

  public ImGuiSearchBox(string label, List<string> targetList) {
    _label = label;
    _targetList = targetList;
    _targetList.Sort();
    UpdateResultList();
  }

  public void SetSize(float width, int row) {
    float height = row * ImGuiNET.ImGui.GetTextLineHeightWithSpacing();
    ListBoxSize = new Vector2(width, height);
  }

  public void SetRow(int row) {
    float height = 10 * ImGuiNET.ImGui.GetTextLineHeightWithSpacing();
    ListBoxSize = new Vector2(ListBoxSize.x > 0 ? ListBoxSize.x : DefaultBoxWidth, height);
  }

  public void SetWidth(float width) {
    float defaultHeight = DefaultRow * ImGuiNET.ImGui.GetTextLineHeightWithSpacing();
    ListBoxSize = new Vector2(width, ListBoxSize.y > 0 ? ListBoxSize.y : defaultHeight);
  }

  public void OnGUI() {
    if (ImGuiNET.ImGui.InputText(_label, ref _inputText, 512)) {
      UpdateResultList();
    }

    if (ImGuiNET.ImGui.BeginListBox("", ListBoxSize)) {
      for (int n = 0; n < _resultList.Count; n++) {
        if (ImGuiNET.ImGui.Selectable(_resultList[n], false)) {
          _inputText = _resultList[n];
        }
      }
      ImGuiNET.ImGui.EndListBox();
    }
  }

  private void UpdateResultList() {
    _resultList.Clear();
    if (string.IsNullOrEmpty(_inputText)) {
      _resultList.AddRange(_targetList.Where(str => !string.IsNullOrEmpty(str)));
    } else {
      foreach (string target in _targetList) {
        if (target.Contains(_inputText, StringComparison.OrdinalIgnoreCase)) {
          _resultList.Add(target);
        }
      }
    }
  }
}

}