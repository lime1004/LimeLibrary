using System.Collections.Generic;
using LimeLibrary.Extensions;
using LimeLibrary.Input;
using LimeLibrary.Input.InputMode;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LimeLibrary.UI.Parts {

[RequireComponent(typeof(Image))]
public class UIKeyImage : MonoBehaviour, IUIParts {
  [SerializeField]
  private UIKeyImageMap _uiKeyImageDictionary;
  [SerializeField]
  private InputBindingPathGetter _inputBindingPathGetter;
  [SerializeField]
  private InputActionReference _bindInputAction;

  private readonly Dictionary<string, string> _inputBindingPathDictionary = new();

  private Image _image;
  private bool _isInitialized;

  public IUIView ParentView { get; private set; }
  public RectTransform RectTransform => transform.AsRectTransform();

  public Image Image => _image;

  public void Initialize(IUIView parentView) {
    if (_isInitialized) return;

    ParentView = parentView;

    _image = GetComponent<Image>();

    foreach (var inputMode in InputModeUpdater.Instance.InputModeList) {
      _inputBindingPathDictionary.Add(inputMode.Name, null);
    }

    // InputMode変更時の処理
    parentView.InputObservables.OnChangeInputModeObservable.Subscribe(inputMode => ApplyImage(inputMode.Name)).AddTo(this);

    // View表示時処理登録
    parentView.OnShowEndObservable.Subscribe(_ => {
      var currentInputMode = parentView.InputObservables.CurrentInputMode;
      if (currentInputMode == null) return;
      ApplyImage(currentInputMode.Name);
    }).AddTo(this);

    // InputActionが設定されている場合はInputActionをバインド
    if (_bindInputAction != null) {
      BindInput(_bindInputAction);
    }

    _isInitialized = true;
  }

  public void SetImage(Image image) {
    _image = image;
  }

  public void BindInput(string inputBindingPath, string inputMode) {
    _inputBindingPathDictionary[inputMode] = inputBindingPath;

    var currentInputMode = ParentView.InputObservables.CurrentInputMode;
    if (currentInputMode == null) return;

    ApplyImage(currentInputMode.Name);
  }

  public void BindInput(InputAction inputAction) {
    if (_inputBindingPathGetter == null) {
      Assertion.Assert(false, "InputBindingPathGetter is null");
      return;
    }

    foreach (var inputMode in InputModeUpdater.Instance.InputModeList) {
      BindInput(_inputBindingPathGetter.GetInputBindingPath(inputAction, inputMode), inputMode.Name);
    }
  }

  public void BindInput(InputBindingType inputBindingType, string inputMode) {
    BindInput(InputBindingPath.Get(inputBindingType), inputMode);
  }

  private void ApplyImage(string inputMode) {
    if (_image == null) return;

    string inputBindingPath = _inputBindingPathDictionary[inputMode];
    if (!string.IsNullOrEmpty(inputBindingPath) && _uiKeyImageDictionary.ContainsImage(inputBindingPath, ParentView.InputObservables.CurrentControllerType)) {
      _image.gameObject.SetActive(true);
      _image.sprite = _uiKeyImageDictionary.GetImage(inputBindingPath, ParentView.InputObservables.CurrentControllerType);
    } else {
      _image.gameObject.SetActive(false);
      _image.sprite = null;
    }
  }
}

}