using System;
using System.Collections.Generic;
using UnityEngine;

namespace LimeLibrary.Module {

/// <summary>
/// Serialize可能なDictionary
/// </summary>
[Serializable]
public abstract class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
  [SerializeField, HideInInspector]
  private List<TKey> _keyData = new List<TKey>();

  [SerializeField, HideInInspector]
  private List<TValue> _valueData = new List<TValue>();

  void ISerializationCallbackReceiver.OnAfterDeserialize() {
    Clear();
    for (int i = 0; i < _keyData.Count && i < _valueData.Count; i++) {
      this[_keyData[i]] = _valueData[i];
    }
  }

  void ISerializationCallbackReceiver.OnBeforeSerialize() {
    _keyData.Clear();
    _valueData.Clear();

    foreach (var (key, value) in this) {
      _keyData.Add(key);
      _valueData.Add(value);
    }
  }
}

}