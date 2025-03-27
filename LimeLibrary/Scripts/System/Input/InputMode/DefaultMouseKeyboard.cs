#if LIME_INPUTSYSTEM
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LimeLibrary.Input.InputMode {

[Serializable, CreateAssetMenu(
   fileName = "DefaultMouseKeyboard",
   menuName = "LimeLibrary/Input/InputMode/DefaultMouseKeyboard")]
public class DefaultMouseKeyboard : InputModeScriptableObject {
  public override string Name => nameof(DefaultMouseKeyboard);

  public override bool CheckChangeInputMode(InputDevice inputDevice) {
    // NOTE: マウスの反応は3ボタンのクリックだけとる
    bool isMouse = inputDevice is Mouse mouse && (
      mouse.leftButton.wasPressedThisFrame ||
      mouse.rightButton.wasPressedThisFrame ||
      mouse.middleButton.wasPressedThisFrame);

    bool isKeyboard = inputDevice is Keyboard;

    return isMouse || isKeyboard;
  }

  public override void OnEnterInputMode() { }
  public override void OnExitInputMode() { }

  public override string[] GetControlPaths() {
    return new[] {
      nameof(Mouse),
      nameof(Keyboard),
    };
  }
}

}
#endif