using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LimeLibrary.System;
using LimeLibrary.Utility;
using UnityEngine;
#if LIME_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace LimeLibrary.Module {

#if LIME_ADDRESSABLES
/// <summary>
/// Addressablesのラベルを使ったリソースリスト保持クラス
/// </summary>
public abstract class ObjectListHolder<T> : DisposableHolder<DynamicResource<IList<T>>> where T : Object {
  protected abstract string GetAddress();

#if LIME_UNITASK
  public async UniTask LoadAllAsync(CancellationToken cancellationToken) {
    Dispose();

    var listResource = await ResourceLoader.LoadAsyncWithLabel<T>(GetAddress(), cancellationToken);
    Add(GetAddress(), listResource);
  }
#endif

  public void LoadAll() {
    Dispose();

    var listResource = ResourceLoader.LoadWithLabel<T>(GetAddress());
    Add(GetAddress(), listResource);
  }

  public bool ExistsResource(string name) {
    return Exists(GetAddress()) && Get(GetAddress()).Resource.Any(obj => IsSame(obj, name));
  }

  public T GetResource(string name) {
    if (!Exists(GetAddress())) {
      Assertion.Assert(false);
      return null;
    }
    if (!ExistsResource(name)) {
      Assertion.Assert(false, name);
      return null;
    }

    return Get(GetAddress()).Resource.FirstOrDefault(obj => IsSame(obj, name));
  }

  public bool IsLoadedList() {
    return Exists(GetAddress());
  }

  public IList<T> GetList() {
    return Get(GetAddress()).Resource;
  }

  protected virtual bool IsSame(T obj, string name) {
    return obj.name == name;
  }
}
#endif

}