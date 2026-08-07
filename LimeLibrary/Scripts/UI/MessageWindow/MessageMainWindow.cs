using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using TMPro;
using UnityEngine;

namespace LimeLibrary.UI.MessageWindow {

public class MessageMainWindow : UISingleView {
  [Serializable]
  private class WindowVariant {
    public MessageWindowType Type;
    public GameObject Root;
    public TextMeshProUGUI Text;
    public RectTransform SpeakerAnchor;
    public RectTransform KeyWaitAnchor;
  }

  [SerializeField]
  private MessageWindowSettings _messageWindowSettings;
  [SerializeField]
  private List<WindowVariant> _windowVariants = new List<WindowVariant>();

  private CancellationTokenSource _cancellationTokenSource;
  private string _playingText = string.Empty;
  private bool _isShowingText;
  private int _textGeneration;
  private MessageWindowType _messageWindowType;
  private WindowVariant _activeVariant;

  public MessageWindowType MessageWindowType {
    get => _messageWindowType;
    set {
      _messageWindowType = value;
      ApplyVariant();
    }
  }

  public RectTransform SpeakerAnchor => _activeVariant?.SpeakerAnchor;
  public RectTransform KeyWaitAnchor => _activeVariant?.KeyWaitAnchor;

  protected override UniTask OnInitialize(CancellationToken cancellationToken) {
    ApplyVariant();

    return UniTask.CompletedTask;
  }

  public async UniTask<bool> ShowText(string text, CancellationToken cancellationToken, float durationMultiplier = 1.0f) {
    var variantText = GetActiveText();
    if (variantText == null) return false;

    int generation = BeginText(text);
    using var mergedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);

    variantText.text = string.Empty;
    float duration = _messageWindowSettings.ShowTextDurationEveryChar * text.Length * durationMultiplier;
    bool isCancel = await variantText.PlayTextTween(text, duration, mergedCancellationTokenSource.Token).SuppressCancellationThrow();
    // 後続の再生要求やバリアント切り替えに追い越されていたら状態を戻さない
    if (generation == _textGeneration) _isShowingText = false;
    return isCancel;
  }

  public async UniTask WaitText(CancellationToken cancellationToken) {
    await UniTask.WaitUntil(() => !_isShowingText, cancellationToken: cancellationToken);
  }

  public void SkipText() {
    if (!_isShowingText) return;

    var variantText = GetActiveText();
    if (variantText == null) return;

    _cancellationTokenSource?.Cancel();
    variantText.text = _playingText;
  }

  private int BeginText(string text) {
    CancelAndDisposeTokenSource();
    _cancellationTokenSource = new CancellationTokenSource();
    _playingText = text;
    _isShowingText = true;
    return ++_textGeneration;
  }

  private void StopText() {
    CancelAndDisposeTokenSource();
    _textGeneration++;
    _isShowingText = false;
    _playingText = string.Empty;
  }

  private void CancelAndDisposeTokenSource() {
    if (_cancellationTokenSource == null) return;

    // 破棄済みインスタンスに触れないよう、Cancelとセットで必ずnullへ戻す
    _cancellationTokenSource.Cancel();
    _cancellationTokenSource.Dispose();
    _cancellationTokenSource = null;
  }

  private TextMeshProUGUI GetActiveText() {
    if (_activeVariant == null) return null;
    return _activeVariant.Text;
  }

  private void ApplyVariant() {
    var previousVariant = _activeVariant;
    _activeVariant = null;
    // 現在のタイプに一致するバリアントだけを表示する
    foreach (var windowVariant in _windowVariants) {
      bool isActive = windowVariant.Type == _messageWindowType;
      if (isActive) _activeVariant = windowVariant;
      if (windowVariant.Root != null) windowVariant.Root.SetActive(isActive);
    }
    Assertion.Assert(_activeVariant != null, $"WindowVariant is not found. Type: {_messageWindowType}");

    // 別のバリアントへ移ったら、前のTextで進行中の再生は打ち切る
    if (previousVariant != _activeVariant) StopText();
  }
}

}