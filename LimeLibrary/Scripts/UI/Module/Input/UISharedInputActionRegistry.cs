using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace LimeLibrary.UI.Module.Input {

internal static class UISharedInputActionRegistry {
  private static readonly Dictionary<string, UISharedInputAction> s_sharedInputActionMap = new();
  private static readonly List<UISharedInputAction> s_rebuildBuffer = new();
  private static readonly List<UIInputReceiver> s_pendingResolveList = new();
  private static readonly HashSet<UIInputReceiver> s_pendingResolveSet = new();
  private static readonly List<UIInputReceiver> s_resolveBuffer = new();
  private static bool s_isSubscribed;
  private static bool s_isRebuilding;
  private static bool s_hasCapturedUpdate;
  private static uint s_capturedUpdateCount;

  public static UISharedInputAction Acquire(UIInputBindingSet bindingSet) {
    if (bindingSet.IsEmpty) return null;

    string key = bindingSet.CreateKey();
    if (s_sharedInputActionMap.TryGetValue(key, out var sharedInputAction)) return sharedInputAction;

    Subscribe();
    // NOTE: 受け手が後から割り当てを足しても台帳の写しが変わらないよう、作った時点の中身を写して渡す
    sharedInputAction = new UISharedInputAction(bindingSet.Clone());
    s_sharedInputActionMap.Add(key, sharedInputAction);
    return sharedInputAction;
  }

  public static void RequestResolve(UIInputReceiver receiver) {
    Subscribe();
    if (s_pendingResolveSet.Add(receiver)) s_pendingResolveList.Add(receiver);
  }

  public static void CaptureEnabledIfNewUpdate() {
    // NOTE: 比較は一致だけで行う。エディタの更新の間は回の数がエディタ用に切り替わり、大小が意味を持たない
    uint updateCount = InputState.updateCount;
    if (s_hasCapturedUpdate && s_capturedUpdateCount == updateCount) return;
    s_hasCapturedUpdate = true;
    s_capturedUpdateCount = updateCount;

    // この入力更新で最初に受け手へ知らせる直前に、その時点で有効な受け手へ今回の回を押す
    foreach (var sharedInputAction in s_sharedInputActionMap.Values) {
      sharedInputAction.CaptureListenerEnabled(updateCount);
    }
  }

  private static void Subscribe() {
    if (s_isSubscribed) return;
    s_isSubscribed = true;
    InputSystem.onBeforeUpdate += OnBeforeUpdate;
    // 元のアクションの割り当てが変わったら、それを写した共有アクションをまとめて写し直す
    InputSystem.onActionChange += OnActionChange;
  }

  private static void OnBeforeUpdate() {
    // NOTE: アクションはエディタ用と描画前の更新ではイベントを処理せず、状態もエディタ用は別に持つため、ゲームの更新だけで行う
    var updateType = InputState.currentUpdateType;
    if (updateType == InputUpdateType.Editor || updateType == InputUpdateType.BeforeRender) return;

    // 割り当てを変えた受け手のつなぎ直しを、この更新のイベントより前に済ませる
    s_resolveBuffer.Clear();
    s_resolveBuffer.AddRange(s_pendingResolveList);
    s_pendingResolveList.Clear();
    s_pendingResolveSet.Clear();
    foreach (var receiver in s_resolveBuffer) {
      receiver.ResolveSharedInputAction();
    }
    s_resolveBuffer.Clear();

    foreach (var sharedInputAction in s_sharedInputActionMap.Values) {
      if (!sharedInputAction.IsWaitingRelease) continue;
      sharedInputAction.UpdateReleaseWait();
    }
  }

  private static void OnActionChange(object actionOrMapOrAsset, InputActionChange inputActionChange) {
    if (inputActionChange != InputActionChange.BoundControlsChanged) return;
    // NOTE: 写し直しで共有アクション自身の変更通知が来て、写し直しの途中に再入するのを防ぐ
    if (s_isRebuilding) return;

    s_rebuildBuffer.Clear();
    foreach (var sharedInputAction in s_sharedInputActionMap.Values) {
      if (sharedInputAction.IsRebuildTarget(actionOrMapOrAsset)) s_rebuildBuffer.Add(sharedInputAction);
    }

    s_isRebuilding = true;
    try {
      foreach (var sharedInputAction in s_rebuildBuffer) {
        sharedInputAction.RebuildBindings();
      }
    } finally {
      s_isRebuilding = false;
      s_rebuildBuffer.Clear();
    }
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  internal static void ResetStaticState() {
    if (s_isSubscribed) {
      InputSystem.onBeforeUpdate -= OnBeforeUpdate;
      InputSystem.onActionChange -= OnActionChange;
    }
    s_isSubscribed = false;
    s_isRebuilding = false;
    s_hasCapturedUpdate = false;
    s_pendingResolveList.Clear();
    s_pendingResolveSet.Clear();

    foreach (var sharedInputAction in s_sharedInputActionMap.Values) {
      sharedInputAction.Dispose();
    }
    s_sharedInputActionMap.Clear();
  }
}

}
