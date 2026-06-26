using System;
using System.Collections.Generic;
using LimeLibrary.Module;
using UnityEngine;

namespace LimeLibrary.Platform {

public class PlatformDetector : SingletonMonoBehaviour<PlatformDetector> {
  [SerializeField]
  private string StoreArgumentName = "store";
  [SerializeField]
  private List<PlatformSetting> _platformSettings = new();
#if UNITY_EDITOR
  [SerializeField]
  private string _editorPlatformName = string.Empty;
#endif

  public string PlatformName { get; private set; } = string.Empty;
  public PlatformStoreSource DetectedSource { get; private set; } = PlatformStoreSource.None;

  protected override void Awake() {
    base.Awake();

    (PlatformName, DetectedSource) = DetectPlatform();

    if (_platformSettings == null) return;

    foreach (var platformSetting in _platformSettings) {
      if (platformSetting == null) continue;
      if (platformSetting.Match(PlatformName)) {
        platformSetting.Initialize(gameObject);
      }
    }
  }

  private (string PlatformName, PlatformStoreSource Source) DetectPlatform() {
    if (TryGetCommandLinePlatform(out string commandLinePlatform)) {
      return (commandLinePlatform, PlatformStoreSource.CommandLine);
    }

#if UNITY_EDITOR
    if (TryGetEditorOverridePlatform(out string editorPlatform)) {
      return (editorPlatform, PlatformStoreSource.EditorOverride);
    }
#endif

    return (string.Empty, PlatformStoreSource.None);
  }

  private bool TryGetCommandLinePlatform(out string platformName) {
    string[] args = Environment.GetCommandLineArgs();
    for (int i = 0; i < args.Length; i++) {
      string arg = args[i];

      if (IsStoreArgument(arg) && i + 1 < args.Length) {
        return TryNormalizePlatformName(args[i + 1], out platformName);
      }

      string prefix = "-" + StoreArgumentName + "=";
      if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
        return TryNormalizePlatformName(arg.Substring(prefix.Length), out platformName);
      }

      prefix = "--" + StoreArgumentName + "=";
      if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
        return TryNormalizePlatformName(arg.Substring(prefix.Length), out platformName);
      }
    }

    platformName = string.Empty;
    return false;
  }

#if UNITY_EDITOR
  private bool TryGetEditorOverridePlatform(out string platformName) =>
    TryNormalizePlatformName(_editorPlatformName, out platformName);
#endif

  private bool IsStoreArgument(string arg) {
    return
      string.Equals(arg, "-" + StoreArgumentName, StringComparison.OrdinalIgnoreCase) ||
      string.Equals(arg, "--" + StoreArgumentName, StringComparison.OrdinalIgnoreCase);
  }

  private static bool TryNormalizePlatformName(string value, out string platformName) {
    platformName = value?.Trim().ToLowerInvariant() ?? string.Empty;
    return !string.IsNullOrWhiteSpace(platformName);
  }
}

}
