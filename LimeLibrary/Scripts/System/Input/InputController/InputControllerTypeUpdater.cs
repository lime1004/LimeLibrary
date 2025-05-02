using LimeLibrary.Attributes;
using LimeLibrary.Module;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LimeLibrary.Input.InputController {

#if LIME_INPUTSYSTEM
/// <summary>
/// 入力デバイスの種類を更新するクラス
/// </summary>
public class InputControllerTypeUpdater : SingletonMonoBehaviour<InputControllerTypeUpdater> {
  [SerializeField, ReadOnly]
  private InputControllerType _controllerType;
  [SerializeField, ReadOnly]
  private string _inputGamepadName;

  public InputControllerType ControllerType => _controllerType;

  public void LateUpdate() {
    UpdateControllerType();
  }

  private void UpdateControllerType() {
    if (Gamepad.current == null) {
      _controllerType = InputControllerType.None;
      return;
    }

    if (_inputGamepadName == Gamepad.current.name) return;

    _inputGamepadName = Gamepad.current.name;
    switch (_inputGamepadName) {
    case "XInputControllerWindows":
      _controllerType = InputControllerType.XInputController;
      break;
    case "DualShock4GamepadHID":
      _controllerType = InputControllerType.DualShockGamepad;
      break;
    case "SwitchProControllerHID":
      _controllerType = InputControllerType.SwitchProController;
      break;
    default: {
      if (_inputGamepadName.Contains("XInput")) {
        _controllerType = InputControllerType.XInputController;
      } else if (_inputGamepadName.Contains("DualShock")) {
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