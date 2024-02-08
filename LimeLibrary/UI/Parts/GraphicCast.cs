using UnityEditor;
using UnityEngine.UI;

namespace LimeLibrary.UI.Parts {

public class GraphicCast : Graphic {
  protected override void OnPopulateMesh(VertexHelper vhelper) {
    base.OnPopulateMesh(vhelper);
    vhelper.Clear();
  }

#if UNITY_EDITOR
  [CustomEditor(typeof(GraphicCast))]
  private class GraphicCastEditor : Editor {
    public override void OnInspectorGUI() { }
  }
#endif
}

}