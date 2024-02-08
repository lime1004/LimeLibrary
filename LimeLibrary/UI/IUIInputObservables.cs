using System;
using LimeLibrary.System;

namespace LimeLibrary.UI {

public interface IUIInputObservables {
  public InputMode CurrentInputMode { get; }
  public ControllerType CurrentControllerType { get; }
  public IObservable<InputMode> OnChangeInputModeObservable { get; }
}

}