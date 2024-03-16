using System;
using LimeLibrary.Input;
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
  private class ControllerInputBindingImageDictionary : SerializedDictionary<ControllerType, InputBindingImageDictionary> { }

  [SerializeField]
  private ControllerInputBindingImageDictionary _dictionary;

  public bool ContainsImage(InputBindingType inputBindingType, ControllerType controllerType) {
    return _dictionary.ContainsKey(controllerType) &&
      _dictionary[controllerType].ContainsKey(inputBindingType);
  }

  public bool ContainsImage(string inputBindingPath) {
    foreach (var (_, dictionary) in _dictionary) {
      foreach (var (bindingType, _) in dictionary) {
        string path = InputBindingPath.Get(bindingType);
        if (path == inputBindingPath) {
          return true;
        }
      }
    }
    return false;
  }

  public bool ContainsImage(string inputBindingPath, ControllerType controllerType) {
    if (!_dictionary.ContainsKey(controllerType)) return false;
    foreach (var (type, _) in _dictionary[controllerType]) {
      string path = InputBindingPath.Get(type);
      if (path == inputBindingPath) {
        return true;
      }
    }
    return false;
  }

  public Sprite GetImage(InputBindingType inputBindingType, ControllerType controllerType) {
    if (!ContainsImage(inputBindingType, controllerType)) {
      Assertion.Assert(false, "Image is not registered. " + inputBindingType + ":" + controllerType);
      return null;
    }
    return _dictionary[controllerType][inputBindingType];
  }

  public Sprite GetImage(string inputBindingPath, ControllerType controllerType) {
    if (!_dictionary.ContainsKey(controllerType)) return null;
    foreach (var (type, sprite) in _dictionary[controllerType]) {
      string path = InputBindingPath.Get(type);
      if (path == inputBindingPath) {
        return sprite;
      }
    }

    Assertion.Assert(false, "画像が登録されていません. " + inputBindingPath + ":" + controllerType);
    return null;
  }
}

}