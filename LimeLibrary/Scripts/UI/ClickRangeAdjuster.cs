using LimeLibrary.UI.Parts;
using UnityEngine;

namespace LimeLibrary.UI {

public class ClickRangeAdjuster {
  private GameObject _graphicCastObject;

  public void Enable(RectTransform parentTransform) {
    if (_graphicCastObject) Disable();

    _graphicCastObject = new GameObject("ClickRangeAdjustObject");
    _graphicCastObject.transform.SetParent(parentTransform, false);
    var rectTransform = _graphicCastObject.AddComponent<RectTransform>();
    rectTransform.sizeDelta = parentTransform.rect.size;
    _graphicCastObject.AddComponent<CanvasRenderer>();
    _graphicCastObject.AddComponent<GraphicCast>();
  }

  public void Disable() {
    Object.Destroy(_graphicCastObject);
  }
}

}