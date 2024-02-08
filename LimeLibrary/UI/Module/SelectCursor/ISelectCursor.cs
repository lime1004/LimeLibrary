using UnityEngine;

namespace LimeLibrary.UI.Module.SelectCursor {

public interface ISelectCursor {
  public void MoveCursor(GameObject targetObject, bool isImmediately = false);
  public void EnableCursor();
  public void DisableCursor();
}

}