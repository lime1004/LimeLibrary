using System;
using System.Collections.Generic;
using LimeLibrary.Utility;

namespace LimeLibrary.Module {

/// <summary>
/// IDisposableなクラスを保持するクラス 
/// </summary>
public class DisposableHolder<T> : IDisposable where T : class, IDisposable {
  private readonly Dictionary<string, T> _disposableDictionary = new(256);

  public void Add(string address, T disposable) {
    if (Exists(address)) {
      Assertion.Assert(false, address);
      return;
    }
    lock (_disposableDictionary) {
      _disposableDictionary.Add(address, disposable);
    }
  }

  public T Get(string address) {
    if (!Exists(address)) {
      Assertion.Assert(false, address);
      return null;
    }
    return _disposableDictionary[address];
  }

  public bool Exists(string address) {
    return _disposableDictionary.ContainsKey(address);
  }

  public virtual void Dispose() {
    foreach (var (_, disposable) in _disposableDictionary) {
      disposable.Dispose();
    }
    _disposableDictionary.Clear();
  }
}

}