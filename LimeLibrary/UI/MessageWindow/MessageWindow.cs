using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LimeLibrary.UI.MessageWindow {

public class MessageWindow : MonoBehaviour {
  [SerializeField]
  private MessageWindowSettings _messageWindowSettings;
  [SerializeField]
  private MessageMainWindow m_messageMainWindow;
  [SerializeField]
  private KeyWait _keyWait;
  [SerializeField]
  private ChoiceWindow _choiceWindow;
  [SerializeField]
  private SpeakerWindow m_speakerWindow;

  private Canvas m_canvas;

  public MessageMainWindow MessageMainWindow => m_messageMainWindow;
  public KeyWait KeyWait => _keyWait;
  public ChoiceWindow ChoiceWindow => _choiceWindow;
  public SpeakerWindow SpeakerWindow => m_speakerWindow;

  public async UniTask Initialize(Camera uiCamera) {
    gameObject.SetActive(true);

    // Canvas
    m_canvas = GetComponent<Canvas>();
    m_canvas.worldCamera = uiCamera;
    m_canvas.sortingLayerName = "UI";

    // 各パーツ
    var cancellationToken = gameObject.GetCancellationTokenOnDestroy();
    await UniTask.WhenAll(
      m_messageMainWindow.Initialize(cancellationToken),
      _keyWait.Initialize(cancellationToken),
      _choiceWindow.Initialize(cancellationToken),
      m_speakerWindow.Initialize(cancellationToken));
  }
}

}