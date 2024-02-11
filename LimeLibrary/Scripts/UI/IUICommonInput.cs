using UnityEngine.InputSystem;

namespace LimeLibrary.UI {

public interface IUICommonInput {
  public InputAction GetDecideInputAction();
  public InputAction GetCancelInputAction();
}

}