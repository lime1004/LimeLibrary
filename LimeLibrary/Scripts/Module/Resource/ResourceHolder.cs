using System.Threading;
using LimeLibrary.Resource;
using LimeLibrary.Utility;
#if LIME_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace LimeLibrary.Module {

/// <summary>
/// リソース保持クラス
/// </summary>
public class ResourceHolder<T> : DisposableHolder<DynamicResource<T>> where T : class {
#if LIME_UNITASK
  public async UniTask<T> GetOrLoadAsync(string address, CancellationToken cancellationToken) {
    if (Exists(address)) {
      return Get(address).Resource;
    } else {
      return (await LoadAsync(address, cancellationToken)).Resource;
    }
  }
#endif

  public T GetOrLoad(string address) {
    if (Exists(address)) {
      return Get(address).Resource;
    } else {
      return Load(address).Resource;
    }
  }

#if LIME_UNITASK
  public async UniTask<DynamicResource<T>> LoadAsync(string address, CancellationToken cancellationToken) {
    if (Exists(address)) {
      Assertion.Assert(false);
      return Get(address);
    }

    var resource = await ResourceLoader.LoadAsync<T>(address, cancellationToken);
    if (Exists(address)) {
      resource.Dispose();
      return Get(address);
    }

    Add(address, resource);

    return resource;
  }
#endif

  public DynamicResource<T> Load(string address) {
    if (Exists(address)) {
      Assertion.Assert(false);
      return Get(address);
    }

    var resource = ResourceLoader.Load<T>(address);
    if (Exists(address)) {
      resource.Dispose();
      return Get(address);
    }

    Add(address, resource);

    return resource;
  }

  public T GetResource(string address) {
    return Get(address)?.Resource;
  }
}

}