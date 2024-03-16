using FastEnumUtility;
using LimeLibrary.Extensions;
using LimeLibrary.Input;
using LimeLibrary.UI.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LimeLibrary.UI.Parts {

public class UIButtonKey : MonoBehaviour, IUIParts {
  [SerializeField]
  private UIButton _uiButton;
  [SerializeField]
  private UIKeyImage _keyImage;
  [SerializeField]
  private InputBindingPathGetter _inputBindingPathGetter;

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

    _isInitialized = true;
  }

  public void BindInput(string inputBindingPath, InputMode inputMode) {
    _uiButton.ClearInputBinding();
    BindInputInternal(inputBindingPath, inputMode);
  }

  public void BindInput(InputBindingType inputBindingType, InputMode inputMode) {
    _uiButton.ClearInputBinding();
    BindInputInternal(InputBindingPath.Get(inputBindingType), inputMode);
  }

  public void BindInput(InputAction inputAction) {
    _uiButton.ClearInputBinding();
    foreach (var inputMode in FastEnum.GetValues<InputMode>()) {
      BindInput(_inputBindingPathGetter.GetInputBindingPath(inputAction, inputMode), inputMode);
    }
  }

  private void BindInputInternal(string inputBindingPath, InputMode inputMode) {
    _uiButton.AddInputBinding(inputBindingPath);
    _keyImage.BindInput(inputBindingPath, inputMode);
  }

}

}