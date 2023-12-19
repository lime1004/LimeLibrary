using System.Diagnostics;

namespace LimeLibrary.Utility {

public static class Assertion {
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
  [Conditional("NEVER_DEFINED_SYMBOL")]
#endif
  public static void Assert(bool condition, object context = null) {
    if (!condition) {
      UnityEngine.Debug.Assert(condition, context);
    }
  }
}

}