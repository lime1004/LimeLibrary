using UnityEngine;

namespace LimeLibrary.Text {

public enum Language {
  /// <summary>
  /// アラビア語
  /// </summary>
  Arabic,
  /// <summary>
  /// ブルガリア語
  /// </summary>
  Bulgarian,
  /// <summary>
  /// チェコ語
  /// </summary>
  Czech,
  /// <summary>
  /// デンマーク語
  /// </summary>
  Danish,
  /// <summary>
  /// オランダ語
  /// </summary>
  Dutch,
  /// <summary>
  /// 英語
  /// </summary>
  English,
  /// <summary>
  /// フィンランド語
  /// </summary>
  Finnish,
  /// <summary>
  /// フランス語
  /// </summary>
  French,
  /// <summary>
  /// ドイツ語
  /// </summary>
  German,
  /// <summary>
  /// ギリシャ語
  /// </summary>
  Greek,
  /// <summary>
  /// ハンガリー語
  /// </summary>
  Hungarian,
  /// <summary>
  /// インドネシア語
  /// </summary>
  Indonesian,
  /// <summary>
  /// イタリア語
  /// </summary>
  Italian,
  /// <summary>
  /// 日本語
  /// </summary>
  Japanese,
  /// <summary>
  /// 韓国語
  /// </summary>
  Korean,
  /// <summary>
  /// ノルウェー語
  /// </summary>
  Norwegian,
  /// <summary>
  /// ポーランド語
  /// </summary>
  Polish,
  /// <summary>
  /// ポルトガル語
  /// </summary>
  Portuguese,
  /// <summary>
  /// ポルトガル語（ブラジル）
  /// </summary>
  PortugueseBrazil,
  /// <summary>
  /// ルーマニア語
  /// </summary>
  Romanian,
  /// <summary>
  /// ロシア語
  /// </summary>
  Russian,
  /// <summary>
  /// スペイン語（スペイン）
  /// </summary>
  SpanishSpain,
  /// <summary>
  /// スペイン語（ラテンアメリカ）
  /// </summary>
  SpanishLatinAmerica,
  /// <summary>
  /// スウェーデン語
  /// </summary>
  Swedish,
  /// <summary>
  /// タイ語
  /// </summary>
  Thai,
  /// <summary>
  /// トルコ語
  /// </summary>
  Turkish,
  /// <summary>
  /// ウクライナ語
  /// </summary>
  Ukrainian,
  /// <summary>
  /// ベトナム語
  /// </summary>
  Vietnamese,
  /// <summary>
  /// 中国語（簡体字）
  /// </summary>
  ChineseSimplified,
  /// <summary>
  /// 中国語（繁体字）
  /// </summary>
  ChineseTraditional,
}

public static class LanguageExtensions {
  public static string ToSteamLanguage(this Language language) {
    switch (language) {
    case Language.Arabic: return "arabic";
    case Language.Bulgarian: return "bulgarian";
    case Language.Czech: return "czech";
    case Language.Danish: return "danish";
    case Language.Dutch: return "dutch";
    case Language.English: return "english";
    case Language.Finnish: return "finnish";
    case Language.French: return "french";
    case Language.German: return "german";
    case Language.Greek: return "greek";
    case Language.Hungarian: return "hungarian";
    case Language.Indonesian: return "indonesian";
    case Language.Italian: return "italian";
    case Language.Japanese: return "japanese";
    case Language.Korean: return "koreana";
    case Language.Norwegian: return "norwegian";
    case Language.Polish: return "polish";
    case Language.Portuguese: return "portuguese";
    case Language.PortugueseBrazil: return "brazilian";
    case Language.Romanian: return "romanian";
    case Language.Russian: return "russian";
    case Language.SpanishSpain: return "spanish";
    case Language.SpanishLatinAmerica: return "latam";
    case Language.Swedish: return "swedish";
    case Language.Thai: return "thai";
    case Language.Turkish: return "turkish";
    case Language.Ukrainian: return "ukrainian";
    case Language.Vietnamese: return "vietnamese";
    case Language.ChineseSimplified: return "schinese";
    case Language.ChineseTraditional: return "tchinese";
    default: return "english";
    }
  }

  public static string ToISO639Code(this Language language) {
    switch (language) {
    case Language.Arabic: return "ar";
    case Language.Bulgarian: return "bg";
    case Language.Czech: return "cs";
    case Language.Danish: return "da";
    case Language.Dutch: return "nl";
    case Language.English: return "en";
    case Language.Finnish: return "fi";
    case Language.French: return "fr";
    case Language.German: return "de";
    case Language.Greek: return "el";
    case Language.Hungarian: return "hu";
    case Language.Indonesian: return "id";
    case Language.Italian: return "it";
    case Language.Japanese: return "ja";
    case Language.Korean: return "ko";
    case Language.Norwegian: return "no";
    case Language.Polish: return "pl";
    case Language.Portuguese: return "pt";
    case Language.PortugueseBrazil: return "pt-br";
    case Language.Romanian: return "ro";
    case Language.Russian: return "ru";
    case Language.SpanishSpain: return "es";
    case Language.SpanishLatinAmerica: return "es-419";
    case Language.Swedish: return "sv";
    case Language.Thai: return "th";
    case Language.Turkish: return "tr";
    case Language.Ukrainian: return "uk";
    case Language.Vietnamese: return "vi";
    case Language.ChineseSimplified: return "zh-cn";
    case Language.ChineseTraditional: return "zh-tw";
    default: return string.Empty;
    }
  }

  public static SystemLanguage? ToSystemLanguage(this Language language) {
    switch (language) {
    case Language.Arabic: return SystemLanguage.Arabic;
    case Language.Bulgarian: return SystemLanguage.Bulgarian;
    case Language.Czech: return SystemLanguage.Czech;
    case Language.Danish: return SystemLanguage.Danish;
    case Language.Dutch: return SystemLanguage.Dutch;
    case Language.English: return SystemLanguage.English;
    case Language.Finnish: return SystemLanguage.Finnish;
    case Language.French: return SystemLanguage.French;
    case Language.German: return SystemLanguage.German;
    case Language.Greek: return SystemLanguage.Greek;
    case Language.Hungarian: return SystemLanguage.Hungarian;
    case Language.Indonesian: return SystemLanguage.Indonesian;
    case Language.Italian: return SystemLanguage.Italian;
    case Language.Japanese: return SystemLanguage.Japanese;
    case Language.Korean: return SystemLanguage.Korean;
    case Language.Norwegian: return SystemLanguage.Norwegian;
    case Language.Polish: return SystemLanguage.Polish;
    case Language.Portuguese: return SystemLanguage.Portuguese;
    case Language.PortugueseBrazil: return SystemLanguage.Portuguese;
    case Language.Romanian: return SystemLanguage.Romanian;
    case Language.Russian: return SystemLanguage.Russian;
    case Language.SpanishSpain: return SystemLanguage.Spanish;
    case Language.SpanishLatinAmerica: return SystemLanguage.Spanish;
    case Language.Swedish: return SystemLanguage.Swedish;
    case Language.Thai: return SystemLanguage.Thai;
    case Language.Turkish: return SystemLanguage.Turkish;
    case Language.Ukrainian: return SystemLanguage.Ukrainian;
    case Language.Vietnamese: return SystemLanguage.Vietnamese;
    case Language.ChineseSimplified: return SystemLanguage.ChineseSimplified;
    case Language.ChineseTraditional: return SystemLanguage.ChineseTraditional;
    default: return null;
    }
  }
}

}