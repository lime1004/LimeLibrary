using LimeLibrary.Extensions;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace LimeLibrary.UI.Module.SelectCursor {

public class SelectCursorObjectData {
  private bool _enabled = true;

  public GameObject CursorObject { get; }
  public Vector2 Offset { get; set; }
  public ISelectCursorResidentAnimator ResidentAnimator { get; set; }

  public bool Enabled {
    get => _enabled;
    set {
      if (_enabled == value) return;
      _enabled = value;
      CursorObject.SetActive(value);
      if (value) {
        ResidentAnimator?.Play();
      } else {
        ResidentAnimator?.Stop();
      }
    }
  }
  public float MoveCounter { get; private set; }
  public Transform TargetObject { get; set; }
  public bool IsFollowTarget { get; set; }

  public SelectCursorObjectData(GameObject cursorObject, Vector2 offset = new(), ISelectCursorResidentAnimator residentAnimator = null) {
    CursorObject = cursorObject;
    Offset = offset;
    ResidentAnimator = residentAnimator;
    Enabled = false;

    cursorObject.UpdateAsObservable().Where(_ => IsFollowTarget).Subscribe(_ => {
      if (TargetObject == null) {
        Enabled = false;
        return;
      }
      if (!TargetObject.gameObject.activeInHierarchy) return;

      CursorObject.transform.position = TargetObject.transform.position + Offset.ToVector3();
    }).AddTo(cursorObject);
  }

  public void AddMoveCounter(float deltaTime) => MoveCounter += deltaTime;
  public void ResetMoveCounter() => MoveCounter = 0;
}

}