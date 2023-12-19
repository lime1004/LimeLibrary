using System;
using System.Collections.Generic;
using LimeLibrary.System;
using LimeLibrary.Utility;

namespace LimeLibrary.Module {

/// <summary>
/// リソースキャッシュクラス
/// </summary>
public class ResourceCacheHolder : IDisposable {
  private readonly Dictionary<string, IDynamicResource> _dictionary;
  private readonly int _maxResources;

  public bool AssertIfOverMaxResources { get; set; } = true;

  public ResourceCacheHolder(int maxResources) {
    _dictionary = new Dictionary<string, IDynamicResource>(maxResources);
    _maxResources = maxResources;
  }

  /// <summary>
  /// リソースを追加
  /// </summary>
  public void AddResource(string address, IDynamicResource dynamicResource) {
    if (ExistsResource(address)) {
      Assertion.Assert(false, "DynamicResourceがすでにキャッシュされています. " + address);
      return;
    }
    if (_dictionary.Count >= _maxResources) {
      if (AssertIfOverMaxResources) Assertion.Assert(false, $"Resourceの数が{_maxResources}を超えました！ " + _dictionary.Count);
    }

    _dictionary.Add(address, dynamicResource);
  }

  /// <summary>
  /// キャッシュが存在するか
  /// </summary>
  public bool ExistsResource(string address) {
    return _dictionary.ContainsKey(address);
  }

  /// <summary>
  /// キャッシュを取得
  /// </summary>
  public DynamicResource<T> GetResource<T>(string address) where T : UnityEngine.Object {
    if (!ExistsResource(address)) {
      Assertion.Assert(false, "DynamicResourceが見つかりません. " + address);
      return null;
    }
    return _dictionary[address] as DynamicResource<T>;
  }

  /// <summary>
  /// Dispose
  /// </summary>
  public void Dispose() {
    foreach (var pair in _dictionary) {
      pair.Value.Dispose();
    }
  }
}

}