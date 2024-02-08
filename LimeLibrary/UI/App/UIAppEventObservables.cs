using System;
using System.Collections.Generic;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using UniRx;

namespace LimeLibrary.UI.App {

internal class UIAppEventObservables : IUIAppEventNotifier, IUIAppEventObservables {
  private readonly Dictionary<UIAppEventType, Subject<Unit>> _subjects = new();

  public UIAppEventObservables() {
    foreach (UIAppEventType eventType in Enum.GetValues(typeof(UIViewEventType))) {
      _subjects.Add(eventType, new Subject<Unit>());
    }
  }

  public IObservable<Unit> GetObservable(UIAppEventType eventType) {
    if (_subjects.TryGetValue(eventType, out var subject)) {
      return subject;
    }
    Assertion.Assert(false, $"Not found {eventType} in {nameof(UIViewEventObservables)}");
    return null;
  }

  public void Notify(UIAppEventType eventType) {
    _subjects[eventType].OnNext(Unit.Default);
  }
}

}