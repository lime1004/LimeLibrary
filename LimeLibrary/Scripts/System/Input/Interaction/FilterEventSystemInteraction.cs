using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if LIME_INPUTSYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.InputSystem.Editor;
#endif
#endif

#if LIME_INPUTSYSTEM
namespace LimeLibrary.System {

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class FilterEventSystemInteraction : IInputInteraction {
#if UNITY_EDITOR
  /// <summary>
  /// 静的コンストラクタ
  /// </summary>
  static FilterEventSystemInteraction() {
    Initialize();
  }
#endif

  /// <summary>
  /// こっちも必要
  /// ないと実機でエラーになる
  /// </summary>
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void Initialize() {
    // 登録が必要
    InputSystem.RegisterInteraction<FilterEventSystemInteraction>();
  }

  public bool m_isOnPress = true;
  public bool m_isOnRelease = true;
  public float m_pressPoint = 0.5f;

  public bool m_filterMouse = true;

  public bool m_filterButton = true;
  public bool m_filterImage = true;

  private float PressPointOrDefault => m_pressPoint;
  private float ReleasePointOrDefault => m_pressPoint;
  private bool m_waitingForRelease;

  public void Process(ref InputInteractionContext context) {
    float actuation = context.ComputeMagnitude();

    if (m_waitingForRelease) {
      if (actuation <= ReleasePointOrDefault) {
        m_waitingForRelease = false;
        if (m_isOnRelease && !IsOverlapUI(ref context)) context.Performed();
        context.Canceled();
      }
    } else if (actuation >= PressPointOrDefault) {
      m_waitingForRelease = true;
      // Stay performed until release.
      if (m_isOnPress && !IsOverlapUI(ref context)) context.PerformedAndStayPerformed();
    } else if (actuation > 0 && !context.isStarted) {
      if (m_isOnPress && !IsOverlapUI(ref context)) context.Started();
    }
  }

  public void Reset() { }

  /// <summary>
  /// UIと重複しているか
  /// </summary>
  /// <param name="context"></param>
  /// <returns></returns>
  private bool IsOverlapUI(ref InputInteractionContext context) {
    var currentEventSystem = EventSystem.current;
    if (!m_filterMouse) return false;

    // レイを飛ばして特定のuGUIのみ判定
    var pointerEventData = new PointerEventData(currentEventSystem) {
      position = ((InputSystemUIInputModule) currentEventSystem.currentInputModule).point.action.ReadValue<Vector2>()
    };
    var results = new List<RaycastResult>();
    currentEventSystem.RaycastAll(pointerEventData, results);
    foreach (var result in results) {
      if (m_filterButton) {
        if (result.gameObject.TryGetComponent<Button>(out var _) ||
          result.gameObject.transform.parent && result.gameObject.transform.parent.TryGetComponent<Button>(out var _)) {
          return true;
        }
      }
      if (m_filterImage && result.gameObject.TryGetComponent<Image>(out var _)) {
        return true;
      }
    }
    return false;
  }
}

// 同じクラス内に定義するとパラメータ変更が反映されなくなる
#if UNITY_EDITOR
public class FilterEventSystemInteractionEditor : InputParameterEditor<FilterEventSystemInteraction> {
  protected override void OnEnable() { }

  public override void OnGUI() {
    EditorGUILayout.LabelField("Timing Controls");
    target.m_isOnPress = EditorGUILayout.Toggle("OnPress", target.m_isOnPress);
    target.m_isOnRelease = EditorGUILayout.Toggle("OnRelease", target.m_isOnRelease);
    EditorGUILayout.Separator();
    EditorGUILayout.LabelField("Target Controls");
    target.m_filterMouse = EditorGUILayout.Toggle("Mouse", target.m_filterMouse);
    EditorGUILayout.Separator();
    EditorGUILayout.LabelField("Target Components");
    target.m_filterButton = EditorGUILayout.Toggle("Button", target.m_filterButton);
    target.m_filterImage = EditorGUILayout.Toggle("Image", target.m_filterImage);
  }
}
#endif

}
#endif