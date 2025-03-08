using System.Threading;
using LimeLibrary.Attribute;
using LimeLibrary.Module;
using LimeLibrary.Utility;
using UnityEngine;

#if LIME_R3
using R3;
#endif

#if LIME_UNITASK
using Cysharp.Threading.Tasks;
#endif

#if LIME_INPUTSYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
#endif

namespace LimeLibrary.Input {

#if LIME_INPUTSYSTEM && LIME_UNITASK
/// <summary>
/// 入力デバイスの種類を更新するクラス
/// </summary>
public class InputModeUpdater : SingletonMonoBehaviour<InputModeUpdater> {
  [SerializeField, ReadOnly]
  private InputMode _inputMode;
  [SerializeField, ReadOnly]
  private ControllerType _controllerType;
  [SerializeField, ReadOnly]
  private string _inputGamepadName;

  private Channel<InputControl> _onUseDeviceChannel;

  public InputMode InputMode => _inputMode;
  public ControllerType ControllerType => _controllerType;

#if LIME_R3
  private readonly Subject<InputMode> _onChangeInputModeSubject = new();
  private readonly Subject<InputDevice> _onUseDeviceSubject = new();
  public Observable<InputMode> OnChangeInputModeObservable => _onChangeInputModeSubject;
  public Observable<InputDevice> OnUseDeviceObservable => _onUseDeviceSubject;
#endif

  protected override void Awake() {
    if (Gamepad.current != null) {
      _inputMode = InputMode.Gamepad;
    }

    InputUser.CreateUserWithoutPairedDevices();
    ++InputUser.listenForUnpairedDeviceActivity;
    InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsedCallback;

    RunUseDeviceEvent(gameObject.GetCancellationTokenOnDestroy()).Forget();
  }

  private void OnDestroy() {
    --InputUser.listenForUnpairedDeviceActivity;
    InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsedCallback;
  }

  private void OnUnpairedDeviceUsedCallback(InputControl inputControl, InputEventPtr inputEventPtr) {
    _onUseDeviceChannel.Writer.TryWrite(inputControl);
  }

  private async UniTaskVoid RunUseDeviceEvent(CancellationToken cancellationToken) {
    _onUseDeviceChannel = Channel.CreateSingleConsumerUnbounded<InputControl>();

    await foreach (var inputControl in _onUseDeviceChannel.Reader.ReadAllAsync(cancellationToken)) {
      // NOTE: ボタンの押下判定などが正確に取れないため、PlayerLoopTiming.Updateまで待つ
      await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
      OnUnpairedDeviceUsed(inputControl);
    }
  }

  private void OnUnpairedDeviceUsed(InputControl inputControl) {
    var device = inputControl.device;
    switch (device) {
    case Mouse mouse:
      // NOTE: マウスの反応は3ボタンのクリックだけとる
      if (!mouse.leftButton.wasPressedThisFrame &&
        !mouse.rightButton.wasPressedThisFrame &&
        !mouse.middleButton.wasPressedThisFrame) break;
      if (_inputMode != InputMode.MouseKeyboard) {
        ChangeToMouseKeyboard();
      }
      break;
    case Keyboard:
      if (_inputMode != InputMode.MouseKeyboard) {
        ChangeToMouseKeyboard();
      }
      break;
    case Gamepad:
      if (_inputMode != InputMode.Gamepad) {
        ChangeToGamePad();
      }
      break;
    }

#if LIME_R3
    _onUseDeviceSubject.OnNext(device);
#endif
  }

  private void LateUpdate() {
    switch (_inputMode) {
    case InputMode.MouseKeyboard: {
      break;
    }
    case InputMode.Gamepad: {
      UpdateControllerType();
      break;
    }
    default:
      Assertion.Assert(false);
      break;
    }
  }

  private void ChangeToGamePad() {
    Cursor.visible = false;
    UpdateControllerType();
    _inputMode = InputMode.Gamepad;
#if LIME_R3
    _onChangeInputModeSubject.OnNext(InputMode.Gamepad);
#endif
  }

  private void ChangeToMouseKeyboard() {
    Cursor.visible = true;
    _inputMode = InputMode.MouseKeyboard;
    _controllerType = ControllerType.None;
    _inputGamepadName = string.Empty;
#if LIME_R3
    _onChangeInputModeSubject.OnNext(InputMode.MouseKeyboard);
#endif
  }

  private void UpdateControllerType() {
    if (Gamepad.current == null) {
      _controllerType = ControllerType.None;
      return;
    }

    if (_inputGamepadName == Gamepad.current.name) return;

    _inputGamepadName = Gamepad.current.name;
    switch (_inputGamepadName) {
    case "XInputControllerWindows":
      _controllerType = ControllerType.XInputController;
      break;
    case "DualShock4GamepadHID":
      _controllerType = ControllerType.DualShockGamepad;
      break;
    case "SwitchProControllerHID":
      _controllerType = ControllerType.SwitchProController;
      break;
    default: {
      if (_inputGamepadName.Contains("XInput")) {
        _controllerType = ControllerType.XInputController;
      } else if (_inputGamepadName.Contains("DualShock")) {
        _controllerType = ControllerType.DualShockGamepad;
      } else if (_inputGamepadName.Contains("Switch")) {
        _controllerType = ControllerType.SwitchProController;
      } else {
        _controllerType = ControllerType.XInputController;
      }
      break;
    }
    }
  }
}
#endif

}