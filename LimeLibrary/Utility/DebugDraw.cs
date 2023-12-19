using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LimeLibrary.Utility {

public static class DebugDraw {
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
  [Conditional("NEVER_DEFINED_SYMBOL")]
#endif
  public static void DrawRect(Rect rect) {
    DrawRect(rect, Color.white);
  }

#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
  [Conditional("NEVER_DEFINED_SYMBOL")]
#endif
  public static void DrawRect(Rect rect, Color color) {
    Debug.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMax, rect.yMin), color);
    Debug.DrawLine(new Vector3(rect.xMax, rect.yMin), new Vector3(rect.xMax, rect.yMax), color);
    Debug.DrawLine(new Vector3(rect.xMax, rect.yMax), new Vector3(rect.xMin, rect.yMax), color);
    Debug.DrawLine(new Vector3(rect.xMin, rect.yMax), new Vector3(rect.xMin, rect.yMin), color);
  }
}

}