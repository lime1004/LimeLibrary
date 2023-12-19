using System.Diagnostics;

namespace LimeLibrary.Utility {

public static class Logger {
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
  [Conditional("NEVER_DEFINED_SYMBOL")]
#endif
  public static void Log(object context) {
    UnityEngine.Debug.Log(context);
  }

#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
  [Conditional("NEVER_DEFINED_SYMBOL")]
#endif
  public static void LogWarning(object context) {
    UnityEngine.Debug.LogWarning(context);
  }

#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
  [Conditional("NEVER_DEFINED_SYMBOL")]
#endif
  public static void LogError(object context) {
    UnityEngine.Debug.LogError(context);
  }
}

}