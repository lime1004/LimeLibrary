using System;
using UnityEngine;

namespace LimeLibrary.Utility {

/// <summary>
/// 条件を表す
/// </summary>
[Serializable]
public enum Condition {
  Equal,
  LessThan,
  GreaterThan,
  LessThanOrEqual,
  GreaterThanOrEqual,
}

public static class ConditionUtility {
  /// <summary>
  /// 条件定義を使用した比較関数
  /// </summary>
  /// <param name="condition">条件定義</param>
  /// <param name="value1">値1</param>
  /// <param name="value2">値2</param>
  /// <typeparam name="T">比較可能な型</typeparam>
  /// <returns>条件を満たしているか</returns>
  public static bool CheckCondition<T>(Condition condition, T value1, T value2) where T : IComparable {
    switch (condition) {
    case Condition.Equal:
      return value1.CompareTo(value2) == 0;
    case Condition.LessThan:
      return value1.CompareTo(value2) < 0;
    case Condition.GreaterThan:
      return value1.CompareTo(value2) > 0;
    case Condition.LessThanOrEqual:
      return value1.CompareTo(value2) <= 0;
    case Condition.GreaterThanOrEqual:
      return value1.CompareTo(value2) >= 0;
    default:
      Assertion.Assert(false);
      return false;
    }
  }

  public static bool CheckCondition(Condition condition, float value1, float value2) {
    switch (condition) {
    case Condition.Equal:
      return Mathf.Approximately(value1, value2);
    case Condition.LessThan:
      return value1 > value2;
    case Condition.GreaterThan:
      return value1 < value2;
    case Condition.LessThanOrEqual:
      return value1 >= value2;
    case Condition.GreaterThanOrEqual:
      return value1 <= value2;
    default:
      Assertion.Assert(false);
      return false;
    }
  }
}

}