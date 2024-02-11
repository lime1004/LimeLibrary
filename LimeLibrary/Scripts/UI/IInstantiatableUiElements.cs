using LimeLibrary.UI.View;
using UnityEngine;

namespace LimeLibrary.UI {

public interface IInstantiatableUiElements {
  public MonoBehaviour Self { get; }
  public void Initialize(IUIView parentView);
}

public static class InstantiatableUiElementsExtensions {
  public static T Instantiate<T>(this IInstantiatableUiElements instantiatable, IUIView parentView) where T : MonoBehaviour {
    var instantiateObject = Object.Instantiate(instantiatable.Self, instantiatable.Self.transform.parent);
    instantiatable.Initialize(parentView);
    return instantiateObject as T;
  }
}

}