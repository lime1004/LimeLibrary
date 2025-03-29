#if LIME_R3
#endif

#if LIME_UNITASK
#endif

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Attribute;
using LimeLibrary.Extensions;
using LimeLibrary.Module;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

namespace LimeLibrary.Input.InputMode {

#if LIME_INPUTSYSTEM && LIME_UNITASK
/// <summary>
/// 入力デバイスの種類を更新するクラス
/// </summary>
public class InputModeUpdater : SingletonMonoBehaviour<InputModeUpdater> {
  [SerializeField]
  private List<InputModeScriptableObject> _inputModeList;
  [SerializeField, ReadOnly]
  private int _currentInputModeIndex = -1;

  private Channel<InputControl> _onUseDeviceChannel;

  public IInputMode CurrentInputMode => _inputModeList.IsDefinedAt(_currentInputModeIndex) ? _inputModeList[_currentInputModeIndex] : null;
  public string CurrentInputModeName => CurrentInputMode?.Name ?? string.Empty;
  public IReadOnlyList<IInputMode> InputModeList => _inputModeList;

#if LIME_R3
  private readonly Subject<IInputMode> _onChangeInputModeSubject = new();
  private readonly Subject<InputDevice> _onUseDeviceSubject = new();
  public Observable<IInputMode> OnChangeInputModeObservable => _onChangeInputModeSubject;
  public Observable<InputDevice> OnUseDeviceObservable => _onUseDeviceSubject;
#endif

  protected override void Awake() {
    InputUser.CreateUserWithoutPairedDevices();
    ++InputUser.listenForUnpairedDeviceActivity;
    InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsedCallback;

    RunUseDeviceEvent(gameObject.GetCancellationTokenOnDestroy()).Forget();
  }

  private void OnDestroy() {
    --InputUser.listenForUnpairedDeviceActivity;
    InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsedCallback;
  }

  private void OnUnpairedDeviceUsedCallback(InputControl inputControl, InputEventPtr inputEventPtr) {
    _onUseDeviceChannel.Writer.TryWrite(inputControl);
  }

  private async UniTaskVoid RunUseDeviceEvent(CancellationToken cancellationToken) {
    _onUseDeviceChannel = Channel.CreateSingleConsumerUnbounded<InputControl>();

    await foreach (var inputControl in _onUseDeviceChannel.Reader.ReadAllAsync(cancellationToken)) {
      // NOTE: ボタンの押下判定などが正確に取れないため、PlayerLoopTiming.Updateまで待つ
      await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
      OnUnpairedDeviceUsed(inputControl);
    }
  }

  private void OnUnpairedDeviceUsed(InputControl inputControl) {
    var device = inputControl.device;

    foreach (var inputMode in _inputModeList) {
      int nextInputModeIndex = _inputModeList.IndexOf(inputMode);
      if (_currentInputModeIndex == nextInputModeIndex) continue;
      if (!inputMode.CheckChangeInputMode(inputControl.device)) continue;

      if (_inputModeList.IsDefinedAt(_currentInputModeIndex)) {
        _inputModeList[_currentInputModeIndex].OnExitInputMode();
      }
      inputMode.OnEnterInputMode();
      _currentInputModeIndex = nextInputModeIndex;
#if LIME_R3
      _onChangeInputModeSubject.OnNext(inputMode);
#endif
      break;
    }

#if LIME_R3
    _onUseDeviceSubject.OnNext(device);
#endif
  }

  private void Update() {
    if (_inputModeList.IsDefinedAt(_currentInputModeIndex)) {
      _inputModeList[_currentInputModeIndex].OnUpdate();
    }
  }

  private void LateUpdate() {
    if (_inputModeList.IsDefinedAt(_currentInputModeIndex)) {
      _inputModeList[_currentInputModeIndex].OnLateUpdate();
    }
  }
}
#endif

}