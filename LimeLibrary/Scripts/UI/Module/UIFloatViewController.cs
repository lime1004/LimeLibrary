using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.UI.Parts;
using LimeLibrary.UI.View;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace LimeLibrary.UI.Module {

public class UIFloatViewController : IDisposable {
  private readonly IUIView _parentView;
  private readonly UnityEngine.UI.Selectable _targetSelectable;
  private readonly IUIView _floatView;
  private readonly InputAction _mousePosition;
  private readonly UIEventReceiver _uiEventReceiver;
  private readonly Subject<IUIView> _onShowFloatViewSubject = new();
  private readonly CompositeDisposable _compositeDisposable = new();

  private bool _isMouseOver;

  public IObservable<IUIView> OnShowFloatViewObservable => _onShowFloatViewSubject;
  public bool Enabled { get; set; } = true;

  public UIFloatViewController(IUIView parentView, UnityEngine.UI.Selectable targetSelectable, IUIView floatView, InputAction mousePosition) {
    _parentView = parentView;
    _targetSelectable = targetSelectable;
    _floatView = floatView;
    _mousePosition = mousePosition;

    // マウスオーバーイベントを取るためUIEventReceiverをセットアップ
    _uiEventReceiver = targetSelectable.gameObject.GetOrAddComponent<UIEventReceiver>();
    _uiEventReceiver.Initialize(parentView);

    // 親View非表示時に非表示にする
    parentView.OnHideEndObservable.Subscribe(_ => {
      HideFloatView(floatView, floatView.ObjectCancellationToken).RunHandlingError().Forget();
    }).AddTo(parentView.RootObject).AddTo(_compositeDisposable);

    SetupShowMouseOver();
    SetupShowGamepadSelect();
  }

  public void SetupShowMouseOver() {
    // マウスオーバー開始時
    _uiEventReceiver.GetObservable(EventTriggerType.PointerEnter).Subscribe(data => {
      if (!Enabled) return;

      var worldPosition = ScreenPointToWorld(data.PointerEventData.position);
      Show(worldPosition, _floatView.ObjectCancellationToken).RunHandlingError().Forget();
      _isMouseOver = true;
    }).AddTo(_parentView.RootObject).AddTo(_compositeDisposable);

    // マウスオーバー解除時
    _uiEventReceiver.GetObservable(EventTriggerType.PointerExit).Subscribe(data => {
      if (!Enabled) return;

      Hide(_floatView.ObjectCancellationToken).RunHandlingError().Forget();
      _isMouseOver = false;
    }).AddTo(_parentView.RootObject).AddTo(_compositeDisposable);

    // マウスオーバー中は座標更新
    _parentView.RootObject.UpdateAsObservable().Where(_ => _isMouseOver).Subscribe(_ => {
      if (!Enabled) return;

      var mousePosition = _mousePosition.ReadValue<Vector2>();
      var worldPosition = ScreenPointToWorld(mousePosition);
      _floatView.SetPosition(worldPosition);
    }).AddTo(_parentView.RootObject).AddTo(_compositeDisposable);
  }

  public void SetupShowGamepadSelect() {
    // ゲームパッド用に選択時に詳細表示
    _targetSelectable.OnSelectAsObservable().Subscribe(_ => {
      if (!Enabled) return;

      Show(_targetSelectable.transform.position, _parentView.ObjectCancellationToken).RunHandlingError().Forget();
    }).AddTo(_parentView.RootObject).AddTo(_compositeDisposable);

    // ゲームパッド用に選択外し時に詳細非表示
    _targetSelectable.OnDeselectAsObservable().Subscribe(_ => {
      if (!Enabled) return;

      Hide(_parentView.ObjectCancellationToken).RunHandlingError().Forget();
    }).AddTo(_parentView.RootObject).AddTo(_compositeDisposable);
  }

  public async UniTask Show(CancellationToken cancellationToken) {
    await ShowFloatView(_floatView, null, cancellationToken);
  }

  public async UniTask Show(Vector2 position, CancellationToken cancellationToken) {
    await ShowFloatView(_floatView, position, cancellationToken);
  }

  public async UniTask Hide(CancellationToken cancellationToken) {
    await HideFloatView(_floatView, cancellationToken);
  }

  private Vector3 ScreenPointToWorld(Vector2 screenPoint) {
    RectTransformUtility.ScreenPointToWorldPointInRectangle(_parentView.RectTransform, screenPoint, _parentView.Canvas.worldCamera, out var worldPosition);
    return worldPosition;
  }

  private async UniTask ShowFloatView(IUIView floatView, Vector2? position, CancellationToken cancellationToken) {
    _onShowFloatViewSubject.OnNext(floatView);
    if (position.HasValue) floatView.SetPosition(position.Value);
    // 描画順を一番前にする
    floatView.SetSortingOrderFront();
    await floatView.Show(cancellationToken);
  }

  private async UniTask HideFloatView(IUIView floatView, CancellationToken cancellationToken) {
    await floatView.Hide(cancellationToken);
  }

  public void Dispose() {
    _compositeDisposable.Dispose();
  }
}

}