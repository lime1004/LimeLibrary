using System;
using System.Collections.Generic;
using FastEnumUtility;
using LimeLibrary.Extensions;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LimeLibrary.UI.Parts {

public class UIEventReceiver : MonoBehaviour, IUIParts, ISelectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISubmitHandler {
  private readonly Dictionary<EventTriggerType, EventTriggerData> _eventTriggerDataDictionary = new();

  private class EventTriggerData {
    public readonly Subject<EventReceivedData> _subject = new();
  }

  public class EventReceivedData {
    public BaseEventData BaseEventData { get; set; }
    public PointerEventData PointerEventData { get; set; }
  }

  private bool _isInitialized;

  public IUIView ParentView { get; private set; }
  public RectTransform RectTransform => transform.AsRectTransform();

  public bool Enabled { get; set; } = true;

  public void Initialize(IUIView parentView) {
    if (_isInitialized) return;

    ParentView = parentView;

    var eventTriggerTypes = FastEnum.GetValues<EventTriggerType>();
    for (int i = 0; i < eventTriggerTypes.Count; i++) {
      var eventTriggerType = eventTriggerTypes[i];
      if (eventTriggerType is
        EventTriggerType.Select or EventTriggerType.PointerEnter or EventTriggerType.PointerExit or
        EventTriggerType.PointerDown or EventTriggerType.PointerUp or EventTriggerType.Submit) {
        var eventTriggerData = new EventTriggerData();
        _eventTriggerDataDictionary.Add(eventTriggerType, eventTriggerData);
      }
    }

    _isInitialized = true;
  }

  public Observable<EventReceivedData> GetObservable(EventTriggerType eventTriggerType) {
    if (!_isInitialized) {
      Assertion.Assert(false, "Not initialized.");
      return Observable.Never<EventReceivedData>();
    }
    if (!_eventTriggerDataDictionary.ContainsKey(eventTriggerType)) {
      Assertion.Assert(false, "Not support eventTriggerType:" + eventTriggerType);
      return Observable.Never<EventReceivedData>();
    }

    return _eventTriggerDataDictionary[eventTriggerType]._subject;
  }

  private bool IsEnable() {
    if (!Enabled) return false;

    if (!gameObject.activeInHierarchy) return false;
    if (ParentView == null) return false;
    if (!ParentView.IsEnable()) return false;

    return true;
  }

  public void OnSelect(BaseEventData eventData) {
    if (!IsEnable()) return;
    _eventTriggerDataDictionary[EventTriggerType.Select]._subject.OnNext(CreateEventReceivedData(eventData));
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (!IsEnable()) return;
    _eventTriggerDataDictionary[EventTriggerType.PointerEnter]._subject.OnNext(CreateEventReceivedData(eventData));
  }

  public void OnPointerExit(PointerEventData eventData) {
    if (!IsEnable()) return;
    _eventTriggerDataDictionary[EventTriggerType.PointerExit]._subject.OnNext(CreateEventReceivedData(eventData));
  }

  public void OnPointerDown(PointerEventData eventData) {
    if (!IsEnable()) return;
    _eventTriggerDataDictionary[EventTriggerType.PointerDown]._subject.OnNext(CreateEventReceivedData(eventData));
  }

  public void OnPointerUp(PointerEventData eventData) {
    if (!IsEnable()) return;
    _eventTriggerDataDictionary[EventTriggerType.PointerUp]._subject.OnNext(CreateEventReceivedData(eventData));
  }

  public void OnSubmit(BaseEventData eventData) {
    if (!IsEnable()) return;
    _eventTriggerDataDictionary[EventTriggerType.Submit]._subject.OnNext(CreateEventReceivedData(eventData));
  }

  private static EventReceivedData CreateEventReceivedData(BaseEventData baseEventData) {
    return new EventReceivedData {
      BaseEventData = baseEventData,
      PointerEventData = baseEventData as PointerEventData
    };
  }
}

}