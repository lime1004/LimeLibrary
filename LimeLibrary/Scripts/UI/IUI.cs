using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace LimeLibrary.UI {

public interface IUI {
  public GameObject RootObject { get; }
  public IObservable<Unit> OnShowEndObservable { get; }
  public IObservable<Unit> OnHideEndObservable { get; }
  public IUIInputObservables InputObservables { get; }

  public UniTask Show(CancellationToken cancellationToken);
  public UniTask Hide(CancellationToken cancellationToken);
}

}