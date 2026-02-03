using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.UI.Parts;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LimeLibrary.UI.Dialog {

public class UIDialogController {
  private readonly List<UIDialogData> _dialogDataList = new(8);
  private readonly IEnumerable<IUIView> _allViews;

  private GameObject _backGroundGameObject;
  private CancellationTokenSource _cancellationTokenSource;

  public UIDialogController(IEnumerable<IUIView> allViews) {
    _allViews = allViews;
  }

  public bool ExistsDialog() => _dialogDataList.Count > 0;
  public IUIView GetTopDialog() => ExistsDialog() ? _dialogDataList[^1].UIView : null;

  public bool ExistsDialog(IUIView view) {
    foreach (var dialogData in _dialogDataList) {
      if (dialogData.UIView == view) return true;
    }
    return false;
  }

  public async UniTask Show(IUIView view, UIDialogOption dialogOption, CancellationToken cancellationToken) {
    if (ExistsDialog(view)) {
      Assertion.Assert(false, $"Dialog is already shown: {view.RootObject.name}");
      return;
    }

    var dialogData = new UIDialogData(view, dialogOption);

    // 既存の表示ViewのUnfocus
    foreach (var showedView in _allViews) {
      if (showedView.State != UIViewState.Show) continue;
      showedView.Unfocus();
    }

    // Selectedを外す
    EventSystem.current.SetSelectedGameObject(null);

    // ポップアップの背景用オブジェクト生成
    if (_backGroundGameObject == null) {
      _backGroundGameObject = UnityUtility.CreateGameObject("PopupBackground");
    }
    SetupBackGround(_backGroundGameObject, dialogData);

    // 最前面に表示（ポップアップ用背景より前）
    view.SetSortingOrderFront();

    _dialogDataList.Add(dialogData);

    // 表示
    await view.Show(cancellationToken);

    // 非表示時処理
    view.EventObservables.GetObservable(UIViewEventType.HideStart).Take(1).Subscribe(view, (_, view) => OnHideStart(view)).AddTo(view.ObjectCancellationToken);
    view.EventObservables.GetObservable(UIViewEventType.HideEnd).Take(1).Subscribe(_ => OnHideEnd()).AddTo(view.ObjectCancellationToken);
  }

  private void OnHideStart(IUIView uiView) {
    _dialogDataList.RemoveAll(data => data.UIView == uiView);

    // 背景画像処理
    var currentDialogData = _dialogDataList[^1];
    if (_dialogDataList.Count > 0) {
      SetupBackGround(_backGroundGameObject, currentDialogData);
    } else {
      DisableBackGround(_backGroundGameObject, currentDialogData).RunHandlingError().Forget();
    }
  }

  private void OnHideEnd() {
    // Focus処理
    if (_dialogDataList.Count > 0) {
      // 先頭のダイアログにFocus
      var otherDialog = _dialogDataList[^1].UIView;
      otherDialog.Focus();
      otherDialog.SetSortingOrderFront();
    } else {
      // 既存の表示ViewにFocus
      foreach (var showedUiView in _allViews) {
        if (showedUiView.State != UIViewState.Show) continue;
        showedUiView.Focus();
      }
    }
  }

  private void SetupBackGround(GameObject gameObject, UIDialogData dialogData) {
    gameObject.SetActive(true);
    gameObject.layer = LayerMask.NameToLayer("UI");
    gameObject.transform.SetParent(dialogData.UIView.RootObject.transform.parent, true);
    gameObject.transform.localScale = Vector3.one;
    // 最前面に表示する
    gameObject.transform.SetAsLastSibling();

    // 背景イメージアニメーション
    PlayBackGroundAnimation(gameObject,
      new Color(0f, 0f, 0, 0f),
      dialogData.DialogOption.BackgroundColor,
      dialogData.DialogOption.BackgroundAnimationDuration,
      gameObject.GetCancellationTokenOnDestroy()).RunHandlingError().Forget();

    // ボタン追加
    var button = gameObject.GetOrAddComponent<Button>();
    var uiButton = gameObject.GetOrAddComponent<UIButton>();
    if (dialogData.DialogOption.IsHideOnClickBackground) {
      button.transition = Selectable.Transition.None;
      button.enabled = true;
      uiButton.Initialize(dialogData.UIView);
      uiButton.DisableNavigation();
      // 背景クリック時閉じる処理
      uiButton.GetEventObservable(UIButtonEventType.PointerUp).Take(1).Subscribe(_ => {
        dialogData.UIView.Hide(gameObject.GetCancellationTokenOnDestroy()).RunHandlingError().Forget();
      }).AddTo(gameObject);
    } else {
      button.enabled = false;
      uiButton.enabled = false;
    }

    if (gameObject.TryGetComponent(out RectTransform rectTransform)) {
      // Stretchに変更
      rectTransform.localPosition = Vector3.zero;
      rectTransform.anchorMin = new Vector2(0, 0);
      rectTransform.anchorMax = new Vector2(1, 1);
      rectTransform.offsetMin = new Vector2(0, 0);
      rectTransform.offsetMax = new Vector2(0, 0);
      rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }
  }

  private async UniTask DisableBackGround(GameObject gameObject, UIDialogData dialogData) {
    // 背景イメージアニメーション
    await PlayBackGroundAnimation(gameObject,
      null,
      new Color(0f, 0f, 0, 0f),
      dialogData.DialogOption.BackgroundAnimationDuration,
      gameObject.GetCancellationTokenOnDestroy());

    gameObject.SetActive(false);
  }

  private async UniTask PlayBackGroundAnimation(GameObject gameObject, Color? startColor, Color endColor, float duration, CancellationToken cancellationToken) {
    var image = gameObject.GetOrAddComponent<Image>();
    if (startColor.HasValue) image.color = startColor.Value;

    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    await image.PlayColorTween(endColor, duration, _cancellationTokenSource.Token);
  }
}

}