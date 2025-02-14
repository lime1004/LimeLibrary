using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.UI.View;
using TMPro;
using UnityEngine;

namespace LimeLibrary.UI.MessageWindow {

public class SpeakerWindow : UISingleView {
  [SerializeField]
  private TextMeshProUGUI _text;

  protected override UniTask OnInitialize(CancellationToken cancellationToken) {
    Animator.RegisterShowHideFadeAnimation(CanvasGroup, 0.1f);
    return UniTask.CompletedTask;
  }

  public void SetSpeakerText(string speakerText) {
    _text.text = speakerText;
  }
}

}