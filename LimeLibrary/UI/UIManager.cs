using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.Module;
using LimeLibrary.UI.MessageWindow;
using LimeLibrary.Utility;

namespace LimeLibrary.UI {

public class UIManager : SingletonMonoBehaviour<UIManager> {
  public UIAppManager UIAppManager { get; private set; }
  public UIScreenSpaceManager UIScreenSpaceManager { get; private set; }
  public UIWorldSpaceManager UIWorldSpaceManager { get; private set; }
  public MessageWindowManager MessageWindowManager { get; private set; }

  protected override void Awake() {
    UIAppManager = GetComponentInChildren<UIAppManager>();
    if (UIAppManager == null) {
      Assertion.Assert(false, "UIAppManagerが見つかりません.");
    }
    UIScreenSpaceManager = GetComponentInChildren<UIScreenSpaceManager>();
    if (UIScreenSpaceManager == null) {
      Assertion.Assert(false, "UIScreenSpaceManagerが見つかりません.");
    }
    UIWorldSpaceManager = GetComponentInChildren<UIWorldSpaceManager>();
    if (UIWorldSpaceManager == null) {
      Assertion.Assert(false, "UIWorldSpaceManagerが見つかりません.");
    }
    MessageWindowManager = GetComponentInChildren<MessageWindowManager>();
    if (MessageWindowManager == null) {
      Assertion.Assert(false, "MessageWindowManagerが見つかりません.");
    }
    MessageWindowManager.Initialize().RunHandlingError().Forget();
  }

  private void Update() {
    UIAppManager.OnUpdate();
    UIWorldSpaceManager.OnUpdate();
  }
}

}