using System;
using System.Collections.Generic;
using System.Linq;
using LimeLibrary.Module;
using Random = UnityEngine.Random;

namespace LimeLibrary.Extensions {

/// <summary>
/// C#の拡張メソッド
/// </summary>
public static class CommonExtensions {
  /// <summary>
  /// 文字列の比較
  /// nullと空文字は同じ扱いにする
  /// </summary>
  public static bool EqualsSamenessNullEmpty(this string string1, string string2) {
    if (string.IsNullOrEmpty(string1) && string.IsNullOrEmpty(string2)) {
      return true;
    }
    return string1 == string2;
  }

  /// <summary>
  /// Listのシャッフル
  /// </summary>
  public static List<T> Shuffle<T>(this List<T> list, RandomInstance randomInstance = null) {
    for (int i = list.Count - 1; i > 0; i--) {
      int j = randomInstance?.Next(i + 1) ?? Random.Range(0, i + 1);
      (list[i], list[j]) = (list[j], list[i]);
    }
    return list;
  }

  /// <summary>
  /// 指定されたインデックスに要素が存在するか
  /// </summary>
  public static bool IsDefinedAt<T>(this IReadOnlyCollection<T> list, int index) {
    return index < list.Count;
  }

  /// <summary>
  /// GCAllocしないContains
  /// </summary>
  public static bool Contains<T>(this IReadOnlyList<T> list, T item) where T : IEquatable<T> {
    if (typeof(T).IsValueType) {
      return list.ContainsAsVal(item);
    } else {
      return list.ContainsAsRef(item);
    }
  }

  private static bool ContainsAsVal<T>(this IReadOnlyList<T> items, T item) where T : IEquatable<T> {
    for (int i = 0; i < items.Count; ++i) {
      if (item.Equals(items[i])) {
        return true;
      }
    }

    return false;
  }

  private static bool ContainsAsRef<T>(this IReadOnlyList<T> items, T item) where T : IEquatable<T> {
    if (item == null) {
      for (int i = 0; i < items.Count; ++i) {
        if (items[i] == null) {
          return true;
        }
      }
      return false;
    } else {
      for (int i = 0; i < items.Count; ++i) {
        if (item.Equals(items[i])) {
          return true;
        }
      }
      return false;
    }
  }

  public static bool Contains<T>(this IReadOnlyList<T> items, T item, IEqualityComparer<T> comparer) {
    for (int i = 0; i < items.Count; ++i) {
      if (comparer.Equals(item, items[i])) {
        return true;
      }
    }

    return false;
  }

  public static bool ContainsGeneric<T>(this IReadOnlyList<T> items, T item) {
    // Reference: https://referencesource.microsoft.com/#mscorlib/system/collections/generic/list.cs,316

    if (!typeof(T).IsValueType && item == null) {
      for (int i = 0; i < items.Count; i++)
        if (items[i] == null)
          return true;
      return false;
    } else {
      var c = EqualityComparer<T>.Default;
      for (int i = 0; i < items.Count; i++) {
        if (c.Equals(items[i], item)) return true;
      }
      return false;
    }
  }

  /// <summary>
  /// Listからランダムに値を取得
  /// </summary>
  public static T GetRandom<T>(this IReadOnlyList<T> list, RandomInstance randomInstance = null) {
    if (list == null) return default;
    int randomIndex = randomInstance?.Range(0, list.Count) ?? Random.Range(0, list.Count);
    if (!list.IsDefinedAt(randomIndex)) return default;
    return list[randomIndex];
  }

  /// <summary>
  /// IEnumerableからランダムに値を取得
  /// </summary>
  public static T GetRandom<T>(this IEnumerable<T> enumerable, RandomInstance randomInstance = null) {
    return GetRandom(enumerable.ToList(), randomInstance);
  }

  /// <summary>
  /// 条件によって2つのEnumerableに分ける
  /// </summary>
  public static (IEnumerable<T>, IEnumerable<T>) Split<T>(this IEnumerable<T> enumerable, Func<T, bool> condition) {
    var list = enumerable.ToList();
    var trueList = list.Where(condition);
    var falseList = list.Where(content => !condition(content));
    return (trueList, falseList);
  }

  /// <summary>
  /// 並び替え
  /// </summary>
  public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> list, Func<TSource, TKey> keySelector, bool isDescending) {
    return isDescending ? list.OrderByDescending(keySelector) : list.OrderBy(keySelector);
  }

  /// <summary>
  /// nullか空ならtrue
  /// </summary>
  public static bool IsNullOrEmpty<T>(this IReadOnlyCollection<T> list) {
    return list == null || list.Count == 0;
  }

  /// <summary>
  /// キーが登録されていなければキーと値をセット
  /// キーが登録されていれば値を加算
  /// </summary>
  public static void AddOrAddValue<TKey>(this Dictionary<TKey, int> dictionary, TKey key, int value) {
    if (!dictionary.ContainsKey(key)) {
      dictionary.Add(key, value);
    } else {
      dictionary[key] += value;
    }
  }

  /// <summary>
  /// キーが登録されていなければキーと値をセット
  /// キーが登録されていれば値をセット
  /// </summary>
  public static void AddOrSetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) {
    if (!dictionary.ContainsKey(key)) {
      dictionary.Add(key, value);
    } else {
      dictionary[key] = value;
    }
  }

  /// <summary>
  /// Dictionaryのforeachでpairをtupleで受け取るためのもの
  /// </summary>
  public static void Deconstruct<TKey, TValue>(
    this KeyValuePair<TKey, TValue> kvp,
    out TKey key,
    out TValue value) {
    key = kvp.Key;
    value = kvp.Value;
  }
}

}