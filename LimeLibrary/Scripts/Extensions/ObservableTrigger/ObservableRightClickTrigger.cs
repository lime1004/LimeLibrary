#if LIME_R3
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LimeLibrary.Extensions {

public class ObservableRightClickTrigger : MonoBehaviour, IPointerClickHandler {
  private readonly Subject<Unit> _onRightClickSubject = new Subject<Unit>();

  public Observable<Unit> OnRightClickAsObservable() {
    return _onRightClickSubject;
  }

  public void OnPointerClick(PointerEventData eventData) {
    if (eventData.button == PointerEventData.InputButton.Right) {
      _onRightClickSubject.OnNext(Unit.Default);
    }
  }
}

}
#endif