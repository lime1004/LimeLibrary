#if LIME_R3
#endif

#if LIME_UNITASK
#endif

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Attributes;
using LimeLibrary.Extensions;
using LimeLibrary.Module;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

namespace LimeLibrary.Input.InputMode {

#if LIME_INPUTSYSTEM && LIME_UNITASK
/// <summary>
/// 入力デバイスの種類を更新するクラス
/// </summary>
public class InputModeUpdater : SingletonMonoBehaviour<InputModeUpdater> {
  // NOTE: 8bitスティックの静止時の揺れ（±1/127程度）を弾き、デッドゾーン既定値0.125も上回る生値の閾値
  private const float GamepadActuationThreshold = 0.2f;

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
    if (!IsInputModeSwitchCandidate(inputControl, inputEventPtr)) return;
    _onUseDeviceChannel.Writer.TryWrite(inputControl);
  }

  private static bool IsInputModeSwitchCandidate(InputControl inputControl, InputEventPtr inputEventPtr) {
    // NOTE: 通知は葉コントロール単位で来るため、マウスは移動・スクロールだけで毎フレーム大量に流れる。消費側が見る3ボタンに絞る
    if (inputControl.device is Mouse mouse) {
      return inputControl == mouse.leftButton ||
             inputControl == mouse.rightButton ||
             inputControl == mouse.middleButton;
    }

    // NOTE: ゲームパッドはスティックの揺れだけでも通知が流れてくるので、実際に操作されたコントロールだけに絞る
    if (inputControl.device is Gamepad) {
      // NOTE: 値を評価できない型は操作の裏付けが取れないため通さない
      if (inputControl is not InputControl<float> valueControl) return false;

      // NOTE: このコールバックはイベントのデバイス反映前に呼ばれ、かつデッドゾーン設定に左右させたくないのでイベントの生値を読む
      float value = valueControl.ReadUnprocessedValueFromEvent(inputEventPtr);
      if (inputControl is ButtonControl buttonControl) return buttonControl.IsValueConsideredPressed(value);

      return Mathf.Abs(value) >= GamepadActuationThreshold;
    }

    return true;
  }

  private async UniTaskVoid RunUseDeviceEvent(CancellationToken cancellationToken) {
    _onUseDeviceChannel = Channel.CreateSingleConsumerUnbounded<InputControl>();
    var reader = _onUseDeviceChannel.Reader;

    while (await reader.WaitToReadAsync(cancellationToken)) {
      // NOTE: ボタンの押下判定などが正確に取れないため、PlayerLoopTiming.Updateまで待つ
      await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

      // NOTE: 1件ごとにYieldすると末尾のイベントが遅延するため、Yieldは1フレーム1回にして溜まった分をまとめて捌く
      while (reader.TryRead(out var inputControl)) {
        cancellationToken.ThrowIfCancellationRequested();
        OnUnpairedDeviceUsed(inputControl);
      }
    }
  }

  /// <summary>
  /// 外部からInputModeをインデックスで切り替える
  /// </summary>
  public void SetInputMode(int index) {
    if (!_inputModeList.IsDefinedAt(index)) return;
    if (_currentInputModeIndex == index) return;
    ChangeInputMode(index);
  }

  /// <summary>
  /// 外部からInputModeを名前で切り替える
  /// </summary>
  public void SetInputMode(string inputModeName) {
    int index = _inputModeList.FindIndex(m => m.Name == inputModeName);
    if (index >= 0) SetInputMode(index);
  }

  private void ChangeInputMode(int nextInputModeIndex) {
    if (_inputModeList.IsDefinedAt(_currentInputModeIndex)) {
      _inputModeList[_currentInputModeIndex].OnExitInputMode();
    }
    _inputModeList[nextInputModeIndex].OnEnterInputMode();
    _currentInputModeIndex = nextInputModeIndex;
#if LIME_R3
    _onChangeInputModeSubject.OnNext(_inputModeList[nextInputModeIndex]);
#endif
  }

  private void OnUnpairedDeviceUsed(InputControl inputControl) {
    var device = inputControl.device;

    foreach (var inputMode in _inputModeList) {
      int nextInputModeIndex = _inputModeList.IndexOf(inputMode);
      if (_currentInputModeIndex == nextInputModeIndex) continue;
      if (!inputMode.CheckChangeInputMode(inputControl.device)) continue;

      ChangeInputMode(nextInputModeIndex);
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