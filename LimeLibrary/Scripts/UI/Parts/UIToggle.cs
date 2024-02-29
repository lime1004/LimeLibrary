using System;
using LimeLibrary.Extensions;
using LimeLibrary.UI.View;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LimeLibrary.UI.Parts {

[RequireComponent(typeof(Toggle))]
public class UIToggle : MonoBehaviour, IUIParts {
  [SerializeField]
  private InputAction _toggleInputAction;

  private readonly Subject<bool> _onValueChangeSubject = new();
  private readonly ClickRangeAdjuster _clickRangeAdjuster = new();
  private bool _isInitialized;

  public IUIView ParentView { get; private set; }
  public RectTransform RectTransform => transform.AsRectTransform();

  public Toggle Toggle { get; private set; }
  public IObservable<bool> OnValueChangeObservable => _onValueChangeSubject;

  public void Initialize(IUIView parentView) {
    if (_isInitialized) return;

    ParentView = parentView;

    Toggle = GetComponent<Toggle>();
    Toggle.OnValueChangedAsObservable().Subscribe(isOn => {
      if (!IsEnable()) return;
      _onValueChangeSubject.OnNext(isOn);
    }).AddTo(gameObject);

    _toggleInputAction = new InputAction("Toggle", InputActionType.Button);
    _toggleInputAction.Enable();
    _toggleInputAction.performed += _ => {
      if (!IsEnable()) return;
      Toggle.isOn = !Toggle.isOn;
    };

    _isInitialized = true;
  }

  private void OnEnable() {
    _toggleInputAction?.Enable();
  }

  private void OnDisable() {
    _toggleInputAction?.Disable();
  }

  private void OnDestroy() {
    _toggleInputAction?.Dispose();
  }

  public void SetIsOn(bool isOn) {
    Toggle.isOn = isOn;
  }

  public void AddInputBinding(string path) {
    _toggleInputAction.AddBinding(path);
  }

  private bool IsEnable() {
    if (gameObject.activeInHierarchy == false) return false;
    if (ParentView == null) return false;
    if (!ParentView.IsEnable()) return false;

    return true;
  }

  public void AdjustButtonRect() => _clickRangeAdjuster.Enable(transform.AsRectTransform());
  public void DisableButtonRect() => _clickRangeAdjuster.Disable();
}

}
