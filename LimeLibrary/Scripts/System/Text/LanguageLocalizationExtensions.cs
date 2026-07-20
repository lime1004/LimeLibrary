#if LIME_LOCALIZATION
using System;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace LimeLibrary.Text {

/// <summary>
/// LanguageとUnity LocalizationのLocaleを結び付ける拡張メソッド
/// </summary>
public static class LanguageLocalizationExtensions {
  /// <summary>
  /// 対応するLocaleを取得する（見つからない場合はnull）
  /// </summary>
  public static Locale ToLocale(this Language language) {
    // NOTE: Localization初期化前に呼ばれるとAvailableLocales/Localesがnullになりうる
    var locales = LocalizationSettings.AvailableLocales?.Locales;
    if (locales == null) return null;
    string code = language.ToISO639Code();
    return locales.Find(
      locale => locale != null && string.Compare(locale.Identifier.Code, code, StringComparison.OrdinalIgnoreCase) == 0);
  }

  /// <summary>
  /// 対応するLocaleが存在するか
  /// </summary>
  public static bool IsAvailable(this Language language) {
    return language.ToLocale() != null;
  }
}

}
#endif
