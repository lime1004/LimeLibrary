using UnityEngine;

// ReSharper disable StringLiteralTypo
#if LIME_STEAMWORKS
using Steamworks;
#endif

namespace LimeLibrary.Text {

public static class LanguageUtility {
  public static Language GetDeviceLanguage() {
    // TODO Switch版対応
#if LIME_STEAMWORKS
    if (SteamManager.Initialized) {
      return ConvertFromSteamLanguage(SteamUtils.GetSteamUILanguage());
    }
#endif
    return ConvertFromSystemLanguage(Application.systemLanguage);
  }

  public static Language ConvertFromSteamLanguage(string steamLanguage) {
    return steamLanguage switch {
      "arabic" => Language.Arabic,
      "bulgarian" => Language.Bulgarian,
      "czech" => Language.Czech,
      "danish" => Language.Danish,
      "dutch" => Language.Dutch,
      "english" => Language.English,
      "finnish" => Language.Finnish,
      "french" => Language.French,
      "german" => Language.German,
      "greek" => Language.Greek,
      "hungarian" => Language.Hungarian,
      "indonesian" => Language.Indonesian,
      "italian" => Language.Italian,
      "japanese" => Language.Japanese,
      "koreana" => Language.Korean,
      "norwegian" => Language.Norwegian,
      "polish" => Language.Polish,
      "portuguese" => Language.Portuguese,
      "brazilian" => Language.PortugueseBrazil,
      "romanian" => Language.Romanian,
      "russian" => Language.Russian,
      "spanish" => Language.SpanishSpain,
      "latam" => Language.SpanishLatinAmerica,
      "swedish" => Language.Swedish,
      "thai" => Language.Thai,
      "turkish" => Language.Turkish,
      "ukrainian" => Language.Ukrainian,
      "vietnamese" => Language.Vietnamese,
      "schinese" => Language.ChineseSimplified,
      "tchinese" => Language.ChineseTraditional,
      _ => Language.English
    };
  }

  public static Language ConvertFromSystemLanguage(SystemLanguage systemLanguage) {
    return systemLanguage switch {
      SystemLanguage.Arabic => Language.Arabic,
      SystemLanguage.Bulgarian => Language.Bulgarian,
      SystemLanguage.Czech => Language.Czech,
      SystemLanguage.Danish => Language.Danish,
      SystemLanguage.Dutch => Language.Dutch,
      SystemLanguage.English => Language.English,
      SystemLanguage.Finnish => Language.Finnish,
      SystemLanguage.French => Language.French,
      SystemLanguage.German => Language.German,
      SystemLanguage.Greek => Language.Greek,
      SystemLanguage.Hungarian => Language.Hungarian,
      SystemLanguage.Indonesian => Language.Indonesian,
      SystemLanguage.Italian => Language.Italian,
      SystemLanguage.Japanese => Language.Japanese,
      SystemLanguage.Korean => Language.Korean,
      SystemLanguage.Norwegian => Language.Norwegian,
      SystemLanguage.Polish => Language.Polish,
      SystemLanguage.Portuguese => Language.Portuguese,
      SystemLanguage.Romanian => Language.Romanian,
      SystemLanguage.Russian => Language.Russian,
      SystemLanguage.Spanish => Language.SpanishSpain,
      SystemLanguage.Swedish => Language.Swedish,
      SystemLanguage.Thai => Language.Thai,
      SystemLanguage.Turkish => Language.Turkish,
      SystemLanguage.Ukrainian => Language.Ukrainian,
      SystemLanguage.Vietnamese => Language.Vietnamese,
      SystemLanguage.ChineseSimplified => Language.ChineseSimplified,
      SystemLanguage.ChineseTraditional => Language.ChineseTraditional,
      _ => Language.English
    };
  }

  public static string GetLanguageText(Language language) {
    return language switch {
      Language.Arabic => "العربية",
      Language.Bulgarian => "български език",
      Language.Czech => "čeština",
      Language.Danish => "Dansk",
      Language.Dutch => "Nederlands",
      Language.English => "English",
      Language.Finnish => "Suomi",
      Language.French => "Français",
      Language.German => "Deutsch",
      Language.Greek => "Ελληνικά",
      Language.Hungarian => "Magyar",
      Language.Indonesian => "Indonesian",
      Language.Italian => "Italiano",
      Language.Japanese => "日本語",
      Language.Korean => "한국어",
      Language.Norwegian => "Norsk",
      Language.Polish => "Polski",
      Language.Portuguese => "Português",
      Language.PortugueseBrazil => "Português-Brasil",
      Language.Romanian => "Română",
      Language.Russian => "Русский",
      Language.SpanishSpain => "Español-España",
      Language.SpanishLatinAmerica => "Español-Latinoamérica",
      Language.Swedish => "Svenska",
      Language.Thai => "ไทย",
      Language.Turkish => "Türkçe",
      Language.Ukrainian => "Українська",
      Language.Vietnamese => "Tiếng Việt",
      Language.ChineseSimplified => "简体中文",
      Language.ChineseTraditional => "繁體中文",
      _ => string.Empty
    };
  }
}

}