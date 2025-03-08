using LimeLibrary.Input;
using R3;

namespace LimeLibrary.UI {

public interface IUIInputObservables {
  public InputMode CurrentInputMode { get; }
  public ControllerType CurrentControllerType { get; }
  public Observable<InputMode> OnChangeInputModeObservable { get; }
}

}