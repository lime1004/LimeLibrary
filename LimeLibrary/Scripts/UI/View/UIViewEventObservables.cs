using System;
using System.Collections.Generic;
using FastEnumUtility;
using LimeLibrary.Utility;
using R3;

namespace LimeLibrary.UI.View {

internal class UIViewEventObservables : IUIViewEventNotifier, IUIViewEventObservables {
  private readonly Dictionary<UIViewEventType, Subject<Unit>> _subjects = new();

  public UIViewEventObservables() {
    foreach (var eventType in FastEnum.GetValues<UIViewEventType>()) {
      _subjects.Add(eventType, new Subject<Unit>());
    }
  }

  public Observable<Unit> GetObservable(UIViewEventType eventType) {
    if (_subjects.TryGetValue(eventType, out var subject)) {
      return subject;
    }
    Assertion.Assert(false, $"Not found {eventType} in {nameof(UIViewEventObservables)}");
    return null;
  }

  public void Notify(UIViewEventType eventType) {
    _subjects[eventType].OnNext(Unit.Default);
  }
}

}