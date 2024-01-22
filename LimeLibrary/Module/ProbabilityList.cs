#if LIME_ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace LimeLibrary.Module {

/// <summary>
/// 確率を設定可能なリスト
/// </summary>
[Serializable, HideLabel]
public class ProbabilityList<T> {
  [SerializeField, LabelText("確率リスト"), OnValueChanged("SetProbabilityParameterParameter", true)]
  private List<Probability<T>> _probabilityList = new();
  public IReadOnlyList<Probability<T>> List => _probabilityList;

  public T Lottery(RandomInstance randomInstance = null) {
    return _probabilityList.Lottery(randomInstance);
  }

  public List<T> Lottery(int lotteryNum, RandomInstance randomInstance = null, bool isIgnoreLotteryed = false) {
    return _probabilityList.Lottery(lotteryNum, randomInstance, isIgnoreLotteryed);
  }

  [OnInspectorInit]
  private void SetProbabilityParameterParameter() {
    foreach (var probability in _probabilityList) {
      probability.SetTotalWeight(_probabilityList);
    }
  }
}

[Serializable, HideLabel]
public class ProbabilityPreviewList<T> {
  [SerializeField, LabelText("確率リスト"), OnValueChanged("SetProbabilityParameterParameter", true)]
  private List<ProbabilityPreview<T>> _probabilityList = new();
  public IReadOnlyList<ProbabilityPreview<T>> List => _probabilityList;

  public T Lottery(RandomInstance randomInstance = null) {
    return _probabilityList.Lottery(randomInstance);
  }

  public List<T> Lottery(int lotteryNum, RandomInstance randomInstance = null, bool isIgnoreLotteryed = false) {
    return _probabilityList.Lottery(lotteryNum, randomInstance, isIgnoreLotteryed);
  }

  [OnInspectorInit]
  private void SetProbabilityParameterParameter() {
    foreach (var probability in _probabilityList) {
      probability.SetTotalWeight(_probabilityList);
    }
  }
}

}
#endif