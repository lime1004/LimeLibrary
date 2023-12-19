using System.Threading;
using UnityEngine;
using LimeLibrary.Extensions;
using LimeLibrary.Utility;

#if LIME_UNITASK
using Cysharp.Threading.Tasks;
#endif

#if LIME_ADDRESSABLES
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace LimeLibrary.System {

/// <summary>
/// リソースロード静的クラス
/// </summary>
public static class ResourceLoader {
#if LIME_ADDRESSABLES
  /// <summary>
  /// リソースの同期ロード
  /// </summary>
  public static DynamicResource<T> Load<T>(string address) where T : class {
    var asyncOperationHandle = Addressables.LoadAssetAsync<T>(address);
    var resource = asyncOperationHandle.WaitForCompletion();
    if (AssertIfFailed(asyncOperationHandle, address)) return new DynamicResource<T>(null);
    return ToDynamicResource(resource);
  }

#if LIME_UNITASK
  /// <summary>
  /// リソースの非同期ロード
  /// </summary>
  public static async UniTask<DynamicResource<T>> LoadAsync<T>(string address, CancellationToken cancellationToken) where T : class {
    var asyncOperationHandle = Addressables.LoadAssetAsync<T>(address);
    var resource = await asyncOperationHandle.WithCancellation(cancellationToken);
    if (AssertIfFailed(asyncOperationHandle, address)) return new DynamicResource<T>(null);
    return ToDynamicResource(resource);
  }
#endif

  /// <summary>
  /// GameObjectを同期ロードしてInstantiate
  /// </summary>
  public static GameObject Instantiate(string address, Transform parent = null) {
    using var prefab = Load<GameObject>(address);
    return !prefab.HasResource() ? null : UnityUtility.Instantiate(prefab.Resource, parent);
  }

  /// <summary>
  /// GameObjectを同期ロードしてInstantiate
  /// 元GameObjectを破棄しない
  /// </summary>
  public static (GameObject, DynamicResource<GameObject>) InstantiateWithResource(string address, Transform parent = null) {
    var prefab = Load<GameObject>(address);
    return (!prefab.HasResource() ? null : UnityUtility.Instantiate(prefab.Resource, parent), prefab);
  }

#if LIME_UNITASK
  /// <summary>
  /// GameObjectを非同期ロードしてInstantiate
  /// </summary>
  public static async UniTask<GameObject> InstantiateAsync(string address, CancellationToken cancellationToken, Transform parent = null) {
    using var prefab = await LoadAsync<GameObject>(address, cancellationToken);
    return !prefab.HasResource() ? null : UnityUtility.Instantiate(prefab.Resource, parent);
  }

  /// <summary>
  /// GameObjectを非同期ロードしてInstantiate
  /// 元GameObjectを破棄しない
  /// </summary>
  public static async UniTask<(GameObject, DynamicResource<GameObject>)> InstantiateWithResourceAsync(
    string address,
    CancellationToken cancellationToken,
    Transform parent = null,
    Vector3? position = null,
    Quaternion? rotation = null) {
    var prefab = await LoadAsync<GameObject>(address, cancellationToken);

    var pos = position ?? Vector3.zero;
    var rot = rotation ?? Quaternion.identity;
    return (!prefab.HasResource() ? null : UnityUtility.Instantiate(prefab.Resource, pos, rot, parent), prefab);
  }
#endif

  /// <summary>
  /// リソースのラベルによる同期ロード
  /// </summary>
  public static DynamicResource<IList<T>> LoadWithLabel<T>(string label) where T : class {
    var asyncOperationHandle = Addressables.LoadAssetsAsync<T>(label, null);
    var resourceList = asyncOperationHandle.WaitForCompletion();
    if (AssertIfFailed(asyncOperationHandle, label)) return new DynamicResource<IList<T>>(null);
    return ToDynamicResource(resourceList);
  }

#if LIME_UNITASK
  /// <summary>
  /// リソースのラベルによる非同期ロード
  /// </summary>
  public static async UniTask<DynamicResource<IList<T>>> LoadAsyncWithLabel<T>(string label, CancellationToken cancellationToken) where T : class {
    var asyncOperationHandle = Addressables.LoadAssetsAsync<T>(label, null);
    var resourceList = await asyncOperationHandle.WithCancellation(cancellationToken);
    if (AssertIfFailed(asyncOperationHandle, label)) return new DynamicResource<IList<T>>(null);
    return ToDynamicResource(resourceList);
  }
#endif

  private static DynamicResource<T> ToDynamicResource<T>(T resource) where T : class {
    var dynamicResource = new DynamicResource<T>(resource);
#if LIME_UNIRX
    dynamicResource.AddTo(UniRxExtensions.Lifespan.Application);
#endif
    return dynamicResource;
  }

  private static bool AssertIfFailed(AsyncOperationHandle asyncOperationHandle, string str) {
    if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded) {
      Assertion.Assert(false, "リソースが見つかりません. " + str);
      return true;
    }
    return false;
  }

  /// <summary>
  /// リソースの存在チェック
  /// </summary>
  public static bool Exists(string address) {
    var asyncOperationHandle = Addressables.LoadResourceLocationsAsync(address);
    var list = asyncOperationHandle.WaitForCompletion();
    return list.Any();
  }

#else
  // Resourcesフォルダ利用想定

  /// <summary>
  /// リソースの同期ロード
  /// </summary>
  public static DynamicResource<T> Load<T>(string path) where T : class {
    var resource = Resources.Load<Object>(path);
    return ToDynamicResource(resource as T);
  }

#if LIME_UNITASK
  /// <summary>
  /// リソースの非同期ロード
  /// </summary>
  public static async UniTask<DynamicResource<T>> LoadAsync<T>(string path, CancellationToken cancellationToken) where T : class {
    var resource = await Resources.LoadAsync<Object>(path).WithCancellation(cancellationToken);
    return ToDynamicResource(resource as T);
  }
#endif

  /// <summary>
  /// GameObjectを同期ロードしてInstantiate
  /// </summary>
  public static GameObject Instantiate(string path, Transform parent = null) {
    using var prefab = Load<GameObject>(path);
    return !prefab.HasResource() ? null : UnityUtility.Instantiate(prefab.Resource, parent);
  }

  /// <summary>
  /// GameObjectを同期ロードしてInstantiate
  /// 元GameObjectを破棄しない
  /// </summary>
  public static (GameObject, DynamicResource<GameObject>) InstantiateWithResource(string path, Transform parent = null) {
    var prefab = Load<GameObject>(path);
    return (!prefab.HasResource() ? null : UnityUtility.Instantiate(prefab.Resource, parent), prefab);
  }

#if LIME_UNITASK
  /// <summary>
  /// GameObjectを非同期ロードしてInstantiate
  /// </summary>
  public static async UniTask<GameObject> InstantiateAsync(string path, CancellationToken cancellationToken, Transform parent = null) {
    using var prefab = await LoadAsync<GameObject>(path, cancellationToken);
    return !prefab.HasResource() ? null : UnityUtility.Instantiate(prefab.Resource, parent);
  }

  /// <summary>
  /// GameObjectを非同期ロードしてInstantiate
  /// 元GameObjectを破棄しない
  /// </summary>
  public static async UniTask<(GameObject, DynamicResource<GameObject>)> InstantiateWithResourceAsync(
    string path,
    CancellationToken cancellationToken,
    Transform parent = null,
    Vector3? position = null,
    Quaternion? rotation = null) {
    var prefab = await LoadAsync<GameObject>(path, cancellationToken);
    var pos = position ?? Vector3.zero;
    var rot = rotation ?? Quaternion.identity;
    return (!prefab.HasResource() ? null : UnityUtility.Instantiate(prefab.Resource, pos, rot, parent), prefab);
  }
#endif

  private static DynamicResource<T> ToDynamicResource<T>(T resource) where T : class {
    var dynamicResource = new DynamicResource<T>(resource);
#if LIME_UNIRX
    dynamicResource.AddTo(UniRxExtensions.Lifespan.Application);
#endif
    return dynamicResource;
  }

  /// <summary>
  /// リソースの存在チェック
  /// </summary>
  public static bool Exists(string path) {
    var resource = Resources.Load(path);
    bool exists = resource != null;
    Resources.UnloadAsset(resource);
    return exists;
  }
#endif
}

}