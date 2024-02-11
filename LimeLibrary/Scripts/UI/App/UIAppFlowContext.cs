using System.Threading;
using LimeLibrary.UI.App;
using LimeLibrary.UI.Module.Input;

namespace LimeLibrary.UI {

public class UIAppFlowContext {
  public UIApp UIApp { get; set; }
  public UIInputReceiver CancelInputReceiver { get; set; }
  public CancellationTokenSource CancellationTokenSource { get; set; }
  public bool IsEnd { get; set; }
}

}