using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using UnityEngine;

namespace LimeLibrary.UI.Animation {

[Serializable]
public class CustomFuncPlayer : IUIAnimationPlayer {
  [SerializeField]
  private string _id;
  public string Id => _id;

  private Func<bool, CancellationToken, UniTask> _func;
  private Action _pauseFunc;
  private CancellationTokenSource _cancellationTokenSource;
  private UniTask _animationTask;

  public CustomFuncPlayer(Func<bool, CancellationToken, UniTask> func) {
    _func = func;
  }

  public void Play() {
    Stop();
    _cancellationTokenSource = new CancellationTokenSource();
    _animationTask = _func(false, _cancellationTokenSource.Token).RunHandlingError();
  }

  public void PlayImmediate() {
    Stop();
    _cancellationTokenSource = new CancellationTokenSource();
    _animationTask = _func(true, _cancellationTokenSource.Token).RunHandlingError();
  }

  public void SetPauseFunc(Action pauseFunc) {
    _pauseFunc = pauseFunc;
  }

  public void Pause() {
    _pauseFunc?.Invoke();
  }

  public void Stop() {
    _cancellationTokenSource?.Cancel();
  }

  public UniTask WaitPlaying(CancellationToken cancellationToken) {
    return _animationTask;
  }
}

}