using System;
using System.Linq;

namespace LimeLibrary.Module {

public enum GetValueType {
  Default,
  Next,
  Previous
}

[Serializable]
public class IntKeyDictionary<T> : SerializedDictionary<int, T> {
  public T GetValue(int key, GetValueType getValueType) {
    if (TryGetValue(key, out var value)) {
      return value;
    }

    switch (getValueType) {
    case GetValueType.Next: {
      int nextKey = Keys.Where(k => k > key).OrderBy(k => k).FirstOrDefault();
      if (nextKey != 0 || ContainsKey(nextKey)) {
        return this[nextKey];
      }
      return default;
    }
    case GetValueType.Previous: {
      int previousKey = Keys.Where(k => k < key).OrderByDescending(k => k).FirstOrDefault();
      if (previousKey != 0 || ContainsKey(previousKey)) {
        return this[previousKey];
      }
      return default;
    }
    default:
      return default;
    }
  }
}

}