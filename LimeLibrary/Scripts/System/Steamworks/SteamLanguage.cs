using LimeLibrary.Text;
using Steamworks;

namespace LimeLibrary.Steam {

public static class SteamLanguage {
  public static Language? GetLanguage() {
    if (!SteamLifecycle.Initialized) return null;
    return LanguageUtility.ConvertFromSteamLanguage(SteamUtils.GetSteamUILanguage());
  }
}

}