using System.Collections.Generic;
using FastEnumUtility;
using LimeLibrary.Extensions;
using LimeLibrary.Input;
using LimeLibrary.Input.InputMode;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace LimeLibrary.UI.Parts {

public class UIButtonKey : MonoBehaviour, IUIParts {
  [SerializeField]
  private UIButton _uiButton;
  [SerializeField]
  private UIKeyImage _keyImage;
  [SerializeField]
  private InputBindingPathGetter _inputBindingPathGetter;

  private readonly List<KeyBindingRegistration> _bindingRegistrationList = new();

  private IUIView _parentView;
  private bool _isInitialized;

  public IUIView ParentView { get; private set; }
  public RectTransform RectTransform => transform.AsRectTransform();

  public UIButton UIButton => _uiButton;
  public UIKeyImage KeyImage => _keyImage;

  public void Initialize(IUIView parentView) {
    if (_isInitialized) return;

    ParentView = parentView;

    // UIKeyImageの初期化
    _keyImage.Initialize(parentView);
    _keyImage.Image.raycastTarget = false;

    // UIButtonの初期化
    _uiButton.Initialize(parentView);

    // リバインドによるバインディング変更に追随させる
    InputSystem.onActionChange += OnActionChange;

    _isInitialized = true;
  }

  private void OnDestroy() {
    InputSystem.onActionChange -= OnActionChange;
  }

  public void BindInput(string inputBindingPath, string inputMode, bool isOverwrite = false) {
    if (isOverwrite) ClearBindingRegistration();

    _bindingRegistrationList.Add(new KeyBindingRegistration(null, inputBindingPath, inputMode));
    BindInputInternal(inputBindingPath, inputMode);
  }

  public void BindInput(InputBindingType inputBindingType, string inputMode, bool isOverwrite = false) {
    BindInput(InputBindingPath.Get(inputBindingType), inputMode, isOverwrite);
  }

  public void BindInput(InputAction inputAction, bool isOverwrite = false) {
    if (isOverwrite) ClearBindingRegistration();

    if (inputAction == null) {
      Assertion.Assert(false, "InputAction is null.");
      return;
    }

    var sourceInputAction = InputActionAssetRegistry.Resolve(inputAction);
    _bindingRegistrationList.Add(new KeyBindingRegistration(sourceInputAction, null, null));
    ApplyInputActionBinding(sourceInputAction);
  }

  private void OnActionChange(object actionOrMapOrAsset, InputActionChange inputActionChange) {
    if (inputActionChange != InputActionChange.BoundControlsChanged) return;
    if (!IsRebindTarget(actionOrMapOrAsset)) return;

    RebindInput();
  }

  private bool IsRebindTarget(object actionOrMapOrAsset) {
    foreach (var registration in _bindingRegistrationList) {
      if (registration.SourceInputAction == null) continue;
      if (registration.SourceInputAction.IsActionChangeTarget(actionOrMapOrAsset)) return true;
    }

    return false;
  }

  private void RebindInput() {
    // NOTE: UIButtonのバインド登録は加算式のため、記録済みの登録を全て貼り直す
    _uiButton.ClearInputBinding();

    for (int i = 0; i < _bindingRegistrationList.Count; i++) {
      var registration = _bindingRegistrationList[i];
      if (registration.SourceInputAction == null) {
        BindInputInternal(registration.BindingPath, registration.InputModeName);
        continue;
      }

      var sourceInputAction = InputActionAssetRegistry.Resolve(registration.SourceInputAction);
      _bindingRegistrationList[i] = new KeyBindingRegistration(sourceInputAction, null, null);
      ApplyInputActionBinding(sourceInputAction);
    }
  }

  private void ClearBindingRegistration() {
    _bindingRegistrationList.Clear();
    _uiButton.ClearInputBinding();
  }

  private void ApplyInputActionBinding(InputAction inputAction) {
    if (_inputBindingPathGetter == null) {
      Assertion.Assert(false, "InputBindingPathGetter is null.");
      return;
    }

    foreach (var inputMode in InputModeUpdater.Instance.InputModeList) {
      BindInputInternal(_inputBindingPathGetter.GetInputBindingPath(inputAction, inputMode), inputMode.Name);
    }
  }

  private void BindInputInternal(string inputBindingPath, string inputMode) {
    // 空パスは無効化なので入力の登録だけ行わず、キー画像は非表示に更新させる
    if (!string.IsNullOrEmpty(inputBindingPath)) _uiButton.AddInputBinding(inputBindingPath);
    _keyImage.BindInput(inputBindingPath, inputMode);
  }

  private readonly struct KeyBindingRegistration {
    public InputAction SourceInputAction { get; }
    public string BindingPath { get; }
    public string InputModeName { get; }

    public KeyBindingRegistration(InputAction sourceInputAction, string bindingPath, string inputModeName) {
      SourceInputAction = sourceInputAction;
      BindingPath = bindingPath;
      InputModeName = inputModeName;
    }
  }
}

}