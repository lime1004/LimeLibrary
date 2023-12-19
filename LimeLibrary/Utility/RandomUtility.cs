using System.Collections.Generic;
using UnityEngine;

namespace LimeLibrary.Utility {

public static class RandomUtility {
  /// <summary>
  /// Vector2.x <= n < Vector2.yの値を取得
  /// </summary>
  public static float Range(Vector2 range) {
    return Random.Range(range.x, range.y);
  }

  /// <summary>
  /// 範囲取得
  /// min <= n < max
  /// </summary>
  public static int Range(int min, int max) {
    return Random.Range(min, max);
  }

  /// <summary>
  /// Vector2.x <= n < Vector2.yの値を取得
  /// </summary>
  public static int Range(Vector2Int range, bool isInclusiveY) {
    if (isInclusiveY) range.y++;
    return Random.Range(range.x, range.y);
  }

  /// <summary>
  /// 指定した範囲から複数の値をランダムに取得（重複なし） 
  /// </summary>
  public static int[] RangeMulti(int minInclusive, int maxExclusive, int num) {
    var numbers = new List<int>();
    int[] result = new int[num];
    for (int i = minInclusive; i < maxExclusive; i++) numbers.Add(i);
    num = Mathf.Clamp(num, 0, numbers.Count);

    for (int i = 0; i < num; i++) {
      int index = Random.Range(0, numbers.Count);
      result[i] = numbers[index];
      numbers.RemoveAt(index);
    }
    return result;
  }

  /// <summary>
  /// ○分の1の確率を取得
  /// </summary>
  public static bool Fraction(int value) {
    return Random.Range(0, value) == 0;
  }

  /// <summary>
  /// int型のランダム値を取得
  /// </summary>
  public static int ValueInt(int max = 1000000) {
    return (int) (Random.value * max);
  }
}

}