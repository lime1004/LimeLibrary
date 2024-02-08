using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.UI.View;
using UnityEngine;

namespace LimeLibrary.UI.MessageWindow {

public class MessageWindowManager : MonoBehaviour {
  [SerializeField]
  private MessageWindow _messageWindow;
  [SerializeField]
  private Camera _camera;

  public async UniTask Initialize() {
    await _messageWindow.Initialize(_camera);
  }

  // MessageMainWindow ========================================================

  public bool IsShowMessageMainWindow() {
    return _messageWindow.MessageMainWindow.State is UIViewState.Show or UIViewState.Showing;
  }

  public async UniTask ShowMessageMainWindow(MessageWindowType messageWindowType, CancellationToken cancellationToken) {
    if (IsShowMessageMainWindow() && _messageWindow.MessageMainWindow.MessageWindowType == messageWindowType) return;

    var showTaskList = new List<UniTask>();
    if (messageWindowType == MessageWindowType.System) {
      showTaskList.Add(HideSpeakerWindow(cancellationToken));
    }
    _messageWindow.MessageMainWindow.MessageWindowType = messageWindowType;
    showTaskList.Add(_messageWindow.MessageMainWindow.Show(cancellationToken));

    await showTaskList;
  }

  public async UniTask HideMessageMainWindow(CancellationToken cancellationToken) {
    if (!IsShowMessageMainWindow()) return;
    await UniTask.WhenAll(
      HideSpeakerWindow(cancellationToken),
      _messageWindow.MessageMainWindow.Hide(cancellationToken));
  }

  public async UniTask ShowMainText(string text, MessageWindowType messageWindowType, CancellationToken cancellationToken, float durationMultiplier = 1.0f) {
    await UniTask.WhenAll(
      ShowMessageMainWindow(messageWindowType, cancellationToken),
      _messageWindow.MessageMainWindow.ShowText(text, cancellationToken, durationMultiplier));
  }

  public async UniTask WaitMainText(CancellationToken cancellationToken) {
    if (!IsShowMessageMainWindow()) return;
    await _messageWindow.MessageMainWindow.WaitText(cancellationToken);
  }

  public void SkipMainText() {
    if (!IsShowMessageMainWindow()) return;
    _messageWindow.MessageMainWindow.SkipText();
  }

  // KeyWait ==================================================================

  public bool IsShowKeyWait() {
    return _messageWindow.KeyWait.State is UIViewState.Show or UIViewState.Showing;
  }

  public UniTask ShowKeyWait(CancellationToken cancellationToken) {
    if (IsShowKeyWait()) return UniTask.CompletedTask;
    return _messageWindow.KeyWait.Show(cancellationToken);
  }

  public UniTask HideKeyWait(CancellationToken cancellationToken) {
    if (!IsShowKeyWait()) return UniTask.CompletedTask;
    return _messageWindow.KeyWait.Hide(cancellationToken);
  }

  // ChoiceWindow =============================================================

  public bool IsShowChoiceWindow() {
    return _messageWindow.ChoiceWindow.State is UIViewState.Show or UIViewState.Showing;
  }

  public async UniTask ShowChoiceWindow(CancellationToken cancellationToken, bool isEnableCancel, params string[] choiceTextList) {
    if (IsShowChoiceWindow()) return;
    _messageWindow.ChoiceWindow.IsEnableCancel = isEnableCancel;
    _messageWindow.ChoiceWindow.ClearChoice();
    foreach (string choiceText in choiceTextList) {
      _messageWindow.ChoiceWindow.AddChoice(choiceText);
    }
    await _messageWindow.ChoiceWindow.Show(cancellationToken);
  }

  public async UniTask<int> WaitChoiceWindowSelect(CancellationToken cancellationToken) {
    return await _messageWindow.ChoiceWindow.WaitSelect(cancellationToken);
  }

  public UniTask HideChoiceWindow(CancellationToken cancellationToken) {
    if (!IsShowChoiceWindow()) return UniTask.CompletedTask;
    return _messageWindow.ChoiceWindow.Hide(cancellationToken);
  }

  // SpeakerWindow =============================================================

  public bool IsShowSpeakerWindow() {
    return _messageWindow.SpeakerWindow.State is UIViewState.Show or UIViewState.Showing;
  }

  public async UniTask ShowSpeakerWindow(string speakerText, CancellationToken cancellationToken) {
    if (IsShowSpeakerWindow()) return;
    _messageWindow.SpeakerWindow.SetSpeakerText(speakerText);
    await _messageWindow.SpeakerWindow.Show(cancellationToken);
  }

  public async UniTask HideSpeakerWindow(CancellationToken cancellationToken) {
    if (!IsShowSpeakerWindow()) return;
    await _messageWindow.SpeakerWindow.Hide(cancellationToken);
  }
}

}