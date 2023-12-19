using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LimeLibrary.Attribute {

public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    EditorGUI.BeginDisabledGroup(true);
    EditorGUI.PropertyField(position, property, label);
    EditorGUI.EndDisabledGroup();
  }
}
#endif

}