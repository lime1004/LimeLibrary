using System.Collections.Generic;
using UnityEngine;

namespace LimeLibrary.Module {

public class ColorStacker {
  private readonly Dictionary<string, Color> _colorDictionary = new(32);

  public void SetColor(string id, Color color) {
    _colorDictionary[id] = color;
  }

  public void RemoveColor(string id) {
    if (_colorDictionary.ContainsKey(id)) {
      _colorDictionary.Remove(id);
    }
  }

  public Color GetColor(string id) {
    return _colorDictionary.TryGetValue(id, out var color) ? color : Color.white;
  }

  public Color GetColor() {
    Color? finalColor = null;
    foreach (var (_, stackColor) in _colorDictionary) {
      if (finalColor == null) {
        finalColor = stackColor;
      } else {
        finalColor = finalColor.Value * stackColor;
      }
    }
    return finalColor ?? Color.white;
  }
}

}