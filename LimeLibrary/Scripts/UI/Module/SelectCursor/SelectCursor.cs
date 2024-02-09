using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using UnityEngine;

namespace LimeLibrary.UI.Module.SelectCursor {

public class SelectCursor : ISelectCursor {
  private readonly IUIView _parentView;
  private readonly List<SelectCursorObjectData> _cursorObjectDataList = new(4);
  private CancellationTokenSource _cancellationTokenSource;

  public bool IsNextMoveImmediate { get; set; }
  public bool IsInvalidEnable { get; set; }
  public SelectCursorMoveOption MoveOption { get; } = new();

  public SelectCursor(IUIView parentView, bool isNextMoveImmediateFirst = true) {
    _parentView = parentView;
    IsNextMoveImmediate = isNextMoveImmediateFirst;
  }

  public SelectCursor(IUIView parentView, GameObject cursorObject, bool isNextMoveImmediateFirst = true) : this(parentView, isNextMoveImmediateFirst) {
    AddCursorObject(cursorObject);
  }

  public SelectCursorObjectData AddCursorObject(GameObject cursorObject, Vector2 offset = new Vector2()) {
    var cursorObjectData = new SelectCursorObjectData(cursorObject, offset);
    AddCursorObject(cursorObjectData);
    return cursorObjectData;
  }

  public void AddCursorObject(SelectCursorObjectData cursorObjectData) {
    _cursorObjectDataList.Add(cursorObjectData);
  }

  public void MoveCursor(GameObject targetObject, bool isImmediately = false) {
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource = new CancellationTokenSource();
    foreach (var cursorObjectData in _cursorObjectDataList) {
      MoveCursor(cursorObjectData, targetObject, isImmediately, _cancellationTokenSource.Token).RunHandlingError().Forget();
    }
  }

  private async UniTask MoveCursor(SelectCursorObjectData cursorObjectData, GameObject targetObject, bool isImmediately, CancellationToken cancellationToken) {
    if (!cursorObjectData.Enabled || cursorObjectData.CursorObject == null) return;

    cursorObjectData.ResetMoveCounter();
    cursorObjectData.TargetObject = targetObject.transform;
    cursorObjectData.IsFollowTarget = false;

    var cursorObject = cursorObjectData.CursorObject;

    var targetRectTransform = targetObject.transform.AsRectTransform();
    var cursorRectTransform = cursorObject.transform.AsRectTransform();

    isImmediately |= IsNextMoveImmediate;
    IsNextMoveImmediate = false;

    bool isFinish = false;
    while (!isFinish && !cancellationToken.IsCancellationRequested) {
      // 時間算出
      cursorObjectData.AddMoveCounter(Time.deltaTime);
      float t = MoveOption.Duration != 0f ? cursorObjectData.MoveCounter / MoveOption.Duration : 1f;
      if (isImmediately) t = 1f;

      // 座標算出
      var fromPosition = cursorRectTransform.position;
      var toPosition = targetRectTransform.position + cursorObjectData.Offset.ToVector3();
      cursorRectTransform.position = UnityUtility.Lerp(fromPosition, toPosition, t, MoveOption.Ease);

      await UniTask.NextFrame(cancellationToken: cancellationToken);

      // 終了判定
      if (t >= 1f || cursorRectTransform == null || targetRectTransform == null) {
        isFinish = true;
      }
    }

    cursorObjectData.IsFollowTarget = true;
  }

  public void SetEnabled(bool enabled) {
    if (enabled) {
      EnableCursor();
    } else {
      DisableCursor();
    }
  }

  public void EnableCursor() {
    if (IsInvalidEnable) return;
    foreach (var cursorObjectData in _cursorObjectDataList) {
      cursorObjectData.Enabled = true;
    }
  }

  public void DisableCursor() {
    foreach (var cursorObjectData in _cursorObjectDataList) {
      cursorObjectData.Enabled = false;
    }
  }
}

}