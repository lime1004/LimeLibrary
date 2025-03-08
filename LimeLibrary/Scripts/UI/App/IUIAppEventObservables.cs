using R3;

namespace LimeLibrary.UI.App {

public interface IUIAppEventObservables {
  public Observable<Unit> GetObservable(UIAppEventType eventType);
}

}