using System;
using System.Collections.Generic;
using LimeLibrary.Utility;
using UniRx;

namespace LimeLibrary.UI.View {

internal class UIViewEventObservables : IUIViewEventNotifier, IUIViewEventObservables {
  private readonly Dictionary<UIViewEventType, Subject<Unit>> _subjects = new();

  public UIViewEventObservables() {
    foreach (UIViewEventType eventType in Enum.GetValues(typeof(UIViewEventType))) {
      _subjects.Add(eventType, new Subject<Unit>());
    }
  }

  public IObservable<Unit> GetObservable(UIViewEventType eventType) {
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