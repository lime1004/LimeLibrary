#if LIME_ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LimeLibrary.Module {

/// <summary>
/// 確率インターフェース
/// </summary>
public interface IProbability<out T> {
  public T Content { get; }
  public float Weight { get; }
  public bool IsFixedLottery { get; }
}

/// <summary>
/// 確率クラス
/// </summary>
[Serializable]
public abstract class ProbabilityBase<T> : IProbability<T> {
  public abstract T Content { get; }

  [SerializeField, LabelText("確定抽選")]
  protected bool _isFixedLottery;
  public bool IsFixedLottery => _isFixedLottery;

  [SerializeField, LabelText("重み"), HideIf("$IsFixedLottery"), Range(0, 100)]
  protected float _weight = 1f;
  public float Weight => _weight;

  [NonSerialized, ShowInInspector, CustomValueDrawer("DrawPercentage")]
  private float _percentage;

  private IEnumerable<ProbabilityBase<T>> _probabilityList;
  private float _totalWeight;

#if UNITY_EDITOR
  private float DrawPercentage(float value, GUIContent label) {
    if (_isFixedLottery) return 1f;
    if (_totalWeight == 0f) return 0f;
    float percentage = _weight / _totalWeight;
    EditorGUILayout.LabelField("確率", $"{percentage:P2}");
    return percentage;
  }
#endif

  public void SetTotalWeight(IEnumerable<ProbabilityBase<T>> probabilityParameterList) {
    _probabilityList = probabilityParameterList;
    _totalWeight = _probabilityList.Sum(parameter => parameter.IsFixedLottery ? 0f : parameter._weight);
  }
}

[Serializable]
public class Probability<T> : ProbabilityBase<T> {
  public Probability() { }

  public Probability(T content, bool isFixedLottery, float weight) {
    _content = content;
    _isFixedLottery = isFixedLottery;
    _weight = weight;
  }

  [SerializeField, LabelText("内容"), PropertyOrder(-1)]
  private T _content;
  public override T Content => _content;
}

[Serializable]
public class ProbabilityPreview<T> : ProbabilityBase<T> {
  public ProbabilityPreview() { }

  public ProbabilityPreview(T content, bool isFixedLottery, float weight) {
    _content = content;
    _isFixedLottery = isFixedLottery;
    _weight = weight;
  }

  [SerializeField, LabelText("内容"), PreviewField, PropertyOrder(-1)]
  private T _content;
  public override T Content => _content;
}

/// <summary>
/// 確率拡張関数定義クラス
/// </summary>
public static class ProbabilityParameterExtensions {
  /// <summary>
  /// 抽選パラメータから1つ抽選
  /// </summary>
  public static T Lottery<T>(this IReadOnlyList<IProbability<T>> probabilityList, RandomInstance randomInstance = null) {
    float totalWeight = CalcTotalWeight(probabilityList);
    float random = randomInstance?.Range(0f, totalWeight) ?? UnityEngine.Random.Range(0f, totalWeight);
    int probabilityCount = probabilityList.Count;

    // 確定抽選判定
    for (int i = 0; i < probabilityCount; i++) {
      if (probabilityList[i].IsFixedLottery) {
        return probabilityList[i].Content;
      }
    }

    // 抽選判定
    float totalRnd = 0f;
    IProbability<T> result = null;
    for (int i = 0; i < probabilityCount; i++) {
      totalRnd += probabilityList[i].Weight;
      if (!(random < totalRnd)) continue;
      result = probabilityList[i];
      break;
    }

    return result == null ? default : result.Content;
  }

  /// <summary>
  /// 抽選パラメータから複数抽選
  /// </summary>
  public static List<T> Lottery<T>(this IReadOnlyList<IProbability<T>> probabilityList, int lotteryNum, RandomInstance randomInstance = null, bool isIgnoreLotteryed = false) {
    var lotteryedList = new List<IProbability<T>>();
    for (int i = 0; i < lotteryNum; i++) {
      float totalWeight = CalcTotalWeight(probabilityList, isIgnoreLotteryed ? lotteryedList : null);
      float random = randomInstance?.Range(0f, totalWeight) ?? UnityEngine.Random.Range(0f, totalWeight);
      int probabilityCount = probabilityList.Count;

      // 確定抽選判定
      bool isFixedLotteryed = false;
      for (int k = 0; k < probabilityCount; k++) {
        var probability = probabilityList[k];
        if (probability.IsFixedLottery && !lotteryedList.Contains(probability)) {
          lotteryedList.Add(probability);
          isFixedLotteryed = true;
          break;
        }
      }
      if (isFixedLotteryed) continue;

      // 抽選判定
      float totalRnd = 0f;
      for (int k = 0; k < probabilityCount; k++) {
        var probability = probabilityList[k];
        if (isIgnoreLotteryed && lotteryedList.Contains(probability)) continue;

        totalRnd += probability.Weight;
        if (random < totalRnd) {
          lotteryedList.Add(probability);
          break;
        }
      }
    }

    return lotteryedList.Select(probability => probability.Content).ToList();
  }

  private static float CalcTotalWeight<T>(this IReadOnlyList<IProbability<T>> probabilityList, List<IProbability<T>> ignoreProbabilityList = null) {
    float totalWeight = 0f;
    int count = probabilityList.Count;
    for (int i = 0; i < count; i++) {
      if (ignoreProbabilityList?.Contains(probabilityList[i]) ?? false) continue;
      totalWeight += probabilityList[i].Weight;
    }
    return totalWeight;
  }
}

}
#endif