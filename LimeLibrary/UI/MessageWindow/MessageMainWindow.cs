using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LimeLibrary.UI.View;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace LimeLibrary.UI.MessageWindow {

public class MessageMainWindow : UISingleView {
  [SerializeField]
  private MessageWindowSettings _messageWindowSettings;
  [SerializeField]
  private Image _windowImage;
  [SerializeField]
  private TextMeshProUGUI _text;
  [SerializeField]
  private Sprite _systemMessageWindowSprite;
  [SerializeField]
  private Sprite _talkMessageWindowSprite;

  private Tween _textTween;

  public MessageWindowType MessageWindowType { get; set; }

  protected override UniTask OnInitialize(CancellationToken cancellationToken) {
    Animator.RegisterShowHideFadeAnimation(CanvasGroup, 0.1f);

    EventObservables.GetObservable(UIViewEventType.ShowStart).SubscribeWithState(MessageWindowType, (_, windowType) => {
      _windowImage.sprite = windowType switch {
        MessageWindowType.System => _systemMessageWindowSprite,
        MessageWindowType.Talk => _talkMessageWindowSprite,
        _ => null,
      };
    }).AddTo(gameObject);

    return UniTask.CompletedTask;
  }

  public async UniTask ShowText(string text, CancellationToken cancellationToken, float durationMultiplier = 1.0f) {
    if (_textTween?.IsActive() ?? false) _textTween.Kill();
    _text.text = string.Empty;
    float duration = _messageWindowSettings.ShowTextDurationEveryChar * text.Length * durationMultiplier;
    _textTween = _text.DOText(text, duration).SetEase(Ease.Linear).SetLink(_text.gameObject);
    await _textTween.ToUniTask(cancellationToken: cancellationToken);
  }

  public async UniTask WaitText(CancellationToken cancellationToken) {
    await UniTask.WaitUntil(() => !_textTween.IsActive(), cancellationToken: cancellationToken);
  }

  public void SkipText() {
    if (_textTween?.IsActive() ?? false) _textTween.Kill(true);
  }
}

}