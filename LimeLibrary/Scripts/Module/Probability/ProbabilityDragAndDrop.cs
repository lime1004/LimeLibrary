#if LIME_ODIN_INSPECTOR
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace LimeLibrary.Module {

#if UNITY_EDITOR
public abstract class ProbabilityDragAndDrop<T> : OdinEditor {
  private readonly List<T> _addContentList = new();

  public override void OnInspectorGUI() {
    var dropArea = GUILayoutUtility.GetRect(0.0f, GetRectHeight(), GUILayout.ExpandWidth(true));
    GUI.Box(dropArea, $"Add to drag {typeof(T)}", EditorStyles.helpBox);
    HandleDragAndDrop(dropArea, _addContentList);

    if (_addContentList.Count > 0) {
      foreach (var content in _addContentList) {
        AddProbabilityList(content);
      }
      _addContentList.Clear();
    }

    base.OnInspectorGUI();
  }

  protected virtual float GetRectHeight() {
    return 50f;
  }

  protected virtual Probability<T> CreateProbability(T content) {
    return new Probability<T>(content, false, 1);
  }

  protected virtual void AddProbabilityList(T content) {
    if (target is IHasProbabilityList<T> probabilityList) {
      var probability = CreateProbability(content);
      probabilityList.GetProbabilityList().Add(probability);
    }
  }

  private static void HandleDragAndDrop(Rect dropArea, ICollection<T> list) {
    var evt = UnityEngine.Event.current;
    switch (evt.type) {
    case EventType.DragUpdated:
    case EventType.DragPerform:
      if (!dropArea.Contains(evt.mousePosition))
        return;

      DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

      if (evt.type == EventType.DragPerform) {
        DragAndDrop.AcceptDrag();

        foreach (var draggedObject in DragAndDrop.objectReferences) {
          if (draggedObject is T addObject) {
            list.Add(addObject);
          }
        }
      }
      UnityEngine.Event.current.Use();
      break;
    }
  }
}
#endif

}
#endif