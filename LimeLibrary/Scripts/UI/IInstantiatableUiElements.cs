using LimeLibrary.UI.View;
using UnityEngine;

namespace LimeLibrary.UI {

public interface IInstantiatableUiElements<out T> where T : MonoBehaviour {
  public T Self { get; }
  public void Initialize(IUIView parentView);
}

public static class InstantiatableUiElementsExtensions {
  public static T Instantiate<T>(this IInstantiatableUiElements<T> instantiatable, IUIView parentView) where T : MonoBehaviour, IInstantiatableUiElements<T> {
    var instantiateObject = Object.Instantiate(instantiatable.Self, instantiatable.Self.transform.parent);
    instantiateObject.Initialize(parentView);
    return instantiateObject;
  }
}

}