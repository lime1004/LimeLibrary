using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Event.Core;
using LimeLibrary.Extensions;
using LimeLibrary.UI.App;
using LimeLibrary.Utility;
using UniRx;
using UnityEngine;

namespace LimeLibrary.UI {

public class AwakeUIAppEvent : AbstractEvent {
  private UIAppCreateType _createType;
  private string _address;
  private GameObject _prefab;
  private Func<CancellationToken, UniTask<UIApp>> _createFunc;
  private UIAppAwakeType _awakeType;
  private UniTask<UIApp> _createUIAppTask;
  private UniTask _finishUIAppTask;
  private UIApp _createdUIApp;

  private enum UIAppCreateType {
    Address,
    Prefab,
    Func,
  }

  private readonly Subject<UIApp> _onCreateUIAppSubject = new();
  public IObservable<UIApp> OnCreateUIAppObservable => _onCreateUIAppSubject;

  public void Setup(string address, UIAppAwakeType awakeType) {
    _createType = UIAppCreateType.Address;
    _address = address;
    _awakeType = awakeType;
  }

  public void Setup(GameObject prefab, UIAppAwakeType awakeType) {
    _createType = UIAppCreateType.Prefab;
    _prefab = prefab;
    _awakeType = awakeType;
  }

  public void Setup(Func<CancellationToken, UniTask<UIApp>> createFunc) {
    _createType = UIAppCreateType.Func;
    _createFunc = createFunc;
  }

  public override void Start() {
    switch (_createType) {
    case UIAppCreateType.Address:
      _createUIAppTask = UIManager.Instance.UIAppManager.CreateAppAsync(_address, _awakeType, CancellationToken, true, false).RunHandlingError();
      break;
    case UIAppCreateType.Prefab:
      _createUIAppTask = UIManager.Instance.UIAppManager.CreateAppAsync(_prefab, _awakeType, CancellationToken, true, false).RunHandlingError();
      break;
    case UIAppCreateType.Func:
      _createUIAppTask = _createFunc.Invoke(CancellationToken).RunHandlingError();
      break;
    default:
      Assertion.Assert(false);
      break;
    }
  }

  private enum Seq {
    WaitCreateApp,
    WaitFinishApp,
    Finish,
  }

  public override EventUpdateResult Update() {
    switch (GetSeq<Seq>()) {
    case Seq.WaitCreateApp:
      if (_createUIAppTask.GetAwaiter().IsCompleted) {
        _createdUIApp = _createUIAppTask.GetAwaiter().GetResult();
        _onCreateUIAppSubject.OnNext(_createdUIApp);
        _createdUIApp.Show(CancellationToken).RunHandlingError().Forget();
        _finishUIAppTask = _createdUIApp.EventObservables.GetObservable(UIAppEventType.Destroy).ToUniTask(true, CancellationToken);
        AddSeq();
      }
      break;

    case Seq.WaitFinishApp:
      if (_finishUIAppTask.GetAwaiter().IsCompleted) {
        AddSeq();
        return EventUpdateResult.ContinueDirect;
      }
      break;

    case Seq.Finish:
      return EventUpdateResult.Finish;

    default:
      Assertion.Assert(false);
      break;
    }

    return EventUpdateResult.Continue;
  }
}

}