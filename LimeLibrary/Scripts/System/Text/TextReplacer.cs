using System.Text;
using UnityEngine;

namespace LimeLibrary.Text {

public static class TextReplacer {
  public static string TagStartString => "{";
  public static string TagEndString => "}";

  public static string ReplaceTag(string text, params string[] replaceTexts) {
    var stringBuilder = new StringBuilder(text, text.Length * 2);
    for (int i = 0; i < replaceTexts.Length; i++) {
      ReplaceTag(stringBuilder, i, replaceTexts[i]);
    }
    return stringBuilder.ToString();
  }

  public static string ReplaceTag(string text, int tagIndex, string replaceText, Color? color = null) {
    return ReplaceTag(new StringBuilder(text, text.Length * 2), tagIndex, replaceText, color).ToString();
  }

  private static StringBuilder ReplaceTag(StringBuilder text, int tagIndex, string replaceText, Color? color = null) {
    if (color.HasValue) {
      string colorCode = ColorUtility.ToHtmlStringRGB(color.Value);
      replaceText = $"<color=#{colorCode}>{replaceText}</color>";
    }
    return text.Replace($"{TagStartString}{tagIndex}{TagEndString}", replaceText);
  }

  public static string Replace(string text, string oldChar, string newChar) {
    var stringBuilder = new StringBuilder(text);
    return stringBuilder.Replace(oldChar, newChar).ToString();
  }
}

}