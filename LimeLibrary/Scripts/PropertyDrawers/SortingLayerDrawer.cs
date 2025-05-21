using LimeLibrary.Attributes;
using UnityEngine;
using UnityEditor;

namespace LimeLibrary.PropertyDrawers {

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SortingLayerAttribute))]
public class SortingLayerDrawer : PropertyDrawer {
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    if (property.propertyType == SerializedPropertyType.Integer) {
      string[] layerNames = GetSortingLayerNames();
      int currentLayerID = property.intValue;
      int selectedIndex = Mathf.Max(0, System.Array.IndexOf(GetSortingLayerIDs(), currentLayerID));

      int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, layerNames);
      property.intValue = GetSortingLayerIDs()[newIndex];
    } else {
      EditorGUI.LabelField(position, label.text, "Use [SortingLayer] with int.");
    }
  }

  private static string[] GetSortingLayerNames() {
    var layers = SortingLayer.layers;
    string[] names = new string[layers.Length];
    for (int i = 0; i < layers.Length; i++) {
      names[i] = layers[i].name;
    }
    return names;
  }

  private static int[] GetSortingLayerIDs() {
    var layers = SortingLayer.layers;
    int[] ids = new int[layers.Length];
    for (int i = 0; i < layers.Length; i++) {
      ids[i] = layers[i].id;
    }
    return ids;
  }
}
#endif

}