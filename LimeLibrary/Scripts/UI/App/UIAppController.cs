using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace LimeLibrary.UI.App {

internal class UIAppController {
  private readonly List<UIView> _viewList;
  private readonly IEnumerable<UIView> _showViews;
  private readonly IUIAppEventNotifier _eventNotifier;

  public UIAppState State { get; private set; } = UIAppState.Default;
  public GameObject RootObject { get; }
  public RectTransform RectTransform { get; }
  public Canvas Canvas { get; }
  public CanvasGroup CanvasGroup { get; }
  public CanvasScaler CanvasScaler { get; }
  public IReadOnlyList<UIView> Views => _viewList;

  private CancellationTokenSource _cancellationTokenSourceOnShowHide;

  public UIAppController(GameObject rootObject, IUIAppEventNotifier eventNotifier, List<UIView> viewList, IEnumerable<UIView> showViews) {
    RootObject = rootObject;
    RectTransform = rootObject.transform.AsRectTransform();
    Canvas = rootObject.GetComponent<Canvas>();
    CanvasGroup = rootObject.GetComponent<CanvasGroup>();
    CanvasScaler = rootObject.GetComponent<CanvasScaler>();
    _eventNotifier = eventNotifier;
    _viewList = viewList;
    _showViews = showViews;
  }

  public async UniTask InitializeView(UIApp parentApp, CancellationToken cancellationToken) {
    // Viewを全て非アクティブにする
    foreach (var view in Views) {
      view.gameObject.SetActive(false);
    }

    // View初期化
    foreach (var view in Views) {
      await view.Initialize(parentApp, cancellationToken);
    }

    _eventNotifier.Notify(UIAppEventType.InitializeView);
  }

  public async UniTask Show(UIAppShowOption showOption, CancellationToken cancellationToken) {
    if (State is UIAppState.Show or UIAppState.Showing) return;

    _cancellationTokenSourceOnShowHide?.Cancel();
    _cancellationTokenSourceOnShowHide = new CancellationTokenSource();
    _cancellationTokenSourceOnShowHide.RegisterRaiseCancelOnDestroy(RootObject);

    var mergedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSourceOnShowHide.Token, cancellationToken);

    State = UIAppState.Showing;

    _eventNotifier.Notify(UIAppEventType.ShowStart);

    // 表示処理
    var showTask = new List<UniTask>();
    foreach (var view in _showViews) {
      showTask.Add(view.Show(new UIViewShowOption {
        IsImmediate = showOption.IsImmediate,
        IsFocus = false,
      }, mergedTokenSource.Token));
    }
    await showTask;

    // フォーカス処理
    var focusOrderedViews = _showViews.OrderBy(view => view.AdvanceSettings.FocusPriority).ToList();
    foreach (var view in focusOrderedViews) {
      if (view == focusOrderedViews[0]) view.Focus();
      else view.Unfocus();
    }

    State = UIAppState.Show;

    _eventNotifier.Notify(UIAppEventType.ShowEnd);
  }

  public async UniTask Hide(UIAppHideOption hideOption, CancellationToken cancellationToken) {
    if (State is UIAppState.Hide or UIAppState.Hiding) return;

    _cancellationTokenSourceOnShowHide?.Cancel();
    _cancellationTokenSourceOnShowHide = new CancellationTokenSource();
    _cancellationTokenSourceOnShowHide.RegisterRaiseCancelOnDestroy(RootObject);

    var mergedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSourceOnShowHide.Token, cancellationToken);

    State = UIAppState.Hiding;
    _eventNotifier.Notify(UIAppEventType.HideStart);

    // 非表示処理
    var hideTaskList = new List<UniTask>();
    foreach (var view in Views) {
      hideTaskList.Add(view.Hide(new UIViewHideOption {
        IsImmediate = hideOption.IsImmediate,
      }, mergedTokenSource.Token));
    }
    await hideTaskList;

    State = UIAppState.Hide;
    _eventNotifier.Notify(UIAppEventType.HideEnd);
  }

  public async UniTask HideAndDestroy(CancellationToken cancellationToken) {
    await Hide(new UIAppHideOption(), cancellationToken);
    Destroy(false);
  }

  public void Destroy(bool isApplicationQuitDestroy) {
    if (State == UIAppState.Destroy) return;

    foreach (var view in Views) {
      view.OnDestroyView();
    }

    _eventNotifier.Notify(UIAppEventType.Destroy);
    _eventNotifier.Notify(isApplicationQuitDestroy ?
      UIAppEventType.ApplicationQuitDestroy :
      UIAppEventType.NormalDestroy);

    if (!isApplicationQuitDestroy) Object.Destroy(RootObject);

    State = UIAppState.Destroy;
  }

  public T GetView<T>(bool containsSubClass = true, int? id = null) where T : UIView {
    foreach (var view in Views) {
      if (id.HasValue && view.AdvanceSettings.Id != id.Value) continue;
      if (containsSubClass && view is T getView) return getView;
      if (!containsSubClass && view.GetType() == typeof(T)) return view as T;
    }
    return null;
  }

  public bool ExistsView<T>(bool isCheckSubClass = false, bool isCheckInterface = false) where T : UIView {
    return Views.Any(view => {
      if (view is T) return true;
      if (isCheckSubClass && view.GetType().IsSubclassOf(typeof(T))) return true;
      if (isCheckInterface && view.GetType().GetInterfaces().Contains(typeof(T))) return true;
      return false;
    });
  }

  public async UniTask<T> DuplicateView<T>(UIApp app, T view, CancellationToken cancellationToken) where T : UIView {
    var duplicatedView = UnityUtility.Instantiate(view, view.RectTransform.parent);
    await duplicatedView.Initialize(app, cancellationToken);
    _viewList.Add(duplicatedView);
    return duplicatedView;
  }
}

}