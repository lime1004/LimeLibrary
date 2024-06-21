using System.Collections.Generic;

namespace LimeLibrary.Debug.ImGui {

public interface IMenu {
  public List<IMenuContent> MenuContents { get; }

  public string GetMenuName();
  public void Initialize(WindowManager windowManager);
  public void Update();
  public void Destroy();

}

}