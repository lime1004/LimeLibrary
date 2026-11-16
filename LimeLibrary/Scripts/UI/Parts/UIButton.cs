using System;
using System.Collections.Generic;
using LimeLibrary.Extensions;
using LimeLibrary.UI.Module.Input;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using TMPro;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LimeLibrary.UI.Parts {

[RequireComponent(typeof(Button))]
public class UIButton : MonoBehaviour, IUIParts, ISelectHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISubmitHandler, IDisposable {
  [SerializeField]
  private TextMeshProUGUI _text;

  private UIInputReceiver _inputReceiver;

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

  public Button Button { get; private set; }
  public TextMeshProUGUI Text { get; private set; }

  public float LongPressSeconds { get; set; } = 1f;
  public bool IsIgnorePress { get; set; }
  public float PointerDownCounter => _pointerDownCounter;
  public Observable<Unit> OnClickObservable => GetEventObservable(UIButtonEventType.Click).AsUnitObservable();

  public void Initialize(IUIView parentView) {
    if (_isInitialized) return;

    _compositeDisposable.Clear();

    ParentView = parentView;
    Button = GetComponent<Button>();
    Text = _text != null ? _text : GetComponentInChildren<TextMeshProUGUI>(true);

    Button.OnClickAsObservable().Subscribe(_ => {
      if (!IsEnable()) return;
      if (_clickEnabledFunc != null && !_clickEnabledFunc.Invoke()) return;
      _eventSubjects[UIButtonEventType.Click].OnNext(null);
    }).AddTo(this);

    ClearSubject();

    _isInitialized = true;
  }

  private void CreateInputReceiverIfNeeded() {
    // NOTE: キー入力を持たないボタンが大半なので、受け手は最初に割り当てを足したときに作る
    if (_inputReceiver != null) return;

    _inputReceiver = new UIInputReceiver(ParentView);
    // NOTE: 親の画面が有効でもボタン自身が押せない間は受け付けないよう、ボタンの条件を足す
    _inputReceiver.SetEnableCondition(IsEnable);
    _inputReceiver.OnInputObservable.Subscribe(_ => OnKeyPress()).AddTo(_compositeDisposable);
    _inputReceiver.OnReleaseObservable.Subscribe(_ => OnKeyRelease()).AddTo(_compositeDisposable);
    _inputReceiver.OnPressCancelObservable.Subscribe(_ => OnKeyPressCancel()).AddTo(_compositeDisposable);
  }

  public void SetClickEnabledFunc(Func<bool> func) {
    _clickEnabledFunc = func;
  }

  private void OnKeyPress() {
    if (!IsIgnorePress) Click();
    OnPointerDown(null);
  }

  private void OnKeyRelease() {
    // NOTE: 離したときに押せなくなっていても押下中の状態は残さない
    if (IsEnable()) {
      if (IsIgnorePress && !_isLongPressed) Click();
      _eventSubjects[UIButtonEventType.PointerUp].OnNext(null);
    }
    ResetPressState();
  }

  private void OnKeyPressCancel() {
    // NOTE: 押したまま割り当てを変えたときは離していないのでクリックにはせず、押下中の状態だけを片づける
    if (IsEnable()) _eventSubjects[UIButtonEventType.PointerUp].OnNext(null);
    ResetPressState();
  }

  private void OnDestroy() {
    Dispose();
  }

  public void Click() {
    Button.onClick.Invoke();
  }

  public Observable<BaseEventData> GetEventObservable(UIButtonEventType eventType) {
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
      var subject = new Subject<BaseEventData>();
      subject.AddTo(_compositeDisposable);
      _eventSubjects.Add(eventType, subject);
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

  /// <summary>
  /// キー入力のパスを足す。反映は次の入力更新から
  /// </summary>
  public void AddInputBinding(string path) {
    if (string.IsNullOrEmpty(path)) {
      Assertion.Assert(false);
      return;
    }
    CreateInputReceiverIfNeeded();
    _inputReceiver.AddInputBinding(path);
  }

  /// <summary>
  /// キー入力のパスをすべて外す。反映は次の入力更新から
  /// </summary>
  public void ClearInputBinding() => _inputReceiver?.ClearInputBinding();

  public void SetText(string text) {
    if (Text == null) return;
    Text.text = text;
  }

  private bool IsEnable() {
    if (this == null) return false;
    if (!_isInitialized) return false;
    if (!enabled) return false;
    if (gameObject == null) return false;
    if (!gameObject.activeInHierarchy) return false;
    if (Button == null) return false;
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

  public void OnPointerClick(PointerEventData eventData) {
    if (!IsEnable()) return;

    UIButtonEventType? eventType = eventData.button switch {
      PointerEventData.InputButton.Left => UIButtonEventType.PointerLeftClick,
      PointerEventData.InputButton.Right => UIButtonEventType.PointerRightClick,
      PointerEventData.InputButton.Middle => UIButtonEventType.PointerMiddleClick,
      _ => null
    };
    if (eventType.HasValue) _eventSubjects[eventType.Value].OnNext(eventData);
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
    ResetPressState();
  }

  private void ResetPressState() {
    _isPress = false;
    _isLongPressed = false;
    _pointerDownCounter = 0f;
  }

  public void OnSubmit(BaseEventData eventData) {
    if (!IsEnable()) return;
    _eventSubjects[UIButtonEventType.Submit].OnNext(eventData);
  }

  private bool _disposed;

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;

    // NOTE: 一覧の項目のようにボタンだけが先に破棄されることがあるため、親の破棄を待たずに受け手を外す
    _inputReceiver?.Dispose();
    _compositeDisposable.Dispose();
  }
}

}