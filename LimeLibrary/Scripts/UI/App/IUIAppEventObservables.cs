using System;
using UniRx;

namespace LimeLibrary.UI.App {

public interface IUIAppEventObservables {
  public IObservable<Unit> GetObservable(UIAppEventType eventType);
}

}