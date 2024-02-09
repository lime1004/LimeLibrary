﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.System;
using LimeLibrary.UI.App;
using LimeLibrary.Utility;
using UniRx;
using UnityEngine;

namespace LimeLibrary.UI {

/// <summary>
/// UIAppの生成・管理
/// </summary>
public class UIAppManager : MonoBehaviour {
  [SerializeField]
  private Camera _uiCamera;
  [SerializeField]
  private SortingLayer _sortingLayer;

  private class UIAppData {
    public UIApp UIApp { get; }
    public string Address { get; }
    public bool IsShowOnStack { get; set; } = true;

    public UIAppData(UIApp uiApp, string address) {
      UIApp = uiApp;
      Address = address;
    }
  }

  private readonly Stack<List<UIAppData>> _appDataListStack = new();
  private readonly List<UIApp> _residentAppList = new();

  public async UniTask<UIApp> CreateAppAsync(string address, UIAppAwakeType appAwakeType, CancellationToken cancellationToken, bool isShow = true) {
    var appObject = await ResourceLoader.InstantiateAsync(address, cancellationToken, transform);

    // カメラ設定
    var rootCanvas = appObject.GetOrAddComponent<Canvas>();
    rootCanvas.worldCamera = _uiCamera;
    rootCanvas.sortingLayerID = _sortingLayer.id;

    // App生成時処理
    var uiApp = appObject.GetComponent<UIApp>();
    if (uiApp) {
      OnCreateApp(uiApp, address, appAwakeType);
    } else {
      Assertion.Assert(false, "UIAppが見つかりません.");
      return null;
    }
    // 削除時にリストから削除
    uiApp.EventObservables.GetObservable(UIAppEventType.Destroy).Subscribe(_ => OnDestroyUIApp(uiApp)).AddTo(gameObject);

    // Appの初期化
    await uiApp.InitializeAsync(cancellationToken);

    // 即時表示
    if (isShow) await uiApp.Show(default);

    return uiApp;
  }

  private void OnCreateApp(UIApp uiApp, string address, UIAppAwakeType appAwakeType) {
    switch (appAwakeType) {
    case UIAppAwakeType.Parallel:
      // 直前のAppは保持したまま表示
      if (!_appDataListStack.Any()) _appDataListStack.Push(new List<UIAppData>());
      _appDataListStack.Peek().Add(new UIAppData(uiApp, address));
      break;
    case UIAppAwakeType.Stack:
      // 直前のAppを非表示にする
      if (_appDataListStack.Any()) {
        foreach (var hideUIAppData in _appDataListStack.Peek()) {
          hideUIAppData.IsShowOnStack = hideUIAppData.UIApp.State is UIAppState.Show or UIAppState.Showing;
          hideUIAppData.UIApp.Hide(gameObject.GetCancellationTokenOnDestroy()).RunHandlingError().Forget();
        }
      }
      _appDataListStack.Push(new List<UIAppData>());
      _appDataListStack.Peek().Add(new UIAppData(uiApp, address));
      break;
    case UIAppAwakeType.Resident:
      _residentAppList.Add(uiApp);
      break;
    default:
      Assertion.Assert(false);
      break;
    }
  }

  private void OnDestroyUIApp(UIApp destroyedUiApp) {
    if (_residentAppList.Contains(destroyedUiApp)) {
      _residentAppList.Remove(destroyedUiApp);
      return;
    }

    if (!_appDataListStack.Any()) {
      Assertion.Assert(false);
      return;
    }

    var appList = _appDataListStack.Peek();

    if (!appList.Exists(data => data.UIApp == destroyedUiApp)) {
      foreach (var uiAppList in _appDataListStack) {
        uiAppList.RemoveAll(app => app.UIApp == destroyedUiApp);
      }
      return;
    }

    appList.RemoveAll(data => data.UIApp == destroyedUiApp);
    if (appList.Any()) return;

    _appDataListStack.Pop();
    if (!_appDataListStack.Any()) return;

    foreach (var uiApp in _appDataListStack.Peek()) {
      if (!uiApp.IsShowOnStack) continue;
      uiApp.UIApp.Show(gameObject.GetCancellationTokenOnDestroy()).RunHandlingError().Forget();
    }
  }

  public void OnUpdate() {
    foreach (var residentApp in _residentAppList) {
      residentApp.OnUpdate();
    }

    if (_appDataListStack.Count != 0) {
      var updateAppList = _appDataListStack.Peek();
      foreach (var uiApp in updateAppList) {
        if (!uiApp.UIApp.IsInitialized) continue;
        uiApp.UIApp.OnUpdate();
      }
    }
  }

  public UIApp GetUIApp(string address, bool isIncludeInactive = false) {
    if (!isIncludeInactive) {
      foreach (var uiAppData in _appDataListStack.Peek()) {
        if (uiAppData.Address == address) {
          return uiAppData.UIApp;
        }
      }
    } else {
      foreach (var uiAppDataList in _appDataListStack) {
        foreach (var uiAppData in uiAppDataList) {
          if (uiAppData.Address == address) {
            return uiAppData.UIApp;
          }
        }
      }
    }

    return null;
  }

  public T GetUIApp<T>(bool isIncludeInactive = false) where T : UIApp {
    if (!isIncludeInactive) {
      foreach (var uiAppData in _appDataListStack.Peek()) {
        if (uiAppData.UIApp is T targetUIApp) {
          return targetUIApp;
        }
      }
    } else {
      foreach (var uiAppDataList in _appDataListStack) {
        foreach (var uiAppData in uiAppDataList) {
          if (uiAppData.UIApp is T targetUIApp) {
            return targetUIApp;
          }
        }
      }
    }
    return null;
  }

  public bool ExistsUIApp(string address) {
    if (_appDataListStack.Count <= 0) return false;
    foreach (var uiAppList in _appDataListStack) {
      foreach (var uiAppData in uiAppList) {
        if (uiAppData.Address == address) return true;
      }
    }
    return false;
  }

  public bool ExistsUIApp<T>() where T : UIApp {
    if (_appDataListStack.Count <= 0) return false;
    foreach (var uiAppList in _appDataListStack) {
      foreach (var uiAppData in uiAppList) {
        if (uiAppData.UIApp is T) return true;
      }
    }
    return false;
  }

  public void ShowTop() {
    if (_appDataListStack.Count == 0) return;
    var showUiAppList = _appDataListStack.Peek();
    if (showUiAppList == null) return;
    foreach (var uiApp in showUiAppList) {
      uiApp.UIApp.Show(gameObject.GetCancellationTokenOnDestroy()).RunHandlingError().Forget();
    }
  }

  public void HideTop() {
    if (_appDataListStack.Count == 0) return;
    var showedUiAppList = _appDataListStack.Peek();
    if (showedUiAppList == null) return;
    foreach (var uiApp in showedUiAppList) {
      uiApp.UIApp.Hide(gameObject.GetCancellationTokenOnDestroy()).RunHandlingError().Forget();
    }
  }
}

}