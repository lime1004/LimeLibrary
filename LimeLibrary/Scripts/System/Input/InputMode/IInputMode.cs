#if LIME_INPUTSYSTEM
using UnityEngine.InputSystem;

namespace LimeLibrary.Input.InputMode {

public interface IInputMode {
  public string Name { get; }
  public bool IsSelectOnChangeInputMode { get; }
  public bool IsSelectOnFocusUIView { get; }
  public bool IsDeselectOnUnfocusUIView { get; }

  public bool CheckChangeInputMode(InputDevice inputDevice);

  public void OnEnterInputMode();
  public void OnExitInputMode();

  public string[] GetControlPaths();
}

}
#endif