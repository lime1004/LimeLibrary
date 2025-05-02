using UnityEditor;
using UnityEngine;

namespace LimeLibrary.Attributes {

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