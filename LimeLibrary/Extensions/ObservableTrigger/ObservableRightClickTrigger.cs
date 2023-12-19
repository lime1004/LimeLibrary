#if LIME_UNIRX
using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LimeLibrary.Extensions {

public class ObservableRightClickTrigger : MonoBehaviour, IPointerClickHandler {
  private readonly Subject<Unit> _onRightClickSubject = new Subject<Unit>();

  public IObservable<Unit> OnRightClickAsObservable() {
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