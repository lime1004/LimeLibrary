using System.Collections.Generic;
using System.Linq;
using LimeLibrary.Utility;
using UnityEngine;
using Random = System.Random;

namespace LimeLibrary.Module {

/// <summary>
/// インスタンス化できる乱数クラス
/// </summary>
public class RandomInstance {
  private readonly Random _random;

  public RandomInstance() : this(RandomUtility.ValueInt()) { }

  public RandomInstance(int seed) {
    _random = new Random(seed);
  }

  /// <summary>
  /// 値取得
  /// </summary>
  public int Value() {
    return _random.Next();
  }

  /// <summary>
  /// 値取得
  /// n < max
  /// </summary>
  public int Next(int max) {
    return _random.Next(max);
  }

  /// <summary>
  /// 範囲取得
  /// min <= n < max
  /// </summary>
  public int Range(int min, int max) {
    return _random.Next(min, max);
  }

  /// <summary>
  /// 範囲取得
  /// range.x <= n <= range.y
  /// </summary>
  public int Range(Vector2Int range, bool isInclusiveY = true) {
    return isInclusiveY ? _random.Next(range.x, range.y + 1) : _random.Next(range.x, range.y);
  }

  /// <summary>
  /// 範囲取得
  /// min <= n < max
  /// </summary>
  public float Range(float min, float max) {
    float sub = max - min;
    float rand = (float) _random.NextDouble();
    return min + sub * rand;
  }

  /// <summary>
  /// 範囲取得
  /// range.x <= n < range.y
  /// </summary>
  public float Range(Vector2 range) {
    return Range(range.x, range.y);
  }

  /// <summary>
  /// ○分の1の確率を取得
  /// </summary>
  public bool Fraction(int value) {
    return Range(0, value) == 0;
  }

  /// <summary>
  /// リストからランダムで取得
  /// </summary>
  public T ListRandom<T>(List<T> list) {
    return !list.Any() ? default : list[Next(list.Count)];
  }
}

}