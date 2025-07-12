using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.UI.View;
using UnityEngine;

namespace LimeLibrary.UI.MessageWindow {

public class KeyWait : UISingleView {
  [SerializeField]
  private Vector2 _animationOffset = new Vector2(0, 10f);
  [SerializeField]
  private float _animationDuration = 0.5f;

  private Vector2 _keyWaitPosition;

  protected UniTask OnInitialize() {
    _keyWaitPosition = transform.AsRectTransform().anchoredPosition;

    return UniTask.CompletedTask;
  }
}

}