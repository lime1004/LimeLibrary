using System.Threading;
using LimeLibrary.UI.App;

namespace LimeLibrary.UI {

public class UIAppFlowContext {
  public UIApp UIApp { get; set; }
  public CancellationTokenSource CancellationTokenSource { get; set; }
  public bool IsEnd { get; set; }
}

}