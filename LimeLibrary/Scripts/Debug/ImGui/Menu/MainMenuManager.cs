using System.Collections.Generic;

namespace LimeLibrary.Debug.ImGui {

public class MainMenuManager {
  private readonly MainMenuBar _mainMenuBar;
  private readonly WindowManager _windowManager;
  private readonly List<IMenu> _menuList = new();

  public MainMenuManager(MainMenuBar mainMenuBar, WindowManager windowManager) {
    _mainMenuBar = mainMenuBar;
    _windowManager = windowManager;
  }

  public void Register(IMenu menu, int order = 0) {
    menu.Initialize(_windowManager);
    foreach (var menuContent in menu.MenuContents) {
      menuContent.Initialize();
    }

    _mainMenuBar.AddMainMenu(menu.GetMenuName(), () => {
      foreach (var menuContent in menu.MenuContents) {
        menuContent.Execute();
      }
    }, order);

    _menuList.Add(menu);
  }

  public void OnUpdate() {
    _mainMenuBar.Update();
    foreach (var menu in _menuList) {
      menu.Update();
    }
  }

  public void OnDestroy() {
    foreach (var menu in _menuList) {
      foreach (var menuContent in menu.MenuContents) {
        menuContent.OnDestroy();
      }
      menu.Destroy();
    }
  }
}

}