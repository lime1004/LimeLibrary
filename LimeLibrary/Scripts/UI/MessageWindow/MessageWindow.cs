using Cysharp.Threading.Tasks;
using LimeLibrary.Attributes;
using UnityEngine;

namespace LimeLibrary.UI.MessageWindow {

public class MessageWindow : MonoBehaviour {
  [SerializeField]
  private Canvas _canvas;
  [SortingLayer]
  private SortingLayer _sortingLayer;
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
    _canvas.sortingLayerName = _sortingLayer.name;

    // 各パーツ
    var cancellationToken = gameObject.GetCancellationTokenOnDestroy();
    await UniTask.WhenAll(
      _messageMainWindow.Initialize(cancellationToken),
      _keyWait.Initialize(cancellationToken),
      _choiceWindow.Initialize(cancellationToken),
      _speakerWindow.Initialize(cancellationToken));
  }

  public void SetCanvasSortingLayer(string sortingLayerName) {
    _canvas.sortingLayerID = SortingLayer.NameToID(sortingLayerName);
  }

  public void SetCanvasSortingOrder(int sortingOrder) {
    _canvas.sortingOrder = sortingOrder;
  }
}

}