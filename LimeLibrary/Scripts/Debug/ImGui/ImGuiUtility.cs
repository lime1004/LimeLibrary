using System;
using System.Linq;
using UnityEngine;

namespace LimeLibrary.Debug.ImGui {

public static class ImGuiUtility {
  public static void SetFontGlobalScale(float globalScale) {
    ImGuiNET.ImGui.GetIO().FontGlobalScale = globalScale;
  }

  public static void Sprite(Sprite sprite, float scale = 1.0f) {
    var textureRect = sprite.textureRect;
    var size = new Vector2(textureRect.width, textureRect.height);
    Sprite(sprite, size * scale);
  }

  public static void Sprite(Sprite sprite, Vector2 size) {
    var texture = sprite.texture;
    var textureRect = sprite.textureRect;
    var uv0 = new Vector2(textureRect.x / texture.width, 1 - (textureRect.y + textureRect.height) / texture.height);
    var uv1 = new Vector2(uv0.x + (textureRect.width / texture.width), uv0.y + (textureRect.height / texture.height));
    ImGuiNET.ImGui.Image(UImGui.UImGuiUtility.GetTextureId(sprite.texture), size, uv0, uv1);
  }

  public static bool EnumCombo<T>(string label, ref T nowContent) where T : struct, Enum {
    string listString = string.Empty;
    foreach (T e in Enum.GetValues(typeof(T))) {
      listString += e.ToString() + "\0";
    }

    int index = Enum.GetValues(typeof(T)).OfType<T>().ToList().IndexOf(nowContent);
    bool isChange = ImGuiNET.ImGui.Combo(label, ref index, listString);
    nowContent = Enum.GetValues(typeof(T)).OfType<T>().ToList()[index];
    return isChange;
  }

  public static void LabelText(string label, string text, float? aligned = null) {
    ImGuiNET.ImGui.Text(label + " ");
    if (aligned.HasValue) ImGuiNET.ImGui.SameLine(aligned.Value);
    else ImGuiNET.ImGui.SameLine();
    ImGuiNET.ImGui.Text(text);
  }
}

}