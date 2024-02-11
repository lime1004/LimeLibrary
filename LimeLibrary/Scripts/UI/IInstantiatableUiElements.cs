using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using UnityEngine;

namespace LimeLibrary.UI {

public interface IInstantiatableUiElements<out T> where T : MonoBehaviour {
  public T Self { get; }

  public T Instantiate(IUIView parentView) {
    var instantiateObject = UnityUtility.Instantiate(Self, Self.transform.parent);
    Setup(parentView);
    return instantiateObject;
  }

  public void Setup(IUIView parentView);
}

}