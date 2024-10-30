using System;
using System.Linq;

namespace LimeLibrary.Module {

public enum GetValueType {
  Default,
  Next,
  Previous
}

[Serializable]
public class LevelValueDictionary : SerializedDictionary<int, int> {
  public int GetValue(int level, GetValueType getValueType, int defaultValue = 0) {
    if (TryGetValue(level, out int value)) {
      return value;
    }

    switch (getValueType) {
    case GetValueType.Next: {
      var nextKey = Keys.Where(k => k > level).OrderBy(k => k).FirstOrDefault();
      if (nextKey != 0 || ContainsKey(nextKey)) {
        return this[nextKey];
      }
      return defaultValue;
    }
    case GetValueType.Previous: {
      var previousKey = Keys.Where(k => k < level).OrderByDescending(k => k).FirstOrDefault();
      if (previousKey != 0 || ContainsKey(previousKey)) {
        return this[previousKey];
      }
      return defaultValue;
    }
    default:
      return defaultValue;
    }
  }
}

}