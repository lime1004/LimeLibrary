using System.Collections.Generic;
using LimeLibrary.Utility;

namespace LimeLibrary.Text {

public class TextQuery<TDataTable, TData> : ITextQuery where TDataTable : ITable<TData> where TData : struct, ITextData {
  private readonly Dictionary<string, ITextData> _textDataDictionary = new();

  public Language DefaultLanguage { get; set; } = Language.Japanese;

  public TextQuery(TDataTable textDataTable) {
    foreach (var textData in textDataTable.List) {
      if (string.IsNullOrEmpty(textData.Label)) continue;
      _textDataDictionary.Add(textData.Label, textData);
    }
  }

  /// <summary>
  /// LabelからTextDataを取得
  /// </summary>
  public ITextData GetTextData(string label) {
    if (!ExistsLabel(label)) {
      Assertion.Assert(false, "指定したLabelのTextDataが見つかりません. " + label);
      return null;
    }
    return _textDataDictionary[label];
  }

  /// <summary>
  /// LabelからTextを取得
  /// </summary>
  public string GetText(string label, Language language) {
    var textData = GetTextData(label);
    if (textData == null) return string.Empty;

    string text = textData.GetText(language);
    return string.IsNullOrEmpty(text) ? textData.GetText(DefaultLanguage) : text;
  }

  /// <summary>
  /// 話者ラベルを取得
  /// </summary>
  public string GetSpeakerLabel(string label) {
    var textData = GetTextData(label);
    return textData == null ? string.Empty : textData.SpeakerLabel;
  }

  /// <summary>
  /// Labelがデータに含まれるか
  /// </summary>
  public bool ExistsLabel(string label) {
    if (label == null) return false;
    return _textDataDictionary.ContainsKey(label);
  }
}

}