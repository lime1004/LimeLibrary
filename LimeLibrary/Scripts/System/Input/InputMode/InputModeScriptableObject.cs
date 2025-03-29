#if LIME_INPUTSYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

namespace LimeLibrary.Input.InputMode {

public abstract class InputModeScriptableObject : ScriptableObject, IInputMode {
  [SerializeField]
  private bool _isSelectOnChangeInputMode;
  [SerializeField]
  private bool _isSelectOnFocusUIView;
  [SerializeField]
  private bool _isDeselectOnUnfocusUIView;

  public abstract string Name { get; }
  public bool IsSelectOnChangeInputMode => _isSelectOnChangeInputMode;
  public bool IsSelectOnFocusUIView => _isSelectOnFocusUIView;
  public bool IsDeselectOnUnfocusUIView => _isDeselectOnUnfocusUIView;

  public abstract bool CheckChangeInputMode(InputDevice inputDevice);

  public abstract void OnEnterInputMode();
  public abstract void OnExitInputMode();

  public abstract void OnUpdate();
  public abstract void OnLateUpdate();

  public abstract string[] GetControlPaths();

}

}
#endif