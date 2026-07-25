#if LIME_LOCALIZATION
using System;
using FastEnumUtility;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace LimeLibrary.Text {

/// <summary>
/// LanguageとUnity LocalizationのLocaleを結び付ける拡張メソッド
/// </summary>
public static class LanguageLocalizationExtensions {
  /// <summary>
  /// LanguageとLocaleが対応しているか（ToISO639CodeとIdentifier.Codeの大文字小文字を無視した完全一致。zh-Hans等の別表記は不一致）
  /// </summary>
  public static bool IsMatch(this Language language, Locale locale) {
    if (locale == null) return false;
    return string.Compare(locale.Identifier.Code, language.ToISO639Code(), StringComparison.OrdinalIgnoreCase) == 0;
  }

  /// <summary>
  /// 対応するLocaleを取得する（見つからない場合はnull）
  /// </summary>
  public static Locale ToLocale(this Language language) {
    // NOTE: Localization初期化前に呼ばれるとAvailableLocales/Localesがnullになりうる
    var locales = LocalizationSettings.AvailableLocales?.Locales;
    if (locales == null) return null;
    return locales.Find(locale => language.IsMatch(locale));
  }

  /// <summary>
  /// 対応するLocaleが存在するか
  /// </summary>
  public static bool IsAvailable(this Language language) {
    return language.ToLocale() != null;
  }

  /// <summary>
  /// 対応するLanguageを取得する（見つからない場合はnull）
  /// </summary>
  public static Language? ToLanguage(this Locale locale) {
    if (locale == null) return null;
    foreach (var language in FastEnum.GetValues<Language>()) {
      if (language.IsMatch(locale)) return language;
    }
    return null;
  }
}

}
#endif