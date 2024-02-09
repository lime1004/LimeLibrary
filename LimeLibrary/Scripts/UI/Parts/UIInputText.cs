using System;
using LimeLibrary.Extensions;
using LimeLibrary.UI.Module.Input;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LimeLibrary.UI.Parts {

public class UIInputText : MonoBehaviour, IUIParts, ISubmitHandler {
  [SerializeField]
  private TMP_InputField _inputFieldText;

  private bool _isInitialized;

  private Selectable _dummySelectable;

  public IUIView ParentView { get; private set; }
  public RectTransform RectTransform => transform.AsRectTransform();

  public TMP_InputField InputField => _inputFieldText;
  public UnityEngine.UI.Selectable Selectable => _dummySelectable;
  public bool IsActivated { get; private set; }

  public IObservable<Unit> OnSubmitObservable => Selectable.OnSubmitAsObservable().AsUnitObservable();
  public IObservable<Unit> OnPointerClickObservable => Selectable.OnPointerClickAsObservable().AsUnitObservable();

  public TMP_InputField.ContentType ContentType {
    get => _inputFieldText.contentType;
    set {
      _inputFieldText.contentType = value;
      _inputFieldText.ForceLabelUpdate();
    }
  }

  public void Initialize(IUIView parentView) {
    if (_isInitialized) return;

    ParentView = parentView;

    // Selectableを別途作成
    var dummyObject = UnityUtility.CreateGameObjectWithRectTransform("DummySelectable", gameObject);
    _dummySelectable = dummyObject.AddComponent<UnityEngine.UI.Selectable>();
    var dummyImage = dummyObject.AddComponent<Image>();
    dummyImage.SetAlpha(0f);
    dummyObject.GetRectTransform().SetAnchor(new Vector2(0, 0), new Vector2(1, 1), false);
    dummyObject.GetRectTransform().anchoredPosition = Vector2.zero;
    dummyObject.GetRectTransform().sizeDelta = Vector2.zero;

    // Navigationは切る
    _inputFieldText.DisableNavigation();
    _inputFieldText.interactable = false;

    // サブミット時、クリック時処理
    Observable.Merge(
      _dummySelectable.OnSubmitAsObservable(),
      _dummySelectable.OnPointerClickAsObservable()).Subscribe(_ => {
      if (!IsEnable()) return;

      _inputFieldText.interactable = true;
      _inputFieldText.ActivateInputField();
      _inputFieldText.Select();
      IsActivated = true;
    }).AddTo(this);

    _isInitialized = true;
  }

  public void SetupOnExitInput(UIInputReceiver exitUiInputReceiver) {
    // キャンセル時処理
    exitUiInputReceiver.OnInputObservable.Subscribe(_ => {
      _inputFieldText.interactable = false;
      _inputFieldText.DeactivateInputField();
      _dummySelectable.Select();
      IsActivated = false;
    }).AddTo(this);
  }

  public string GetText() {
    return _inputFieldText.text;
  }

  public void SetText(string text) {
    _inputFieldText.SetTextWithoutNotify(text);
  }

  private bool IsEnable() {
    if (!enabled) return false;
    if (!gameObject.activeInHierarchy) return false;
    if (ParentView == null) return false;
    if (!ParentView.IsEnable()) return false;

    return true;
  }

  public void OnSubmit(BaseEventData eventData) {
    if (_inputFieldText != null) {
      _inputFieldText.interactable = true;
      _inputFieldText.ActivateInputField();
      _inputFieldText.Select();
    }
  }
}

}