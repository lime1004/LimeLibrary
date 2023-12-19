using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LimeLibrary.Attribute {

/// <summary>
/// InspectorにButtonを表示するAttribute
/// </summary>
public class ButtonAttribute : PropertyAttribute {
  public string MethodName { get; }

  public ButtonAttribute(string methodName) {
    MethodName = methodName;
  }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ButtonAttribute))]
public class ButtonAttributeDrawer : PropertyDrawer {
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    if (!GUI.Button(position, label)) return;

    var buttonAttribute = (ButtonAttribute) attribute;
    var method = property.serializedObject.targetObject.GetType().GetMethod(buttonAttribute.MethodName);
    if (method != null) {
      method.Invoke(property.serializedObject.targetObject, null);
    }
  }
}
#endif

}