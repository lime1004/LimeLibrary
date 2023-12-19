using System;
using LimeLibrary.Utility;
using UnityEngine;

namespace LimeLibrary.Module {

/// <summary>
/// Dictionary形式のScriptableObjectを作成するための基底クラス
/// </summary>
public abstract class DictionaryScriptableObject<T1, T2> : ScriptableObject {
  [Serializable]
  protected class Dictionary : SerializedDictionary<T1, T2> { }

  [SerializeField]
  protected Dictionary _dictionary;

  public T2 Get(T1 key) {
    if (!Exists(key)) {
      Assertion.Assert(false, $"Not found key. {key}");
      return default;
    }
    return _dictionary[key];
  }

  public bool Exists(T1 key) {
    return _dictionary.ContainsKey(key);
  }
}

}