using LimeLibrary.Attributes;
using LimeLibrary.Module;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace LimeLibrary.Input.InputController {

#if LIME_INPUTSYSTEM
/// <summary>
/// 入力デバイスの種類を更新するクラス
/// </summary>
public class InputControllerTypeUpdater : SingletonMonoBehaviour<InputControllerTypeUpdater> {
  // NOTE: 8bitスティックの静止時の揺れ（±1/127程度）を弾き、デッドゾーン既定値0.125も上回る生値の閾値
  private const float GamepadActuationThreshold = 0.2f;

  [SerializeField, ReadOnly]
  private InputControllerType _controllerType;
  [SerializeField, ReadOnly]
  private string _inputGamepadName;

  private Gamepad _operatedGamepad;

  public InputControllerType ControllerType => _controllerType;

  protected override void Awake() {
    base.Awake();

    InputSystem.onEvent += OnEventCallback;
  }

  private void OnDestroy() {
    InputSystem.onEvent -= OnEventCallback;
  }

  public void LateUpdate() {
    UpdateControllerType();
  }

  private void OnEventCallback(InputEventPtr inputEventPtr, InputDevice inputDevice) {
    if (inputDevice is not Gamepad gamepad) return;
#if UNITY_EDITOR
    // NOTE: エディタウィンドウ向けの更新で流れるイベントはゲームの操作ではないため除外する
    if (InputState.currentUpdateType == InputUpdateType.Editor) return;
#endif
    // NOTE: EnumerateChangedControlsはStateEvent/DeltaStateEvent以外を渡すと例外になるため先に弾く
    if (!inputEventPtr.IsA<StateEvent>() && !inputEventPtr.IsA<DeltaStateEvent>()) return;
    // NOTE: 無効化中のデバイスのイベントも通知されるため除外する
    if (!gamepad.enabled) return;
    if (!HasActuation(gamepad, inputEventPtr)) return;

    _operatedGamepad = gamepad;
  }

  // NOTE: ゲームパッドはセンサー等のノイズだけでもイベントが流れてくるので、実際に操作されたコントロールがあるかを見る
  private static bool HasActuation(Gamepad gamepad, InputEventPtr inputEventPtr) {
    foreach (var inputControl in inputEventPtr.EnumerateChangedControls(gamepad)) {
      // NOTE: 値を評価できない型は操作の裏付けが取れないため通さない
      if (inputControl is not InputControl<float> valueControl) continue;

      // NOTE: このコールバックはイベントのデバイス反映前に呼ばれ、かつデッドゾーン設定に左右させたくないのでイベントの生値を読む
      float value = valueControl.ReadUnprocessedValueFromEvent(inputEventPtr);
      if (inputControl is ButtonControl buttonControl) {
        if (buttonControl.IsValueConsideredPressed(value)) return true;
        continue;
      }

      if (Mathf.Abs(value) >= GamepadActuationThreshold) return true;
    }

    return false;
  }

  // NOTE: ノイズを吐き続けるパッドにGamepad.currentを奪われるため、操作実績のあるパッドを優先し、まだ操作が無い間だけcurrentで補う
  private Gamepad GetInputGamepad() {
    if (IsUsableGamepad(_operatedGamepad)) return _operatedGamepad;

    _operatedGamepad = null;
    return IsUsableGamepad(Gamepad.current) ? Gamepad.current : null;
  }

  private static bool IsUsableGamepad(Gamepad gamepad) => gamepad is { added: true, enabled: true };

  private void UpdateControllerType() {
    var gamepad = GetInputGamepad();
    if (gamepad == null) {
      _controllerType = InputControllerType.None;
      // NOTE: 再接続時に名前一致で更新が飛ばされないようクリアする
      _inputGamepadName = null;
      return;
    }

    if (_inputGamepadName == gamepad.name) return;

    _inputGamepadName = gamepad.name;
    switch (_inputGamepadName) {
    case "XInputControllerWindows":
      _controllerType = InputControllerType.XInputController;
      break;
    case "DualShock4GamepadHID":
    case "DualSenseGamepadHID":
      _controllerType = InputControllerType.DualShockGamepad;
      break;
    case "SwitchProControllerHID":
      _controllerType = InputControllerType.SwitchProController;
      break;
    default: {
      if (_inputGamepadName.Contains("XInput")) {
        _controllerType = InputControllerType.XInputController;
      } else if (_inputGamepadName.Contains("DualShock") || _inputGamepadName.Contains("DualSense")) {
        _controllerType = InputControllerType.DualShockGamepad;
      } else if (_inputGamepadName.Contains("Switch")) {
        _controllerType = InputControllerType.SwitchProController;
      } else {
        _controllerType = InputControllerType.XInputController;
      }
      break;
    }
    }
  }
}
#endif

}