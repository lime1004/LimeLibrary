#if LIME_INPUTSYSTEM
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LimeLibrary.Input.InputMode {

[Serializable, CreateAssetMenu(
   fileName = "DefaultGamepad",
   menuName = "LimeLibrary/Input/InputMode/DefaultGamepad")]
public class DefaultGamepad : InputModeScriptableObject {
  [SerializeField]
  private bool _isHideCursor;

  public override string Name => nameof(DefaultGamepad);

  public override bool CheckChangeInputMode(InputDevice inputDevice) {
    return inputDevice is Gamepad;
  }

  public override void OnEnterInputMode() {
    if (_isHideCursor) {
      Cursor.visible = false;
    }
  }

  public override void OnExitInputMode() { }

  public override void OnUpdate() { }
  public override void OnLateUpdate() { }

  public override string[] GetControlPaths() {
    return new[] {
      nameof(Gamepad),
    };
  }
}

}
#endif