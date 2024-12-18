using System;
using System.Collections.Generic;
using UnityEngine;

namespace LimeLibrary.Module {

[Serializable]
public class CountCollection<T> {
  [SerializeField]
  private List<T> _list;

  [SerializeField]
  private int _count;

  public void Initialize(List<T> list, int initialCount) {
    _list = list;
    _count = initialCount;
  }

  public List<T> GetList() {
    var list = new List<T>();
    for (int i = 0; i < _count; i++) {
      list.Add(_list[i]);
    }
    return list;
  }

  public void AddCount(int count) {
    _count += count;
  }

  public void Insert(T content) {
    if (_list == null) return;
    _list.Remove(content);
    _list.Insert(_count, content);
    _count++;
  }

  public bool IsUnlocked(T content) {
    if (_list == null) return false;
    int index = _list.IndexOf(content);
    return index >= 0 && index < _count;
  }

  public void Clear() {
    _list = null;
    _count = 0;
  }
}

}