using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace LimeLibrary.UI {

public interface IUI {
  public GameObject RootObject { get; }
  public Observable<Unit> OnShowEndObservable { get; }
  public Observable<Unit> OnHideEndObservable { get; }
  public IUIInputObservables InputObservables { get; }

  public UniTask Show(CancellationToken cancellationToken);
  public UniTask Hide(CancellationToken cancellationToken);
}

}