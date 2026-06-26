using System;
using System.Collections.Generic;
using UnityEngine;

namespace LimeLibrary.Platform {

[Serializable]
public class PlatformSetting {
  [SerializeField]
  private List<string> _detectCommandLineArgs = new();
  [SerializeReference]
  private IPlatformInitializer _initializer;

  public void Initialize(GameObject parent) {
    _initializer?.Initialize(parent);
  }

  public bool Match(string arg) {
    if (string.IsNullOrWhiteSpace(arg) || _detectCommandLineArgs == null) return false;

    foreach (string detectCommandLineArg in _detectCommandLineArgs) {
      if (string.Equals(detectCommandLineArg?.Trim(), arg, StringComparison.OrdinalIgnoreCase)) {
        return true;
      }
    }

    return false;
  }
}

}