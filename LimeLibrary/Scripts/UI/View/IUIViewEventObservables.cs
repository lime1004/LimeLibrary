using System;
using R3;

namespace LimeLibrary.UI.View {

public interface IUIViewEventObservables {
  public Observable<Unit> GetObservable(UIViewEventType eventType);
}

}