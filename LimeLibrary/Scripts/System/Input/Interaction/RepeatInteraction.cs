using UnityEngine;
#if LIME_INPUTSYSTEM
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif
#endif

namespace LimeLibrary.System {

#if LIME_INPUTSYSTEM
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class RepeatInteraction : IInputInteraction {
#if UNITY_EDITOR
  static RepeatInteraction() {
    Initialize();
  }
#endif

  [RuntimeInitializeOnLoadMethod]
  private static void Initialize() {
    InputSystem.RegisterInteraction<RepeatInteraction>();
  }

  [Tooltip("連射開始時間（秒）")]
  public float m_repeatSeconds = 0.5f;
  [Tooltip("連射間隔")]
  public float m_repeatDuration = 0.2f;
  [Tooltip("ボタンを押したと判断するしきい値")]
  public float m_pressPoint = 0.5f;

  public void Process(ref InputInteractionContext context) {
    if (context.timerHasExpired) {
      context.Canceled();
      if (context.ControlIsActuated(m_pressPoint)) {
        context.Started();
        context.Performed();
        context.SetTimeout(m_repeatDuration);
      }
      return;
    }

    switch (context.phase) {
    case InputActionPhase.Waiting:
      if (context.ControlIsActuated(m_pressPoint)) {
        context.Started();
        context.Performed();
        context.SetTimeout(m_repeatSeconds);
      }
      break;

    case InputActionPhase.Performed:
      if (!context.ControlIsActuated(m_pressPoint)) {
        context.Canceled();
        return;
      }

      break;
    }
  }

  public void Reset() { }
}
#endif

}