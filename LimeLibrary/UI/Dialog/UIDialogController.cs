using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LimeLibrary.Extensions;
using LimeLibrary.UI.Parts;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace LimeLibrary.UI.Dialog {

public class UIDialogController {
  private readonly Stack<UIDialogData> _dialogDataStack = new(8);
  private readonly IEnumerable<IUIView> _allViews;

  private GameObject _backGroundGameObject;
  private Tween _backGroundColorTween;

  public UIDialogController(IEnumerable<IUIView> allViews) {
    _allViews = allViews;
  }

  public bool ExistsDialog() => _dialogDataStack.Any();

  public IUIView GetTopDialog() => ExistsDialog() ? _dialogDataStack.Peek().UIView : null;

  public async UniTask Show(IUIView view, UIDialogOption dialogOption, CancellationToken cancellationToken) {
    _dialogDataStack.Push(new UIDialogData(view, dialogOption));

    // 既存の表示ViewのUnfocus
    foreach (var showedView in _allViews.Where(uiView => uiView.State == UIViewState.Show)) {
      showedView.Unfocus();
    }

    // ポップアップの背景用オブジェクト生成
    if (_backGroundGameObject == null) {
      _backGroundGameObject = UnityUtility.CreateGameObject("PopupBackground");
    }
    SetupBackGround(_backGroundGameObject, _dialogDataStack.Peek());

    // 最前面に表示（ポップアップ用背景より前）
    view.SetSortingOrderFront();

    // 表示
    await view.Show(cancellationToken);

    // 非表示時にリストから削除
    UniTask.Create(async () => {
      await view.EventObservables.GetObservable(UIViewEventType.HideEnd).ToUniTask(true, view.RootObject.GetCancellationTokenOnDestroy());
      Terminate(view);
    }).RunHandlingError().Forget();
  }

  private void Terminate(IUIView uiView) {
    if (!_dialogDataStack.Any() || _dialogDataStack.Peek().UIView != uiView) {
      return;
    }

    _dialogDataStack.Pop();

    if (_dialogDataStack.Any()) {
      SetupBackGround(_backGroundGameObject, _dialogDataStack.Peek());
    } else {
      DisableBackGround(_backGroundGameObject);
    }
    // 既存のViewの復帰
    if (_dialogDataStack.Any()) {
      var otherUiView = _dialogDataStack.Peek().UIView;
      otherUiView.Focus();
      otherUiView.SetSortingOrderFront();
    } else {
      var showedViews = _allViews.Where(view => view.State == UIViewState.Show);
      foreach (var showedUiView in showedViews) {
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
      dialogData.DialogOption.IsDarkenBackground ? new Color(0f, 0f, 0, 0.2f) : new Color(0f, 0f, 0, 0f),
      dialogData.DialogOption.IsDarkenBackground ? 0.2f : 0f,
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
      uiButton.GetEventObservable(UIButtonEventType.PointerUp).FirstOrDefault().Subscribe(_ => {
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

  private void DisableBackGround(GameObject gameObject) {
    // 背景イメージアニメーション
    PlayBackGroundAnimation(gameObject,
      null,
      new Color(0f, 0f, 0, 0f),
      0.2f,
      gameObject.GetCancellationTokenOnDestroy()).RunHandlingError().Forget();
  }

  private async UniTask PlayBackGroundAnimation(GameObject gameObject, Color? startColor, Color endColor, float duration, CancellationToken cancellationToken) {
    var image = gameObject.GetOrAddComponent<Image>();
    if (startColor.HasValue) image.color = startColor.Value;

    _backGroundColorTween?.KillIfActive();
    _backGroundColorTween = image.DOColor(endColor, duration).SetLink(gameObject);
    await _backGroundColorTween.ToUniTask(cancellationToken: cancellationToken);
  }
}

}