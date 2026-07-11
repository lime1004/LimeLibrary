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
    if (!IsInputModeSwitchCandidate(inputControl)) return;
    _onUseDeviceChannel.Writer.TryWrite(inputControl);
  }

  // NOTE: onUnpairedDeviceUsedは親のVector2Control等ではなく葉コントロール単位で通知される
  // （マウスのposition/x等やスティックのleftStick/x等はAxisControl、D-padの各方向はButtonControl）。
  // マウスは移動・スクロール由来の通知だけでも毎フレーム大量に来てキューを埋め、
  // ゲームパッド切替の反映を遅らせるので、マウスについては消費側DefaultMouseKeyboard.CheckChangeInputMode
  // が実際に見ているボタン（左/右/中）だけをキューに積む。他デバイスはそのまま通す
  private static bool IsInputModeSwitchCandidate(InputControl inputControl) {
    if (inputControl.device is not Mouse mouse) return true;

    return inputControl == mouse.leftButton ||
           inputControl == mouse.rightButton ||
           inputControl == mouse.middleButton;
  }

  private async UniTaskVoid RunUseDeviceEvent(CancellationToken cancellationToken) {
    _onUseDeviceChannel = Channel.CreateSingleConsumerUnbounded<InputControl>();
    var reader = _onUseDeviceChannel.Reader;

    while (await reader.WaitToReadAsync(cancellationToken)) {
      // NOTE: ボタンの押下判定などが正確に取れないため、PlayerLoopTiming.Updateまで待つ
      await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

      // NOTE: 1件読むごとにYieldするとキューが伸びたときに末尾のイベントが数フレーム〜数秒単位で
      // 遅延する（マウス移動のようにキュー投入頻度が高いデバイスが混ざると顕著）。
      // 1フレームにつきYieldは1回だけにし、そのフレームで溜まっている分をTryReadでまとめて捌く
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