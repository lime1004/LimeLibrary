using System.Collections.Generic;
using UnityEngine;
#if LIME_ADDRESSABLES
using System.Reflection;
using UnityEngine.AddressableAssets;
#endif

namespace LimeLibrary.Resource {

/// <summary>
/// Disposeによってリソースの解放を行うクラス
/// </summary>
public class DynamicResource<T> : IDynamicResource where T : class {
  public T Resource { get; private set; }

  public DynamicResource(T resource) {
    Resource = resource;
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
    // UnityEditor上ではEditor終了時にAddressablesが初期化されてしまうため初期化されないルートでリリースする
    ReleaseReflection();
#else
    Addressables.Release(Resource);
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

#if UNITY_EDITOR && LIME_ADDRESSABLES
  public void ReleaseReflection() {
    if (Resource == null) return;

    var type = typeof(Addressables);
    var addressableImplInfo = type.GetField("m_AddressablesInstance", BindingFlags.Static | BindingFlags.NonPublic);
    object addressableImpl = addressableImplInfo.GetValue(null);

    var addressableImplType = addressableImpl.GetType();
    var methods = addressableImplType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
    foreach (var method in methods) {
      if (method.Name == "Release" && method.IsGenericMethod) {
        if (method.GetParameters()[0].ParameterType == method.GetGenericArguments()[0]) {
          var genericReleaseMethodInfo = method.MakeGenericMethod(typeof(T));
          genericReleaseMethodInfo.Invoke(addressableImpl, new object[] { Resource });
          return;
        }
      }
    }
  }
#endif
}

}