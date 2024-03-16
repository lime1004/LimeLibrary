using System;
using System.Collections.Generic;
using LimeLibrary.Input;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine.InputSystem;

namespace LimeLibrary.UI.Module.Input {

public class UIInputReceiver : IDisposable {
  private readonly IUI _parentUI;
  private readonly InputAction _inputAction;
  private readonly Subject<InputAction.CallbackContext> _onInputSubject = new();

  public bool Enabled { get; set; } = true;
  public IObservable<InputAction.CallbackContext> OnInputObservable => _onInputSubject;

  public UIInputReceiver(IUI parentUI, InputInteractionType inputInteractionType, int? behaviourType = null) :
    this(parentUI, InputInteractionBuilder.GetInteractions(inputInteractionType, behaviourType)) { }

  public UIInputReceiver(IUI parentUI, string interactions = "") {
    if (parentUI == null) {
      Assertion.Assert(false, "ParentUI is null.");
      return;
    }

    _parentUI = parentUI;

    // 親Viewの表示非表示にInputの有効化/無効化を行う
    parentUI.OnShowEndObservable.Subscribe(_ => EnableInputAction()).AddTo(parentUI.RootObject);
    parentUI.OnHideEndObservable.Subscribe(_ => DisableInputAction()).AddTo(parentUI.RootObject);
    parentUI.RootObject.OnDestroyAsObservable().FirstOrDefault().Subscribe(_ => Dispose());

    _inputAction = new InputAction("UIInputReceiver", InputActionType.Button, interactions: interactions);
    EnableInputAction();

    // Input時処理登録
    _inputAction.performed += context => {
      if (!IsEnable()) return;
      _onInputSubject.OnNext(context);
    };
  }

  public void Dispose() {
    _onInputSubject.OnCompleted();
    _inputAction.Dispose();
  }

  public void AddInputBinding(string inputBindingPath, string interactions = "") {
    _inputAction.AddBinding(inputBindingPath, interactions);
  }

  public void AddInputBinding(IReadOnlyList<string> inputBindingPaths, string interactions = "") {
    for (int i = 0; i < inputBindingPaths.Count; i++) {
      _inputAction.AddBinding(inputBindingPaths[i], interactions);
    }
  }

  public void AddInputBinding(InputBindingType inputBindingType, string interactions = "") {
    AddInputBinding(InputBindingPath.Get(inputBindingType), interactions);
  }

  public void AddInputBinding(InputAction inputAction, string interactions = "") {
    var bindingPaths = inputAction.GetInputBindingPaths();
    foreach (string bindingPath in bindingPaths) {
      AddInputBinding(bindingPath, interactions);
    }
  }

  public void EnableInputAction() => _inputAction.Enable();
  public void DisableInputAction() => _inputAction.Disable();

  private bool IsEnable() {
    if (!Enabled) return false;
    if (_parentUI is null) return false;
    if (_parentUI is IUIView view && !view.IsEnable()) return false;

    return true;
  }
}

}