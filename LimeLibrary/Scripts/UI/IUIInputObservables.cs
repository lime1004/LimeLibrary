using LimeLibrary.Input.InputController;
using LimeLibrary.Input.InputMode;
using R3;

namespace LimeLibrary.UI {

public interface IUIInputObservables {
  public IInputMode CurrentInputMode { get; }
  public InputControllerType CurrentControllerType { get; }
  public Observable<IInputMode> OnChangeInputModeObservable { get; }
}

}