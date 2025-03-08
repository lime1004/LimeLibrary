using LimeLibrary.Input;
using R3;

namespace LimeLibrary.UI {

public class UIInputObservables : IUIInputObservables {
  public InputMode CurrentInputMode => InputModeUpdater.Instance.InputMode;
  public ControllerType CurrentControllerType => InputModeUpdater.Instance.ControllerType;
  public Observable<InputMode> OnChangeInputModeObservable => InputModeUpdater.Instance.OnChangeInputModeObservable;
}

}