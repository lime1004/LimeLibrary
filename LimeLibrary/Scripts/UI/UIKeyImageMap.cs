using System;
using LimeLibrary.Input;
using LimeLibrary.Input.InputController;
using LimeLibrary.Module;
using LimeLibrary.Utility;
using UnityEngine;

namespace LimeLibrary.UI {

[Serializable, CreateAssetMenu(
   fileName = "UIKeyImageDictionary",
   menuName = "LimeLibrary/UI/UIKeyImageDictionary")]
public class UIKeyImageMap : ScriptableObject {
  [Serializable]
  private class InputBindingImageDictionary : SerializedDictionary<InputBindingType, Sprite> { }

  [Serializable]
  private class ControllerInputBindingImageDictionary : SerializedDictionary<InputControllerType, InputBindingImageDictionary> { }

  [SerializeField]
  private InputBindingImageDictionary _defaultDictionary;

  [SerializeField]
  private ControllerInputBindingImageDictionary _overwriteDictionary;

  public bool ContainsImage(InputBindingType inputBindingType, InputControllerType controllerType) {
    return _defaultDictionary.ContainsKey(inputBindingType) ||
      (_overwriteDictionary.ContainsKey(controllerType) && _overwriteDictionary[controllerType].ContainsKey(inputBindingType));
  }

  public bool ContainsImage(string inputBindingPath, InputControllerType controllerType) {
    return ContainsImage(InputBindingPath.From(inputBindingPath), controllerType);
  }

  public Sprite GetImage(InputBindingType inputBindingType, InputControllerType controllerType) {
    if (!ContainsImage(inputBindingType, controllerType)) {
      Assertion.Assert(false, "Image is not registered. " + inputBindingType + ":" + controllerType);
      return null;
    }
    if (_overwriteDictionary.ContainsKey(controllerType) && _overwriteDictionary[controllerType].ContainsKey(inputBindingType)) {
      return _overwriteDictionary[controllerType][inputBindingType];
    } else {
      return _defaultDictionary[inputBindingType];
    }
  }

  public Sprite GetImage(string inputBindingPath, InputControllerType controllerType) {
    return GetImage(InputBindingPath.From(inputBindingPath), controllerType);
  }
}

}