using System;
using LimeLibrary.System;

namespace LimeLibrary.UI {

public class UIInputObservables : IUIInputObservables {
  public InputMode CurrentInputMode => InputModeUpdater.Instance.InputMode;
  public ControllerType CurrentControllerType => InputModeUpdater.Instance.ControllerType;
  public IObservable<InputMode> OnChangeInputModeObservable => InputModeUpdater.Instance.OnChangeInputModeObservable;
}

}