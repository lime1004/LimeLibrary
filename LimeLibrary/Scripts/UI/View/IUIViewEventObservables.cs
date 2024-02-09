using System;
using UniRx;

namespace LimeLibrary.UI.View {

public interface IUIViewEventObservables {
  public IObservable<Unit> GetObservable(UIViewEventType eventType);
}

}