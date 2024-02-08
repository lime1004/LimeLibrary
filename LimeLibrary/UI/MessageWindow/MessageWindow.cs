using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LimeLibrary.UI.MessageWindow {

public class MessageWindow : MonoBehaviour {
  [SerializeField]
  private Canvas _canvas;
  [SerializeField]
  private MessageWindowSettings _messageWindowSettings;
  [SerializeField]
  private MessageMainWindow _messageMainWindow;
  [SerializeField]
  private KeyWait _keyWait;
  [SerializeField]
  private ChoiceWindow _choiceWindow;
  [SerializeField]
  private SpeakerWindow _speakerWindow;

  public MessageMainWindow MessageMainWindow => _messageMainWindow;
  public KeyWait KeyWait => _keyWait;
  public ChoiceWindow ChoiceWindow => _choiceWindow;
  public SpeakerWindow SpeakerWindow => _speakerWindow;

  public async UniTask Initialize(Camera uiCamera) {
    gameObject.SetActive(true);

    // Canvas
    _canvas.worldCamera = uiCamera;
    _canvas.sortingLayerName = "UI";

    // 各パーツ
    var cancellationToken = gameObject.GetCancellationTokenOnDestroy();
    await UniTask.WhenAll(
      _messageMainWindow.Initialize(cancellationToken),
      _keyWait.Initialize(cancellationToken),
      _choiceWindow.Initialize(cancellationToken),
      _speakerWindow.Initialize(cancellationToken));
  }
}

}