using System.Collections.Generic;
using FastEnumUtility;
using LimeLibrary.Extensions;
using LimeLibrary.Input;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using UniRx;
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

  private readonly Dictionary<InputMode, string> _inputBindingPathDictionary = new();

  private Image _image;
  private bool _isInitialized;

  public IUIView ParentView { get; private set; }
  public RectTransform RectTransform => transform.AsRectTransform();

  public Image Image => _image;

  public void Initialize(IUIView parentView) {
    if (_isInitialized) return;

    ParentView = parentView;

    _image = GetComponent<Image>();

    foreach (var inputMode in FastEnum.GetValues<InputMode>()) {
      _inputBindingPathDictionary.Add(inputMode, null);
    }

    // InputMode変更時の処理
    parentView.InputObservables.OnChangeInputModeObservable.Subscribe(ApplyImage).AddTo(this);

    // View表示時処理登録
    parentView.OnShowEndObservable.Subscribe(_ => {
      var currentInputMode = parentView.InputObservables.CurrentInputMode;
      ApplyImage(currentInputMode);
    }).AddTo(this);

    _isInitialized = true;
  }

  public void SetImage(Image image) {
    _image = image;
  }

  public void BindInput(string inputBindingPath, InputMode inputMode) {
    _inputBindingPathDictionary[inputMode] = inputBindingPath;

    ApplyImage(ParentView.InputObservables.CurrentInputMode);
  }

  public void BindInput(InputAction inputAction) {
    if (_inputBindingPathGetter == null) {
      Assertion.Assert(false, "InputBindingPathGetter is null");
      return;
    }

    foreach (var inputMode in FastEnum.GetValues<InputMode>()) {
      BindInput(_inputBindingPathGetter.GetInputBindingPath(inputAction, inputMode), inputMode);
    }
  }

  public void BindInput(InputBindingType inputBindingType, InputMode inputMode) {
    BindInput(InputBindingPath.Get(inputBindingType), inputMode);
  }

  private void ApplyImage(InputMode inputMode) {
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