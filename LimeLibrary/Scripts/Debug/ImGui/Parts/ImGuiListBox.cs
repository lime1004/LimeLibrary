using System.Collections.Generic;
using UnityEngine;

namespace LimeLibrary.Debug.ImGui {

public class ImGuiListBox {
  private List<string> _textList;
  private string _selectText = string.Empty;

  private const float DefaultBoxWidth = 500f;
  private const float DefaultRow = 10;

  public Vector2 ListBoxSize { get; set; }
  public string SelectText => _selectText;

  public ImGuiListBox() {
    _textList = new List<string>();
  }

  public ImGuiListBox(List<string> textList) {
    Set(textList);
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
    if (ImGuiNET.ImGui.BeginListBox("", ListBoxSize)) {
      for (int n = 0; n < _textList.Count; n++) {
        if (ImGuiNET.ImGui.Selectable(_textList[n], false)) {
          _selectText = _textList[n];
        }
      }
      ImGuiNET.ImGui.EndListBox();
    }
  }

  public void Set(List<string> textList) {
    _textList = textList;
  }

  public void Add(string text) {
    _textList.Add(text);
  }

  public void Clear() {
    _textList.Clear();
  }
}

}