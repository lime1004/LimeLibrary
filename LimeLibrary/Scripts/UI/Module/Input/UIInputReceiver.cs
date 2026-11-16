using System;
using System.Collections.Generic;
using LimeLibrary.Input;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using R3;
using R3.Triggers;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace LimeLibrary.UI.Module.Input {

public class UIInputReceiver : IDisposable {
  private readonly IUI _parentUI;
  private readonly UIInputBindingSet _bindingSet;
  private readonly Subject<InputAction.CallbackContext> _onInputSubject = new();
  private readonly Subject<InputAction.CallbackContext> _onReleaseSubject = new();
  private readonly Subject<Unit> _onPressCancelSubject = new();
  private readonly CompositeDisposable _parentSubscriptions = new();

  private UISharedInputAction _sharedInputAction;
  private Func<bool> _enableCondition;
  private bool _enabled = true;
  private bool _isAccepting = true;
  private bool _disposed;
  private bool _hasCapturedEnabled;
  private uint _capturedEnabledUpdateCount;
  private bool _hasEnabledEdge;
  private uint _enabledEdgeUpdateCount;

  public bool Enabled {
    get => _enabled;
    set {
      if (value && !_enabled) StampEnabledEdge();
      _enabled = value;
    }
  }
  public Observable<InputAction.CallbackContext> OnInputObservable => _onInputSubject;
  internal Observable<InputAction.CallbackContext> OnReleaseObservable => _onReleaseSubject;
  internal Observable<Unit> OnPressCancelObservable => _onPressCancelSubject;

  public Observable<TValue> OnValueObservable<TValue>() where TValue : struct {
    return Observable.EveryUpdate()
      .Where(_ => IsInputEnabled() && _sharedInputAction != null)
      .Select(_ => _sharedInputAction.InputAction.ReadValue<TValue>());
  }

  public UIInputReceiver(IUI parentUI, InputInteractionType inputInteractionType, int? behaviourType = null) :
    this(parentUI, InputActionType.Button, InputInteractionBuilder.GetInteractions(inputInteractionType, behaviourType)) { }

  public UIInputReceiver(IUI parentUI, InputActionType inputActionType = InputActionType.Button, string interactions = "") {
    if (parentUI == null) {
      Assertion.Assert(false, "ParentUI is null.");
      return;
    }

    _parentUI = parentUI;
    _bindingSet = new UIInputBindingSet(inputActionType, interactions);

    // 親の表示が終わったら受け付け、非表示が終わったら受け付けない
    // NOTE: 部品の受け手は親より先に破棄されることがあるため、親への購読は自分の破棄でも外す
    parentUI.OnShowEndObservable.Subscribe(_ => EnableInputAction()).AddTo(_parentSubscriptions);
    parentUI.OnHideEndObservable.Subscribe(_ => DisableInputAction()).AddTo(_parentSubscriptions);
    parentUI.RootObject.OnDestroyAsObservable().Take(1).Subscribe(_ => Dispose()).AddTo(_parentSubscriptions);
    // NOTE: ダイアログが閉じたときのように表示の合図なしに有効へ戻る画面があるため、フォーカスも有効になった合図として扱う
    if (parentUI is IUIView parentView) {
      parentView.EventObservables.GetObservable(UIViewEventType.Focus).Subscribe(_ => StampEnabledEdge()).AddTo(_parentSubscriptions);
    }
  }

  public void Dispose() {
    if (_disposed) return;
    _disposed = true;

    _parentSubscriptions.Dispose();
    _sharedInputAction?.RemoveListenerAndPress(this);
    _sharedInputAction = null;
    _onInputSubject.OnCompleted();
    _onReleaseSubject.OnCompleted();
    _onPressCancelSubject.OnCompleted();
  }

  /// <summary>
  /// 入力パスを受け付ける入力に足す。反映は次の入力更新から
  /// </summary>
  public void AddInputBinding(string inputBindingPath, string interactions = "") {
    AddInputBinding(new[] { inputBindingPath }, interactions);
  }

  /// <summary>
  /// 複数の入力パスをまとめて受け付ける入力に足す。反映は次の入力更新から
  /// </summary>
  public void AddInputBinding(IReadOnlyList<string> inputBindingPaths, string interactions = "") {
    if (inputBindingPaths == null) {
      Assertion.Assert(false, "InputBindingPaths is null.");
      return;
    }

    _bindingSet.AddPaths(inputBindingPaths, interactions);
    RequestResolve();
  }

  /// <summary>
  /// 定義済みの入力の種類を受け付ける入力に足す。反映は次の入力更新から
  /// </summary>
  public void AddInputBinding(InputBindingType inputBindingType, string interactions = "") {
    AddInputBinding(InputBindingPath.Get(inputBindingType), interactions);
  }

  /// <summary>
  /// InputActionの割り当てを受け付ける入力に足す。元のInputActionは有効・無効を含めて変更しない。反映は次の入力更新から
  /// </summary>
  public void AddInputBinding(InputAction inputAction, string interactions = "") {
    if (inputAction == null) {
      Assertion.Assert(false, "InputAction is null.");
      return;
    }

    _bindingSet.AddSourceAction(inputAction, interactions);
    RequestResolve();
  }

  /// <summary>
  /// 受け付ける入力をすべて外す。反映は次の入力更新から
  /// </summary>
  public void ClearInputBinding() {
    _bindingSet.Clear();
    RequestResolve();
  }

  /// <summary>
  /// 入力の受け付けを再開する。入力そのものは常に有効で、届け先として選ばれるかだけが変わる
  /// </summary>
  public void EnableInputAction() {
    if (!_isAccepting) StampEnabledEdge();
    _isAccepting = true;
  }

  /// <summary>
  /// 入力の受け付けを止める。入力そのものは常に有効で、届け先として選ばれるかだけが変わる
  /// </summary>
  public void DisableInputAction() => _isAccepting = false;

  internal void SetEnableCondition(Func<bool> enableCondition) => _enableCondition = enableCondition;

  internal bool IsInputEnabled() {
    if (_disposed) return false;
    if (!_enabled) return false;
    if (!_isAccepting) return false;
    if (_parentUI is IUIView view && !view.IsEnable()) return false;
    if (_enableCondition != null && !_enableCondition()) return false;

    return true;
  }

  internal void CaptureEnabled(uint updateCount) {
    if (!IsInputEnabled()) return;
    _hasCapturedEnabled = true;
    _capturedEnabledUpdateCount = updateCount;
  }

  internal bool CanOwnPress(uint updateCount) {
    // 今回の入力更新でUIの入力を処理する前に有効だったこと
    if (!_hasCapturedEnabled || _capturedEnabledUpdateCount != updateCount) return false;

    return CanReceivePress(updateCount);
  }

  internal bool CanReceivePress(uint updateCount) {
    // NOTE: 同じ更新の中で有効になった受け手は、控えに乗っていても押下の処理中に開いた画面とみなして外す
    if (_hasEnabledEdge && _enabledEdgeUpdateCount == updateCount) return false;

    return IsInputEnabled();
  }

  internal void ResolveSharedInputAction() {
    if (_disposed) return;
    // NOTE: 入力更新の前は回の数がこれから処理する更新のものになっており、有効になった印を押すとこの更新の押下を落とすため押さない
    var next = UISharedInputActionRegistry.Acquire(_bindingSet);
    if (next == _sharedInputAction) return;

    bool wasPressing = _sharedInputAction != null && _sharedInputAction.RemoveListenerAndPress(this);
    _sharedInputAction = next;
    _sharedInputAction?.AddListener(this);
    // NOTE: 押したまま割り当てを変えると前の押下の離しは届かなくなるため、ここで押下を打ち切ったことを伝える
    if (wasPressing) _onPressCancelSubject.OnNext(Unit.Default);
  }

  internal void NotifyPress(InputAction.CallbackContext context) {
    if (_disposed) return;
    _onInputSubject.OnNext(context);
  }
  internal void NotifyRelease(InputAction.CallbackContext context) {
    if (_disposed) return;
    _onReleaseSubject.OnNext(context);
  }

  private void StampEnabledEdge() {
    _hasEnabledEdge = true;
    _enabledEdgeUpdateCount = InputState.updateCount;
  }

  private void RequestResolve() {
    if (_disposed) return;
    // NOTE: 入力のコールバックの中で共有アクションを作らず、初期化で何度も足しても写しを1つにするため、次の入力更新の前に台帳がまとめてつなぐ
    UISharedInputActionRegistry.RequestResolve(this);
  }
}

}
