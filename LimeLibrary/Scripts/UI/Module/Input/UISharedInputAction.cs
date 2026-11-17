using System;
using System.Collections.Generic;
using LimeLibrary.Input;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Pool;

namespace LimeLibrary.UI.Module.Input {

internal sealed class UISharedInputAction {
  private readonly UIInputBindingSet _bindingSet;
  private readonly InputAction _inputAction;
  private readonly List<UIInputReceiver> _listenerList = new();
  private readonly List<UIInputReceiver> _pressOwnerList = new();
  private readonly float _interactionPressPoint;

  private bool _isPressing;
  private bool _isWaitingRelease;
  private bool _isHeldWhileWaitingRelease;

  public InputAction InputAction => _inputAction;
  // NOTE: Value型は指の位置のように休まない入力があり、押下のまとまりも離し待ちも終わらなくなるためButton型だけで行う
  private bool IsButton => _bindingSet.ActionType == InputActionType.Button;
  public bool IsWaitingRelease => _isWaitingRelease;

  public UISharedInputAction(UIInputBindingSet bindingSet) {
    _bindingSet = bindingSet;
    _interactionPressPoint = FindInteractionPressPoint(bindingSet);
    _inputAction = new InputAction("UIShared", bindingSet.ActionType, interactions: bindingSet.Interactions);
    ApplyBindings();

    _inputAction.started += OnStarted;
    _inputAction.performed += OnPerformed;
    _inputAction.canceled += OnCanceled;
    // NOTE: 押されたまま有効にしたButton型も押下中として扱わせ、離したときのCanceledで離し待ちを解けるようにする
    _inputAction.wantsInitialStateCheck = IsButton;

    // NOTE: 入力を切ると押しっぱなしの押下を新しい押下と取り違えるため、作ったら破棄まで有効のままにする
    _inputAction.Enable();
    WaitReleaseIfHeld();
  }

  public void AddListener(UIInputReceiver receiver) {
    if (!_listenerList.Contains(receiver)) _listenerList.Add(receiver);
  }

  public bool RemoveListenerAndPress(UIInputReceiver receiver) {
    _listenerList.Remove(receiver);
    return _pressOwnerList.Remove(receiver);
  }

  public bool IsRebuildTarget(object actionOrMapOrAsset) => _bindingSet.ContainsSource(actionOrMapOrAsset);

  public void RebuildBindings() {
    // NOTE: 割り当ての変更は押下中の状態を捨てるため、自分で区切ってから離し待ちに入る
    _inputAction.Disable();
    while (_inputAction.bindings.Count > 0) {
      _inputAction.ChangeBinding(0).Erase();
    }
    ApplyBindings();
    _inputAction.Enable();
    WaitReleaseIfHeld();
  }

  public void UpdateReleaseWait() {
    if (!_isWaitingRelease) return;
    // NOTE: 初めの状態の確認より先に離すとCanceledが来ないため、離したかを直接読む
    if (IsAnyBindingHeld()) return;
    EndReleaseWait();
  }

  public void Dispose() {
    _inputAction.started -= OnStarted;
    _inputAction.performed -= OnPerformed;
    _inputAction.canceled -= OnCanceled;
    _inputAction.Dispose();
    _listenerList.Clear();
    _pressOwnerList.Clear();
  }

  public void CaptureListenerEnabled(uint updateCount) {
    foreach (var receiver in _listenerList) {
      receiver.CaptureEnabled(updateCount);
    }
  }

  private void OnStarted(InputAction.CallbackContext context) {
    // NOTE: Button型は離しきらずに離しのしきい値を下回るとPerformedからStartedへ戻るため、これも押下の終わりとして扱う
    if (_isWaitingRelease) {
      if (_isHeldWhileWaitingRelease) EndReleaseWait();
      return;
    }
    if (!_isPressing) return;
    UISharedInputActionRegistry.CaptureEnabledIfNewUpdate();
    EndPress(context);
  }

  private void OnPerformed(InputAction.CallbackContext context) {
    if (_isWaitingRelease) {
      _isHeldWhileWaitingRelease = true;
      return;
    }
    // NOTE: 受け手のいない共有アクションは写しが残り続けるため、ここで止めて控えの見回りを起こさない
    if (!_isPressing && _listenerList.Count == 0) return;

    // 受け手へ知らせる前に、この入力更新で有効だった受け手を控えさせる
    UISharedInputActionRegistry.CaptureEnabledIfNewUpdate();
    uint updateCount = InputState.updateCount;
    bool isFirstNotify = !_isPressing;
    if (isFirstNotify) {
      BeginPress();
    }

    // 押下の持ち主のうち、届ける瞬間にも有効なものだけへ届ける
    // NOTE: 受け手の処理から写し直しが起きると同じアクションの通知が入れ子で来るため、届け先の写しは呼び出しごとに借りる
    using (ListPool<UIInputReceiver>.Get(out var deliverList)) {
      deliverList.AddRange(_pressOwnerList);
      foreach (var receiver in deliverList) {
        if (receiver.CanReceivePress(updateCount)) {
          receiver.NotifyPress(context);
          continue;
        }
        // NOTE: 最初の押下を受けなかった受け手には離しも届けない。押していないのに離しだけで動くのを防ぐ
        if (isFirstNotify) _pressOwnerList.Remove(receiver);
      }
    }

    // Button型以外は1回の通知を1回の押下として扱い、毎回持ち主を選び直す
    if (!IsButton) EndPressWithoutNotify();
  }

  private void OnCanceled(InputAction.CallbackContext context) {
    if (_isWaitingRelease) {
      EndReleaseWait();
      return;
    }
    if (!_isPressing) return;
    UISharedInputActionRegistry.CaptureEnabledIfNewUpdate();
    EndPress(context);
  }

  private void BeginPress() {
    _isPressing = true;
    _pressOwnerList.Clear();
    // NOTE: 押下の処理中に開いた画面へ同じ押下を届けないよう、この入力更新で受け手へ知らせる前から有効だった受け手だけを持ち主にする
    uint updateCount = InputState.updateCount;
    foreach (var receiver in _listenerList) {
      if (receiver.CanOwnPress(updateCount)) _pressOwnerList.Add(receiver);
    }
  }

  private void EndPress(InputAction.CallbackContext context) {
    using (ListPool<UIInputReceiver>.Get(out var deliverList)) {
      deliverList.AddRange(_pressOwnerList);
      EndPressWithoutNotify();

      // 離したことは持ち主に有効かどうかを問わず伝え、押下中の状態を片づけさせる
      foreach (var receiver in deliverList) {
        receiver.NotifyRelease(context);
      }
    }
  }

  private void EndPressWithoutNotify() {
    _isPressing = false;
    _pressOwnerList.Clear();
  }

  private void WaitReleaseIfHeld() {
    // NOTE: 押されたまま作られたら、作る前からの押下を新しい押下と取り違えないよう離すまで捨てる
    _isWaitingRelease = IsButton && IsAnyBindingHeld();
    _isHeldWhileWaitingRelease = false;
  }

  private void EndReleaseWait() {
    _isWaitingRelease = false;
    _isHeldWhileWaitingRelease = false;
  }

  private bool IsAnyBindingHeld() {
    var bindings = _inputAction.bindings;
    for (int i = 0; i < bindings.Count; i++) {
      var binding = bindings[i];
      if (binding.isComposite) {
        int partEnd = i + 1;
        while (partEnd < bindings.Count && bindings[partEnd].isPartOfComposite) partEnd++;
        if (IsCompositeHeld(binding, i + 1, partEnd)) return true;
        i = partEnd - 1;
      } else if (!binding.isPartOfComposite && IsBindingHeld(binding)) {
        return true;
      }
    }
    return false;
  }

  private bool IsCompositeHeld(InputBinding composite, int partStart, int partEnd) {
    var bindings = _inputAction.bindings;
    if (!IsModifierComposite(composite)) {
      for (int i = partStart; i < partEnd; i++) {
        if (IsBindingHeld(bindings[i])) return true;
      }
      return false;
    }

    // NOTE: 修飾キーの組み合わせは全部のパートがそろって初めて押下になるため、修飾キーだけを押していても押されているとみなさない
    for (int i = partStart; i < partEnd; i++) {
      if (!IsPartHeld(bindings[i].name, partStart, partEnd)) return false;
    }
    return partStart < partEnd;
  }

  private bool IsPartHeld(string partName, int partStart, int partEnd) {
    // 同じ名前のパートは、どれか1つが押されていればそのパートが押されているとみなす
    var bindings = _inputAction.bindings;
    for (int i = partStart; i < partEnd; i++) {
      if (!string.Equals(bindings[i].name, partName, StringComparison.OrdinalIgnoreCase)) continue;
      if (IsBindingHeld(bindings[i])) return true;
    }
    return false;
  }

  private bool IsBindingHeld(InputBinding binding) {
    string path = binding.effectivePath;
    if (string.IsNullOrEmpty(path)) return false;
    // NOTE: 1つのコントロールが複数の割り当てに使われることがあるため、コントロールから割り当てを引かずにパスで照らし合わせる
    foreach (var control in _inputAction.controls) {
      if (!InputControlPath.Matches(path, control)) continue;
      if (!IsReleased(control)) return true;
    }
    return false;
  }

  private static bool IsModifierComposite(InputBinding composite) {
    if (string.IsNullOrEmpty(composite.path)) return false;
    string compositeName = NameAndParameters.Parse(composite.path).name;
    if (string.IsNullOrEmpty(compositeName)) return false;
    var compositeType = InputSystem.TryGetBindingComposite(compositeName);
    if (compositeType == null) return false;

    return typeof(OneModifierComposite).IsAssignableFrom(compositeType)
      || typeof(TwoModifiersComposite).IsAssignableFrom(compositeType)
      || typeof(ButtonWithOneModifier).IsAssignableFrom(compositeType)
      || typeof(ButtonWithTwoModifiers).IsAssignableFrom(compositeType);
  }

  private bool IsReleased(InputControl control) {
    float pressPoint = control is ButtonControl buttonControl ? buttonControl.pressPointOrDefault : InputSystem.settings.defaultButtonPressPoint;
    // NOTE: interactionが低い押下点を持つと、コントロールの押下点より手前でも押下になるため低い方で測る
    if (_interactionPressPoint < pressPoint) pressPoint = _interactionPressPoint;
    float releasePoint = pressPoint * InputSystem.settings.buttonReleaseThreshold;
    return control.magnitude <= releasePoint;
  }

  private static float FindInteractionPressPoint(UIInputBindingSet bindingSet) {
    float pressPoint = float.MaxValue;
    UpdateInteractionPressPoint(bindingSet.Interactions, ref pressPoint);
    foreach (var registration in bindingSet.Registrations) {
      UpdateInteractionPressPoint(registration.Interactions, ref pressPoint);
    }
    return pressPoint;
  }

  private static void UpdateInteractionPressPoint(string interactions, ref float pressPoint) {
    if (string.IsNullOrEmpty(interactions)) return;
    foreach (var interaction in NameAndParameters.ParseMultiple(interactions)) {
      foreach (var parameter in interaction.parameters) {
        if (parameter.name != "pressPoint" && parameter.name != "_pressPoint") continue;
        float value = parameter.value.ToSingle();
        if (value > 0f && value < pressPoint) pressPoint = value;
      }
    }
  }

  private void ApplyBindings() {
    foreach (var registration in _bindingSet.Registrations) {
      if (registration.SourceInputAction != null) {
        // 割り当ての変更に追随するため、写すたびに登録済みアセットから元のアクションを引き直す
        var sourceInputAction = InputActionAssetRegistry.Resolve(registration.SourceInputAction);
        CopyBindings(sourceInputAction, registration.Interactions);
        continue;
      }

      foreach (string path in registration.BindingPaths) {
        _inputAction.AddBinding(path, registration.Interactions);
      }
    }
  }

  private void CopyBindings(InputAction sourceInputAction, string interactions) {
    var bindings = sourceInputAction.bindings;

    for (int i = 0; i < bindings.Count; i++) {
      var binding = bindings[i];

      if (binding.isComposite) {
        // NOTE: コンポジット親はoverridePathが空文字なら無効化なので子パートごと飛ばし、通常時は合成名であるpathをそのまま使う
        if (binding.overridePath != null && binding.overridePath.Length == 0) {
          while (i + 1 < bindings.Count && bindings[i + 1].isPartOfComposite) i++;
          continue;
        }

        var compositeBuilder = _inputAction.AddCompositeBinding(binding.path, interactions);
        while (i + 1 < bindings.Count && bindings[i + 1].isPartOfComposite) {
          i++;
          compositeBuilder.With(bindings[i].name, bindings[i].effectivePath);
        }
      } else if (!binding.isPartOfComposite) {
        _inputAction.AddBinding(binding.effectivePath, interactions: interactions);
      }
    }
  }
}

}
