using System.Reflection;
using LimeLibrary.Input;
using LimeLibrary.UI.Module.Selectable.SelectableAppearance;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using R3;
using R3.Triggers;
using UnityEngine.EventSystems;

namespace LimeLibrary.UI.Module.Selectable {

public class SelectableExtender {
  protected readonly IUIView _parentView;
  protected readonly UnityEngine.UI.Selectable _selectable;

  private readonly CompositeDisposable _compositeDisposable = new();
  private readonly Subject<(ExtendSelectionState, ExtendSelectionState)> _onChangeExtendSelectionStateSubject = new();

  private ExtendSelectionState _extendSelectionState;
  public ExtendSelectionState ExtendSelectionState {
    get => _extendSelectionState;
    protected set {
      if (_extendSelectionState == value) return;
      _onChangeExtendSelectionStateSubject.OnNext((_extendSelectionState, value));
      _extendSelectionState = value;
    }
  }
  private readonly SelectableAppearanceDictionary _selectableAppearanceDictionary = new();
  private readonly Subject<AxisEventData> _onMoveSubject = new();

  public Observable<(ExtendSelectionState from, ExtendSelectionState to)> OnChangeExtendSelectionState => _onChangeExtendSelectionStateSubject;
  public Observable<AxisEventData> OnMoveObservable => _onMoveSubject;
  public bool IsUnclickable { get; set; }

  public SelectableExtender(IUIView parentView, UnityEngine.UI.Selectable selectable) {
    _parentView = parentView;
    _selectable = selectable;

    // 元の状態変化効果を無効化
    selectable.transition = UnityEngine.UI.Selectable.Transition.None;

    // Move時処理
    selectable.OnMoveAsObservable().Subscribe(axisEventData => _onMoveSubject.OnNext(axisEventData)).AddTo(_compositeDisposable);

    // SelectionState変更時処理
    selectable.LateUpdateAsObservable().Subscribe(_ => {
      UpdateSelectionState();
    }).AddTo(_compositeDisposable);

    // 状態変更時処理
    OnChangeExtendSelectionState.Subscribe(_parentView, (states, view) => {
      var (prevState, nextState) = states;
      OnChangeState(view.InputObservables.CurrentInputMode.Name, prevState, nextState);
    }).AddTo(_compositeDisposable);

    // InputMode変更時処理
    _parentView.InputObservables.OnChangeInputModeObservable.Subscribe(inputMode => {
      var appearanceDataList = _selectableAppearanceDictionary.GetSelectableAppearanceDataList(ExtendSelectionState);
      foreach (var selectableAppearanceData in appearanceDataList) {
        if (selectableAppearanceData.IsEnableInputMode(inputMode.Name)) {
          selectableAppearanceData.Apply();
        }
      }
      foreach (var selectableAppearanceData in appearanceDataList) {
        if (!selectableAppearanceData.IsEnableInputMode(inputMode.Name)) {
          selectableAppearanceData.Revert();
        }
      }
    }).AddTo(_compositeDisposable);

    _compositeDisposable.AddTo(selectable);

    ExtendSelectionState = ExtendSelectionState.Normal;
  }

  public void AddSelectableAppearance(ExtendSelectionState extendSelectionState, SelectableAppearance.SelectableAppearance selectableAppearance, string inputMode, params string[] additionalInputModes) {
    _selectableAppearanceDictionary.AddAppearance(extendSelectionState, selectableAppearance, inputMode);
    _selectableAppearanceDictionary.AddAppearance(extendSelectionState, selectableAppearance, additionalInputModes);
  }

  public void ClearSelectableAppearance() {
    _selectableAppearanceDictionary.ClearAppearance();
  }

  public void ApplySelectableAppearance() {
    var selectableAppearanceDataList = _selectableAppearanceDictionary.GetSelectableAppearanceDataList(ExtendSelectionState);
    string inputMode = _parentView.InputObservables.CurrentInputMode.Name;
    foreach (var selectableAppearanceData in selectableAppearanceDataList) {
      if (selectableAppearanceData.IsEnableInputMode(inputMode)) {
        selectableAppearanceData.Apply();
      }
    }
  }

  private void OnChangeState(string inputMode, ExtendSelectionState prevState, ExtendSelectionState nextState) {
    var prevAppearanceDataList = _selectableAppearanceDictionary.GetSelectableAppearanceDataList(prevState);
    foreach (var selectableAppearanceData in prevAppearanceDataList) {
      if (selectableAppearanceData.IsEnableInputMode(inputMode)) {
        selectableAppearanceData.Revert();
      }
    }
    if (_parentView.IsFocus) {
      var nextAppearanceDataList = _selectableAppearanceDictionary.GetSelectableAppearanceDataList(nextState);
      foreach (var selectableAppearanceData in nextAppearanceDataList) {
        if (selectableAppearanceData.IsEnableInputMode(inputMode)) {
          selectableAppearanceData.Apply();
        }
      }
    }
  }

  private void UpdateSelectionState() {
    int currentSelectionState = GetSelectionState(_selectable);
    ExtendSelectionState = IsUnclickable ? ExtendSelectionState.Unclickable : ExtendSelectionStateUtility.ToExtendSelectionState(currentSelectionState);
  }

  private int GetSelectionState(UnityEngine.UI.Selectable selectable) {
    var type = selectable.GetType();
    var propertyInfo = type.GetProperty("currentSelectionState", BindingFlags.Instance | BindingFlags.NonPublic);
    if (propertyInfo == null) {
      Assertion.Assert(false);
      return 0;
    }
    return (int) propertyInfo.GetValue(selectable);
  }
}

}