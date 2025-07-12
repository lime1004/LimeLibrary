using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.UI.View;
using TMPro;
using R3;
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

  private CancellationTokenSource _cancellationTokenSource;
  private string _playingText = string.Empty;
  private bool _isShowingText;

  public MessageWindowType MessageWindowType { get; set; }

  protected override UniTask OnInitialize(CancellationToken cancellationToken) {
    EventObservables.GetObservable(UIViewEventType.ShowStart).Subscribe(MessageWindowType, (_, windowType) => {
      _windowImage.sprite = windowType switch {
        MessageWindowType.System => _systemMessageWindowSprite,
        MessageWindowType.Talk => _talkMessageWindowSprite,
        _ => null,
      };
    }).AddTo(gameObject);

    return UniTask.CompletedTask;
  }

  public async UniTask<bool> ShowText(string text, CancellationToken cancellationToken, float durationMultiplier = 1.0f) {
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource = new CancellationTokenSource();
    var mergedCancellationTokenToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token).Token;

    _text.text = string.Empty;
    _playingText = text;
    _isShowingText = true;
    float duration = _messageWindowSettings.ShowTextDurationEveryChar * text.Length * durationMultiplier;
    bool isCancel = await _text.PlayTextTween(text, duration, mergedCancellationTokenToken).SuppressCancellationThrow();
    _isShowingText = false;
    return isCancel;
  }

  public async UniTask WaitText(CancellationToken cancellationToken) {
    await UniTask.WaitUntil(() => !_isShowingText, cancellationToken: cancellationToken);
  }

  public void SkipText() {
    _cancellationTokenSource?.Cancel();
    _text.text = _playingText;
  }
}

}