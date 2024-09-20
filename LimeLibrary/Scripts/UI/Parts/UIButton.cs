using System;
using System.Collections.Generic;
using LimeLibrary.Extensions;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LimeLibrary.UI.Parts {

[RequireComponent(typeof(Button))]
public class UIButton : MonoBehaviour, IUIParts, ISelectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISubmitHandler {
  private InputAction _inputAction;

  private readonly Dictionary<UIButtonEventType, Subject<BaseEventData>> _eventSubjects = new();
  private readonly CompositeDisposable _compositeDisposable = new();
  private readonly ClickRangeAdjuster _clickRangeAdjuster = new();

  private bool _isInitialized;
  private Func<bool> _clickEnabledFunc;
  private bool _isPress;
  private bool _isLongPressed;
  private float _pointerDownCounter;

  public RectTransform RectTransform => transform.AsRectTransform();
  public IUIView ParentView { get; private set; }

  public UnityEngine.UI.Button Button { get; private set; }
  public TextMeshProUGUI Text { get; private set; }

  public float LongPressSeconds { get; set; } = 1f;
  public bool IsIgnorePress { get; set; }
  public float PointerDownCounter => _pointerDownCounter;
  public IObservable<Unit> OnClickObservable => GetEventObservable(UIButtonEventType.Click).AsUnitObservable();

  public void Initialize(IUIView parentView) {
    if (_isInitialized) return;

    ParentView = parentView;
    Button = GetComponent<UnityEngine.UI.Button>();
    Text = GetComponentInChildren<TextMeshProUGUI>(true);

    Button.OnClickAsObservable().Subscribe(_ => {
      if (!IsEnable()) return;
      if (_clickEnabledFunc != null && !_clickEnabledFunc.Invoke()) return;
      _eventSubjects[UIButtonEventType.Click].OnNext(null);
    }).AddTo(this);

    InitializeInputAction();
    ClearSubject();

    _isInitialized = true;
  }

  private void InitializeInputAction() {
    _inputAction?.Dispose();
    _compositeDisposable?.Clear();

    _inputAction = new InputAction("Button", InputActionType.Button);
    _inputAction.Enable();
    _inputAction.performed += OnPerformed;

    ParentView.EventObservables.GetObservable(UIViewEventType.Focus).Subscribe(_ => _inputAction.Enable()).AddTo(this).AddTo(_compositeDisposable);
    ParentView.EventObservables.GetObservable(UIViewEventType.Unfocus).Subscribe(_ => _inputAction.Disable()).AddTo(this).AddTo(_compositeDisposable);
  }

  public void SetClickEnabledFunc(Func<bool> func) {
    _clickEnabledFunc = func;
  }

  private void OnPerformed(InputAction.CallbackContext context) {
    if (!IsEnable()) return;

    if (context.control.IsActuated()) {
      if (!IsIgnorePress) Click();
      OnPointerDown(null);
    } else {
      if (IsIgnorePress && !_isLongPressed) Click();
      OnPointerUp(null);
    }
  }

  private void OnDestroy() {
    if (_inputAction == null) return;

    _inputAction.performed -= OnPerformed;
    _inputAction.Dispose();

    _compositeDisposable.Dispose();
  }

  public void Click() {
    Button.onClick.Invoke();
  }

  public IObservable<BaseEventData> GetEventObservable(UIButtonEventType eventType) {
    if (!_isInitialized) {
      Assertion.Assert(false, "UIButtonが初期化されていません. " + gameObject);
      return Observable.Never<BaseEventData>();
    }

    return _eventSubjects[eventType];
  }

  public void ClearSubject() {
    foreach (var (_, subject) in _eventSubjects) {
      subject.Dispose();
    }
    _eventSubjects.Clear();

    foreach (UIButtonEventType eventType in Enum.GetValues(typeof(UIButtonEventType))) {
      _eventSubjects.Add(eventType, new Subject<BaseEventData>());
    }
  }

  private void Update() {
    if (!_isInitialized) return;

    if (_isPress && !_isLongPressed) {
      _pointerDownCounter += Time.deltaTime;
      if (_pointerDownCounter > LongPressSeconds) {
        _eventSubjects[UIButtonEventType.LongPress].OnNext(null);
        _isLongPressed = true;
      }
    }
  }

  public void AddInputBinding(string path) {
    if (string.IsNullOrEmpty(path)) {
      Assertion.Assert(false);
      return;
    }
    _inputAction.AddBinding(path);
  }

  public void ClearInputBinding() {
    InitializeInputAction();
  }

  public void SetText(string text) {
    if (Text == null) return;
    Text.text = text;
  }

  private bool IsEnable() {
    if (this == null) return false;
    if (!enabled) return false;
    if (!gameObject.activeInHierarchy) return false;
    if (!Button.interactable) return false;
    if (ParentView == null) return false;
    if (!ParentView.IsEnable()) return false;

    return true;
  }

  public void EnableNavigation(Navigation.Mode navigationMode) => Button.EnableNavigation(navigationMode);
  public void DisableNavigation() => Button.DisableNavigation();

  public void AdjustButtonRect() => _clickRangeAdjuster.Enable(transform.AsRectTransform());
  public void DisableButtonRect() => _clickRangeAdjuster.Disable();

  public void DisableImage() {
    if (TryGetComponent(out Image image)) {
      image.enabled = false;
    }
  }

  public void OnSelect(BaseEventData eventData) {
    if (!IsEnable()) return;
    _eventSubjects[UIButtonEventType.Select].OnNext(eventData);
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (!IsEnable()) return;
    _eventSubjects[UIButtonEventType.PointerEnter].OnNext(eventData);
  }

  public void OnPointerExit(PointerEventData eventData) {
    if (!IsEnable()) return;
    _eventSubjects[UIButtonEventType.PointerExit].OnNext(eventData);
  }

  public void OnPointerDown(PointerEventData eventData) {
    if (!IsEnable()) return;
    _eventSubjects[UIButtonEventType.PointerDown].OnNext(eventData);
    _isPress = true;
  }

  public void OnPointerUp(PointerEventData eventData) {
    if (!IsEnable()) return;
    _eventSubjects[UIButtonEventType.PointerUp].OnNext(eventData);
    _isPress = false;
    _isLongPressed = false;
    _pointerDownCounter = 0f;
  }

  public void OnSubmit(BaseEventData eventData) {
    if (!IsEnable()) return;
    _eventSubjects[UIButtonEventType.Submit].OnNext(eventData);
  }
}

}