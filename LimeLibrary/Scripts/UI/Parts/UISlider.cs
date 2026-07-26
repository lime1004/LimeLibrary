using System;
using Cysharp.Threading.Tasks;
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
  // NOTE: R3のOnValueChangedAsObservableは購読時に現在値を1回流すため使わない
  public Observable<float> OnChangeValueObservable => Slider.onValueChanged.AsObservable(Slider.GetCancellationTokenOnDestroy());

  public void Initialize(IUIView parentView) {
    if (_isInitialized) return;

    ParentView = parentView;

    Slider = GetComponent<Slider>();

    _isInitialized = true;
  }

  public void SetValue(float value) {
    Slider.SetValueWithoutNotify(value);
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