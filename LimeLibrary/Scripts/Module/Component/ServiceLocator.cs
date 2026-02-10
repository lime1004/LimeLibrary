using System;
using System.Collections.Generic;
using LimeLibrary.Utility;
using UnityEngine;

namespace LimeLibrary.Module {

public abstract class ServiceLocator<T> : SingletonMonoBehaviour<T> where T : MonoBehaviour {
  private readonly Dictionary<Type, object> _serviceDictionary = new();

  /// <summary>
  /// インスタンスを登録
  /// </summary>
  public void RegisterInstance<TInstance>(TInstance instance, bool isOverwrite = false) where TInstance : class {
    var type = typeof(TInstance);
    if (!isOverwrite && ExistInstance<TInstance>()) {
      Assertion.Assert(false, "その型は既に登録済みです. " + type);
      return;
    }
    if (isOverwrite && ExistInstance<TInstance>()) {
      _serviceDictionary[type] = instance;

      // 実装しているインターフェースも登録
      foreach (var i in type.GetInterfaces()) {
        _serviceDictionary[i] = instance;
      }
    } else {
      _serviceDictionary.Add(type, instance);

      // 実装しているインターフェースも登録
      foreach (var i in type.GetInterfaces()) {
        _serviceDictionary.TryAdd(i, instance);
      }
    }
  }

  /// <summary>
  /// インスタンスを削除
  /// </summary>
  public void RemoveInstance<TInstance>() {
    var type = typeof(TInstance);
    if (!ExistInstance<TInstance>()) {
      Assertion.Assert(false, "その型は登録されていません. " + type);
      return;
    }
    _serviceDictionary.Remove(type);

    // 実装しているインターフェースも削除
    foreach (var i in type.GetInterfaces()) {
      _serviceDictionary.Remove(i);
    }
  }

  /// <summary>
  /// インスタンスを削除
  /// </summary>
  public void RemoveInstance<TInstance>(TInstance instance) where TInstance : class {
    var type = typeof(TInstance);
    if (!ExistInstance<TInstance>()) {
      return;
    }

    if (_serviceDictionary[type] != instance) return;

    _serviceDictionary.Remove(type);

    // 実装しているインターフェースも削除
    foreach (var i in type.GetInterfaces()) {
      if (_serviceDictionary.TryGetValue(i, out var value) && value == instance) {
        _serviceDictionary.Remove(i);
      }
    }
  }

  /// <summary>
  /// インスタンスを取得
  /// </summary>
  public TInstance GetInstance<TInstance>() where TInstance : class {
    var type = typeof(TInstance);
    if (!ExistInstance<TInstance>()) {
      Assertion.Assert(false, "その型は登録されていません. " + type);
      return null;
    }
    return _serviceDictionary[type] as TInstance;
  }

  /// <summary>
  /// インスタンスが存在するか
  /// </summary>
  public bool ExistInstance<TInstance>() {
    var type = typeof(TInstance);
    return _serviceDictionary.ContainsKey(type) && _serviceDictionary[type] != null;
  }

  /// <summary>
  /// インスタンスを全て削除
  /// </summary>
  public void Clear() {
    _serviceDictionary.Clear();
  }
}

}