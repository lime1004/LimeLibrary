using System;
using System.Collections.Generic;
using LimeLibrary.Input;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using R3;
using R3.Triggers;
using UnityEngine.InputSystem;

namespace LimeLibrary.UI.Module.Input {

public class UIInputReceiver : IDisposable {
  private readonly IUI _parentUI;
  private readonly InputAction _inputAction;
  private readonly Subject<InputAction.CallbackContext> _onInputSubject = new();
  private readonly List<InputBindingRegistration> _bindingRegistrationList = new();

  private bool _isRebuilding;

  public bool Enabled { get; set; } = true;
  public Observable<InputAction.CallbackContext> OnInputObservable => _onInputSubject;

  public Observable<TValue> OnValueObservable<TValue>() where TValue : struct {
    return Observable.EveryUpdate().Where(_ => IsEnable() && _inputAction.enabled).Select(_ => _inputAction.ReadValue<TValue>());
  }

  public UIInputReceiver(IUI parentUI, InputInteractionType inputInteractionType, int? behaviourType = null) :
    this(parentUI, InputActionType.Button, InputInteractionBuilder.GetInteractions(inputInteractionType, behaviourType)) { }

  public UIInputReceiver(IUI parentUI, InputActionType inputActionType = InputActionType.Button, string interactions = "") {
    if (parentUI == null) {
      Assertion.Assert(false, "ParentUI is null.");
      return;
    }

    _parentUI = parentUI;

    // 親Viewの表示非表示にInputの有効化/無効化を行う
    parentUI.OnShowEndObservable.Subscribe(_ => EnableInputAction()).AddTo(parentUI.RootObject);
    parentUI.OnHideEndObservable.Subscribe(_ => DisableInputAction()).AddTo(parentUI.RootObject);
    parentUI.RootObject.OnDestroyAsObservable().Take(1).Subscribe(_ => Dispose());

    _inputAction = new InputAction("UIInputReceiver", inputActionType, interactions: interactions);
    EnableInputAction();

    // Input時処理登録
    _inputAction.performed += context => {
      if (!IsEnable()) return;
      _onInputSubject.OnNext(context);
    };

    // リバインドによるバインディング変更に追随させる
    InputSystem.onActionChange += OnActionChange;
  }

  public void Dispose() {
    InputSystem.onActionChange -= OnActionChange;
    _onInputSubject.OnCompleted();
    _inputAction.Dispose();
  }

  public void AddInputBinding(string inputBindingPath, string interactions = "") {
    AddInputBinding(new[] { inputBindingPath }, interactions);
  }

  public void AddInputBinding(IReadOnlyList<string> inputBindingPaths, string interactions = "") {
    if (inputBindingPaths == null) {
      Assertion.Assert(false, "InputBindingPaths is null.");
      return;
    }

    // 再構築時に同じ登録を再適用するため、呼び出し側の変更を受けないようコピーして保持する
    var registration = new InputBindingRegistration(null, new List<string>(inputBindingPaths), interactions);
    _bindingRegistrationList.Add(registration);
    ApplyBindingRegistration(registration);
  }

  public void AddInputBinding(InputBindingType inputBindingType, string interactions = "") {
    AddInputBinding(InputBindingPath.Get(inputBindingType), interactions);
  }

  public void AddInputBinding(InputAction inputAction, string interactions = "") {
    if (inputAction == null) {
      Assertion.Assert(false, "InputAction is null.");
      return;
    }

    var registration = new InputBindingRegistration(InputActionAssetRegistry.Resolve(inputAction), null, interactions);
    _bindingRegistrationList.Add(registration);
    ApplyBindingRegistration(registration);
  }

  public void EnableInputAction() => _inputAction.Enable();
  public void DisableInputAction() => _inputAction.Disable();

  private void OnActionChange(object actionOrMapOrAsset, InputActionChange inputActionChange) {
    if (inputActionChange != InputActionChange.BoundControlsChanged) return;
    // 再構築中の自前アクションの変更で再入するのを防ぐ
    if (_isRebuilding) return;
    if (!IsRebuildTarget(actionOrMapOrAsset)) return;

    RebuildInputBindings();
  }

  private bool IsRebuildTarget(object actionOrMapOrAsset) {
    foreach (var registration in _bindingRegistrationList) {
      if (registration.SourceInputAction == null) continue;
      if (registration.SourceInputAction.IsActionChangeTarget(actionOrMapOrAsset)) return true;
    }

    return false;
  }

  private void RebuildInputBindings() {
    bool wasEnabled = _inputAction.enabled;
    _isRebuilding = true;

    try {
      _inputAction.Disable();
      ClearInputBindings();
      foreach (var registration in _bindingRegistrationList) {
        ApplyBindingRegistration(registration);
      }
    } finally {
      if (wasEnabled) _inputAction.Enable();
      _isRebuilding = false;
    }
  }

  private void ClearInputBindings() {
    // NOTE: コンポジット親のEraseは子パートも消すため、常に先頭から消して詰める
    while (_inputAction.bindings.Count > 0) {
      _inputAction.ChangeBinding(0).Erase();
    }
  }

  private void ApplyBindingRegistration(InputBindingRegistration registration) {
    if (registration.SourceInputAction != null) {
      ApplyInputActionBindings(registration.SourceInputAction, registration.Interactions);
      return;
    }

    var inputBindingPaths = registration.BindingPaths;
    for (int i = 0; i < inputBindingPaths.Count; i++) {
      _inputAction.AddBinding(inputBindingPaths[i], registration.Interactions);
    }
  }

  private void ApplyInputActionBindings(InputAction inputAction, string interactions) {
    var bindings = inputAction.bindings;

    for (int i = 0; i < bindings.Count; i++) {
      var binding = bindings[i];

      if (binding.isComposite) {
        // NOTE: コンポジット親はoverridePathが空文字なら無効化なので子パートごと飛ばし、通常時は合成名であるpathをそのまま使う
        if (binding.overridePath != null && binding.overridePath.Length == 0) {
          while (i + 1 < bindings.Count && bindings[i + 1].isPartOfComposite) {
            i++;
          }
          continue;
        }

        var compositeBuilder = _inputAction.AddCompositeBinding(binding.path, interactions);

        // Composite の子要素を追加
        i++;
        while (i < bindings.Count && bindings[i].isPartOfComposite) {
          var partBinding = bindings[i];
          compositeBuilder.With(partBinding.name, partBinding.effectivePath);
          i++;
        }
        i--;
      } else if (!binding.isPartOfComposite) {
        // 通常のバインディング
        _inputAction.AddBinding(binding.effectivePath, interactions: interactions);
      }
    }
  }

  private bool IsEnable() {
    if (!Enabled) return false;
    if (_parentUI is null) return false;
    if (_parentUI is IUIView view && !view.IsEnable()) return false;

    return true;
  }

  private readonly struct InputBindingRegistration {
    public InputAction SourceInputAction { get; }
    public IReadOnlyList<string> BindingPaths { get; }
    public string Interactions { get; }

    public InputBindingRegistration(InputAction sourceInputAction, IReadOnlyList<string> bindingPaths, string interactions) {
      SourceInputAction = sourceInputAction;
      BindingPaths = bindingPaths;
      Interactions = interactions;
    }
  }
}

}