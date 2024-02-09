using System;
using System.Collections.Generic;
using LimeLibrary.Utility;
using UnityEngine;

namespace LimeLibrary.Module {

/// <summary>
/// Serialize可能な多次元配列
/// </summary>
[Serializable]
public class SerializableMultidimensionalArray<T> {
  [SerializeField]
  private List<T> _list = new List<T>();
  [SerializeField]
  private int _rowCount;
  public int RowCount => _rowCount;
  [SerializeField]
  private int _columnCount;
  public int ColumnCount => _columnCount;
  [SerializeField]
  private bool _isInitialized;
  public bool IsInitialized => _isInitialized;

  public SerializableMultidimensionalArray(T[,] multidimensionalArray) {
    _rowCount = multidimensionalArray.GetLength(0);
    _columnCount = multidimensionalArray.GetLength(1);

    for (int x = 0; x < _rowCount; x++) {
      for (int y = 0; y < _columnCount; y++) {
        _list.Add(multidimensionalArray[x, y]);
      }
    }

    _isInitialized = true;
  }

  public SerializableMultidimensionalArray(List<T> list, int rowCount, int columnCount, bool isInitialized) {
    _list = list;
    _rowCount = rowCount;
    _columnCount = columnCount;
    _isInitialized = isInitialized;
  }

  public T Get(int x, int y) {
    if (!IsValidIndex(x, y)) {
      Assertion.Assert(false);
      return default;
    }

    int index = GetIndex(x, y);
    return _list[index];
  }

  public T Get(Vector2Int pos) {
    return Get(pos.x, pos.y);
  }

  public bool IsValidIndex(int x, int y) {
    if (x < 0) return false;
    if (y < 0) return false;
    if (x >= _rowCount) return false;
    if (y >= _columnCount) return false;
    return true;
  }

  public T[,] ToArray() {
    var array = new T[_rowCount, _columnCount];
    for (int x = 0; x < _columnCount; x++) {
      for (int y = 0; y < _rowCount; y++) {
        array[x, y] = Get(x, y);
      }
    }
    return array;
  }

  public List<T> ToList() {
    return _list;
  }

  public Dictionary<Vector2Int, T> ToCenterBaseDictionary() {
    var dic = new Dictionary<Vector2Int, T>();
    var basePos = new Vector2Int(_rowCount / 2, _columnCount / 2);
    for (int x = 0; x < _rowCount; x++) {
      for (int y = 0; y < _columnCount; y++) {
        var pos = new Vector2Int(x - basePos.x, -y + basePos.y);
        dic.Add(pos, Get(x, y));
      }
    }
    return dic;
  }

  private int GetIndex(int x, int y) {
    return _columnCount * x + y;
  }
}

}