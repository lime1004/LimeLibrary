using System;
using LimeLibrary.Extensions;
using LimeLibrary.UI.View;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace LimeLibrary.UI.Parts {

[RequireComponent(typeof(Slider))]
public class UISlider : MonoBehaviour, IUIParts {
  private bool _isInitialized;

  public IUIView ParentView { get; private set; }
  public RectTransform RectTransform => transform.AsRectTransform();

  public Slider Slider { get; private set; }
  public Observable<float> OnChangeValueObservable => Slider.OnValueChangedAsObservable();

  public void Initialize(IUIView parentView) {
    if (_isInitialized) return;

    ParentView = parentView;

    Slider = GetComponent<Slider>();

    _isInitialized = true;
  }

  public void SetValue(float value) {
    Slider.value = value;
  }

  public void SetMinMaxValue(float min, float max) {
    Slider.minValue = min;
    Slider.maxValue = max;
  }

  public void SetIsInteger(bool isInteger) {
    Slider.wholeNumbers = isInteger;
  }

  public float GetValue() {
    return Slider.value;
  }

  public float GetNormalizedValue() {
    return Slider.normalizedValue;
  }
}

}