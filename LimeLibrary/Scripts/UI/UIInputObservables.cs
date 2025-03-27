using LimeLibrary.Input.InputController;
using LimeLibrary.Input.InputMode;
using R3;

namespace LimeLibrary.UI {

public class UIInputObservables : IUIInputObservables {
  public IInputMode CurrentInputMode => InputModeUpdater.Instance.CurrentInputMode;
  public InputControllerType CurrentControllerType => InputControllerTypeUpdater.Instance.ControllerType;
  public Observable<IInputMode> OnChangeInputModeObservable => InputModeUpdater.Instance.OnChangeInputModeObservable;
}

}