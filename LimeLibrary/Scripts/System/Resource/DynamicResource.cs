using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
#if LIME_ADDRESSABLES
using UnityEngine.AddressableAssets;
#endif

namespace LimeLibrary.Resource {

/// <summary>
/// Disposeによってリソースの解放を行うクラス
/// </summary>
public class DynamicResource<T> : IDynamicResource where T : class {
  private readonly AsyncOperationHandle<T>? _handle;

  public T Resource { get; private set; }

  public DynamicResource(T resource, AsyncOperationHandle<T>? handle = null) {
    Resource = resource;
    _handle = handle;
  }

  /// <summary>
  /// リソースがあるかどうか
  /// </summary>
  public bool HasResource() {
    return Resource != null;
  }

  public void Dispose() {
    if (Resource == null) return;

#if LIME_ADDRESSABLES
#if UNITY_EDITOR
    if (_handle.HasValue && _handle.Value.IsValid()) {
      Addressables.Release(_handle.Value);
    } else if (!_handle.HasValue) {
      Addressables.Release(Resource);
    }
#endif
#else
    // Resourcesフォルダ利用想定で処理
    switch (Resource) {
    case Object unityObject:
      Resources.UnloadAsset(unityObject);
      break;
    case IList<Object> list: {
      foreach (var obj in list) {
        Resources.UnloadAsset(obj);
      }
      break;
    }
    }
#endif
    Resource = null;
  }
}

}