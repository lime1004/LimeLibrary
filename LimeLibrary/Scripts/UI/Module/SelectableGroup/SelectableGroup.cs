using System.Collections.Generic;
using System.Linq;
using LimeLibrary.Input;
using LimeLibrary.UI.Module.Input;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LimeLibrary.UI.Module.SelectableGroup {

public abstract class SelectableGroup {
  protected readonly IUIView _parentView;
  private readonly Subject<SelectableData> _onSelectSubject = new();
  private readonly Subject<SelectableData> _onDeselectSubject = new();
  private readonly Dictionary<UnityEngine.UI.Selectable, CompositeDisposable> _compositeDisposableDictionary = new(256);

  private UIInputReceiver _inputReceiver;
  private int _selectedIndex;
  private UnityEngine.UI.Selectable _selectedObject;
  private bool _isSelectedByGamepad;

  public bool Enabled { get; set; } = true;
  public bool IsLoop { get; set; }
  public int SelectedIndex => _selectedIndex;
  public Observable<SelectableData> OnSelectObservable => _onSelectSubject;
  public Observable<SelectableData> OnDeselectObservable => _onDeselectSubject;

  protected SelectableGroup(IUIView parentView, SelectableGroupSelectMode selectMode, IReadOnlyCollection<string> inputBindingPathList = null) {
    _parentView = parentView;

    switch (selectMode) {
    case SelectableGroupSelectMode.Auto:
      SetupAutoSelect(parentView);
      break;
    case SelectableGroupSelectMode.Manual:
      SetupManualSelect(inputBindingPathList);
      break;
    }

    parentView.OnHideEndObservable.Subscribe(_ => Deselect()).AddTo(parentView.RootObject);
  }

  private bool IsEnable() {
    return Enabled && _parentView.IsEnable();
  }

  private void SetupAutoSelect(IUIView parentView) {
    // 入力切り替え時処理登録
    parentView.InputObservables.OnChangeInputModeObservable.Subscribe(inputMode => {
      if (!IsEnable()) return;

      if (inputMode.IsSelectOnChangeInputMode) {
        Select();
      } else {
        Deselect();
      }
    }).AddTo(parentView.RootObject);

    // Focus変更時処理登録
    parentView.EventObservables.GetObservable(UIViewEventType.Focus).Subscribe(_ => {
      if (!IsEnable()) return;

      if (parentView.InputObservables.CurrentInputMode.IsSelectOnFocusUIView) {
        Select();
      }
    }).AddTo(parentView.RootObject);

    parentView.EventObservables.GetObservable(UIViewEventType.Unfocus).Subscribe(_ => {
      if (!IsEnable()) return;

      if (parentView.InputObservables.CurrentInputMode.IsDeselectOnUnfocusUIView) {
        Deselect();
      }
    }).AddTo(parentView.RootObject);
  }

  private void SetupManualSelect(IReadOnlyCollection<string> inputBindingPathList) {
    if (inputBindingPathList == null) return;

    _inputReceiver = new UIInputReceiver(_parentView);
    foreach (string inputBindingPath in inputBindingPathList) {
      _inputReceiver.AddInputBinding(inputBindingPath);
    }

    _inputReceiver.OnInputObservable.Subscribe(_ => {
      if (!IsEnable()) return;

      if (!_isSelectedByGamepad) {
        Select();
        _isSelectedByGamepad = true;
      } else {
        Deselect();
        _isSelectedByGamepad = false;
      }
    }).AddTo(_parentView.RootObject);

    _parentView.OnHideEndObservable.Subscribe(_ => {
      _isSelectedByGamepad = false;
      _inputReceiver.Dispose();
    }).AddTo(_parentView.RootObject);

    OnDeselectObservable.Subscribe(_ => {
      _isSelectedByGamepad = false;
    }).AddTo(_parentView.RootObject);
  }

  public void Select() {
    if (!Enabled) return;

    var (selectable, _) = GetSelectSelectable();
    if (selectable) selectable.Select();
  }

  public void Select(int index) {
    if (!Enabled) return;

    _selectedIndex = index;
    Select();
  }

  public (UnityEngine.UI.Selectable selectable, int index) GetSelectSelectable() {
    var list = GetSelectableDataList();

    if (!list.Any()) return (null, 0);

    int index = Mathf.Clamp(_selectedIndex, 0, list.Count - 1);
    for (int i = index; i >= 0; i--) {
      if (!list[i].IsSelectable()) continue;
      return (list[i].Selectable, i);
    }
    for (int i = index; i < list.Count; i++) {
      if (!list[i].IsSelectable()) continue;
      return (list[i].Selectable, i);
    }
    return (null, 0);
  }

  public bool IsValidSelectedIndex() {
    return _selectedIndex >= 0 && _selectedIndex < GetSelectableDataList().Count;
  }

  public void Deselect() {
    if (!Enabled) return;

    if (EventSystem.current == null || _selectedObject == null) return;
    if (EventSystem.current.currentSelectedGameObject == _selectedObject.gameObject) {
      EventSystem.current.SetSelectedGameObject(null);
    }
  }

  public void ResetSelectIndex() {
    _selectedIndex = 0;
    _selectedObject = null;
    Deselect();
  }

  public abstract List<SelectableData> GetSelectableDataList();
  public abstract void AddSelectable(UnityEngine.UI.Selectable selectable, List<object> dataList = null);
  public abstract void AddSelectable(IEnumerable<UnityEngine.UI.Selectable> selectables, List<object> dataList = null);
  public abstract void RemoveSelectable(UnityEngine.UI.Selectable selectable);
  public abstract bool ExistsSelectable(UnityEngine.UI.Selectable selectable);
  public abstract SelectableData GetSelectableData(UnityEngine.UI.Selectable selectable);

  public virtual void ClearSelectable() {
    ResetSelectIndex();
    foreach (var (_, disposable) in _compositeDisposableDictionary) {
      disposable.Dispose();
    }
    _compositeDisposableDictionary.Clear();
  }

  public abstract void SetupNavigation();

  public bool IsSelectedInGroup() {
    var (selectableData, _) = GetSelectableData(EventSystem.current.currentSelectedGameObject);
    return selectableData != null;
  }

  protected void RegisterEvent(UnityEngine.UI.Selectable selectable) {
    UnregisterEvent(selectable);

    var compositeDisposable = new CompositeDisposable();
    selectable.OnSelectAsObservable().Subscribe(baseEventData => {
      (var selectableData, int index) = GetSelectableData(baseEventData.selectedObject);
      if (selectableData == null) {
        Assertion.Assert(false);
        return;
      }
      _selectedObject = selectableData.Selectable;
      _selectedIndex = index;
      _onSelectSubject.OnNext(selectableData);
    }).AddTo(_parentView.RootObject).AddTo(compositeDisposable);

    selectable.OnDeselectAsObservable().Subscribe(baseEventData => {
      (var selectableData, int _) = GetSelectableData(baseEventData.selectedObject);
      _onDeselectSubject.OnNext(selectableData);
    }).AddTo(_parentView.RootObject).AddTo(compositeDisposable);

    _compositeDisposableDictionary.Add(selectable, compositeDisposable);
  }

  protected void UnregisterEvent(UnityEngine.UI.Selectable selectable) {
    if (_compositeDisposableDictionary.ContainsKey(selectable)) {
      _compositeDisposableDictionary[selectable].Dispose();
      _compositeDisposableDictionary.Remove(selectable);
    }
  }

  protected void UpdateSelectedIndex() {
    if (_selectedObject == null) return;
    (var selectableData, int index) = GetSelectableData(_selectedObject.gameObject);
    if (selectableData == null) return;
    _selectedIndex = index;
  }

  private (SelectableData, int) GetSelectableData(GameObject selectableGameObject) {
    var list = GetSelectableDataList();
    for (int i = 0; i < list.Count; i++) {
      var selectableData = list[i];
      if (selectableData.Selectable == null) continue;
      if (selectableData.Selectable.gameObject != selectableGameObject) continue;

      return (selectableData, i);
    }
    return (null, 0);
  }
}

}