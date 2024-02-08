using System.Collections.Generic;
using LimeLibrary.Module;
using UnityEngine;
using UnityEngine.UI;

namespace LimeLibrary.UI.Module.Selectable.SelectableAppearance {

public class ColorChanger : SelectableAppearance {
  private readonly Dictionary<Graphic, ColorStacker> _targetGraphicDictionary = new();

  public Color MultiplyColor { get; set; } = Color.white;
  public Color? OverwriteColor { get; set; }
  public bool IsEnableRevert { get; set; } = false;

  public ColorChanger() { }

  public ColorChanger(Color overwriteColor) {
    OverwriteColor = overwriteColor;
  }

  protected override void OnApplyAppearance() {
    foreach (var (graphic, colorStacker) in _targetGraphicDictionary) {
      if (graphic == null) continue;
      colorStacker.SetColor("BaseColor", OverwriteColor ?? colorStacker.GetColor("BaseColor"));
      colorStacker.SetColor("MultiplyColor", MultiplyColor);
      graphic.color = colorStacker.GetColor();
    }
  }

  protected override void OnRevertAppearance() {
    if (!IsEnableRevert) return;

    foreach (var (graphic, colorStacker) in _targetGraphicDictionary) {
      if (graphic == null) continue;
      if (OverwriteColor.HasValue) colorStacker.RemoveColor("BaseColor");
      colorStacker.RemoveColor("MultiplyColor");
    }
  }

  public void AddTargetGraphic(Graphic graphic) {
    var colorStacker = new ColorStacker();
    colorStacker.SetColor("BaseColor", graphic.color);
    _targetGraphicDictionary.Add(graphic, colorStacker);
  }

  public void AddTargetGraphic(IEnumerable<Graphic> graphics) {
    foreach (var graphic in graphics) {
      AddTargetGraphic(graphic);
    }
  }
}

}