#if LIME_INPUTSYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

namespace LimeLibrary.Input.Interaction {

public class RepeatInteraction : IInputInteraction {
#if UNITY_EDITOR
  [UnityEditor.InitializeOnLoadMethod]
#else
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
  private static void Initialize() {
    InputSystem.RegisterInteraction<RepeatInteraction>();
  }

  [Tooltip("ボタンが最初に押されてからリピート処理が始まるまでの時間（秒）")]
  public float _repeatDelay = 0.5f;
  [Tooltip("ボタンが押されている間のリピート処理の間隔")]
  public float _repeatInterval = 0.2f;
  [Tooltip("ボタンを押したと判断するしきい値")]
  public float _pressPoint = 0.5f;

  // 設定値かデフォルト値の値を格納するフィールド
  private float PressPointOrDefault => _pressPoint > 0 ? _pressPoint : InputSystem.settings.defaultButtonPressPoint;
  private float ReleasePointOrDefault => PressPointOrDefault * InputSystem.settings.buttonReleaseThreshold;

  // 次のリピート時刻
  private double _nextRepeatSeconds;

  public void Process(ref InputInteractionContext context) {
    if (_repeatDelay <= 0 || _repeatInterval <= 0) {
      Utility.Logger.LogError("RepeatInteraction: _repeatDelay and _repeatInterval must be greater than 0.");
      return;
    }

    if (context.timerHasExpired) {
      // リピート時刻に達したら再びPerformedに遷移
      if (context.time >= _nextRepeatSeconds) {
        // リピート処理の次回実行時刻を設定
        _nextRepeatSeconds = context.time + _repeatInterval;
        // リピート時の処理
        context.PerformedAndStayPerformed();
        // 次のリピート時刻にProcessメソッドが呼ばれるようにタイムアウトを設定
        context.SetTimeout(_repeatInterval);
      }
      return;
    }

    switch (context.phase) {
    case InputActionPhase.Waiting:
      // ボタンが押されたらStartedに遷移
      if (context.ControlIsActuated(PressPointOrDefault)) {
        // ボタンが押された時の処理
        context.Started();
        context.PerformedAndStayPerformed();

        // リピート処理の初回実行時刻を設定
        _nextRepeatSeconds = context.time + _repeatDelay;
        // 次のリピート時刻にProcessメソッドが呼ばれるようにタイムアウトを設定
        context.SetTimeout(_repeatDelay);
      }
      break;

    case InputActionPhase.Performed:
      // ボタンが離されたらCanceledに遷移
      if (!context.ControlIsActuated(ReleasePointOrDefault)) {
        // ボタンが離された時の処理
        context.Canceled();
      }
      break;
    }
  }

  public void Reset() {
    _nextRepeatSeconds = 0;
  }
}

}

#endif