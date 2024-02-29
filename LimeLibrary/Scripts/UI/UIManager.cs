using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.Module;
using LimeLibrary.UI.MessageWindow;
using UnityEngine;

namespace LimeLibrary.UI {

public class UIManager : SingletonMonoBehaviour<UIManager> {
  [SerializeField]
  private UIAppManager _uiAppManager;
  [SerializeField]
  private UIScreenSpaceManager _uiScreenSpaceManager;
  [SerializeField]
  private UIWorldSpaceManager _uiWorldSpaceManager;
  [SerializeField]
  private MessageWindowManager _messageWindowManager;

  public UIAppManager UIAppManager => _uiAppManager;
  public UIScreenSpaceManager UIScreenSpaceManager => _uiScreenSpaceManager;
  public UIWorldSpaceManager UIWorldSpaceManager => _uiWorldSpaceManager;
  public MessageWindowManager MessageWindowManager => _messageWindowManager;

  protected override void Awake() {
    MessageWindowManager.Initialize().RunHandlingError().Forget();
  }

  private void Update() {
    UIAppManager.OnUpdate();
    UIWorldSpaceManager.OnUpdate();
  }
}

}