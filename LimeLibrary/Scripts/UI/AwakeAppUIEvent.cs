using System;
using Cysharp.Threading.Tasks;
using LimeLibrary.Event;
using LimeLibrary.Event.Core;
using LimeLibrary.Extensions;
using LimeLibrary.UI.App;
using LimeLibrary.Utility;
using UniRx;

namespace LimeLibrary.UI {

public class AwakeAppUIEvent : AbstractEvent {
  private string _address;
  private UIAppAwakeType _awakeType;
  private UniTask<UIApp> _createUIAppTask;
  private UniTask _finishUIAppTask;
  private UIApp _createdUIApp;

  private readonly Subject<UIApp> _onCreateUIAppSubject = new();
  public IObservable<UIApp> OnCreateUIAppObservable => _onCreateUIAppSubject;

  public void Setup(string address, UIAppAwakeType awakeType) {
    _address = address;
    _awakeType = awakeType;
  }

  public override void Start() {
    // UI生成
    _createUIAppTask = UIManager.Instance.UIAppManager.CreateAppAsync(_address, _awakeType, CancellationToken, true, false).RunHandlingError();
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