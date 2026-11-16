using System.Collections.Generic;
using LimeLibrary.Input.Interaction;
using NUnit.Framework;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace LimeLibrary.Tests.Input {

public class InputSystemBehaviourTests : InputTestFixture {
  private Gamepad _gamepad;
  private readonly List<string> _log = new();

  public override void Setup() {
    base.Setup();
    // NOTE: InputTestFixtureが入力の状態を作り直してもRepeatを使えるよう、念のため登録し直す
    InputSystem.RegisterInteraction<RepeatInteraction>();
    _gamepad = InputSystem.AddDevice<Gamepad>();
    _log.Clear();
  }

  private InputAction CreateAction(InputActionType type, string binding, string interactions = null) {
    var action = new InputAction("Test", type, binding, interactions);
    action.started += _ => _log.Add("started");
    action.performed += _ => _log.Add("performed");
    action.canceled += _ => _log.Add("canceled");
    return action;
  }

  [Test]
  public void Button型は押し込みで1回だけperformedし離しきるとcanceled() {
    var action = CreateAction(InputActionType.Button, "<Gamepad>/leftTrigger");
    action.Enable();

    Set(_gamepad.leftTrigger, 0.9f);
    Set(_gamepad.leftTrigger, 0.95f);
    Set(_gamepad.leftTrigger, 0.0f);

    Assert.That(_log, Is.EqualTo(new[] { "started", "performed", "canceled" }));
  }

  [Test]
  public void Button型は離しのしきい値を下回るとstartedに戻り押し直すと再びperformed() {
    var action = CreateAction(InputActionType.Button, "<Gamepad>/leftTrigger");
    action.Enable();

    Set(_gamepad.leftTrigger, 0.9f);
    // NOTE: 既定は押下0.5・離し係数0.75なので0.375以下で離し扱い
    Set(_gamepad.leftTrigger, 0.3f);
    Set(_gamepad.leftTrigger, 0.9f);

    Assert.That(_log, Is.EqualTo(new[] { "started", "performed", "started", "performed" }));
  }

  [Test]
  public void Button型は握ったまま有効にするとアナログの揺れでperformedする() {
    var action = CreateAction(InputActionType.Button, "<Gamepad>/leftTrigger");
    Set(_gamepad.leftTrigger, 0.9f);
    action.Enable();
    InputSystem.Update();
    Assert.That(_log, Is.Empty, "Button型は有効にした時点の状態を見ない");

    Set(_gamepad.leftTrigger, 0.89f);
    Assert.That(_log, Is.EqualTo(new[] { "started", "performed" }));
  }

  [Test]
  public void Button型でもwantsInitialStateCheckを立てると握ったまま有効にした時点でperformedする() {
    var action = CreateAction(InputActionType.Button, "<Gamepad>/leftTrigger");
    action.wantsInitialStateCheck = true;
    Set(_gamepad.leftTrigger, 0.9f);
    action.Enable();
    InputSystem.Update();

    Assert.That(_log, Is.EqualTo(new[] { "started", "performed" }));
  }

  [Test]
  public void Value型は握ったまま有効にすると次の更新でperformedする() {
    var action = CreateAction(InputActionType.Value, "<Gamepad>/leftTrigger");
    Set(_gamepad.leftTrigger, 0.9f);
    action.Enable();
    InputSystem.Update();

    Assert.That(_log, Does.Contain("performed"));
  }

  [Test]
  public void ずっと有効なButton型は握ったままの揺れで再発火しない() {
    var action = CreateAction(InputActionType.Button, "<Gamepad>/leftTrigger");
    action.Enable();

    Set(_gamepad.leftTrigger, 0.9f);
    for (int i = 0; i < 10; i++) Set(_gamepad.leftTrigger, 0.85f + i * 0.01f);

    Assert.That(_log, Is.EqualTo(new[] { "started", "performed" }));
  }

  [Test]
  public void 有効なアクションに割り当てを足すと押下中の状態が捨てられ揺れでperformedする() {
    var action = CreateAction(InputActionType.Button, "<Gamepad>/leftTrigger");
    action.Enable();
    Set(_gamepad.leftTrigger, 0.9f);
    _log.Clear();

    action.AddBinding("<Gamepad>/rightTrigger");
    Set(_gamepad.leftTrigger, 0.89f);

    // NOTE: 写し直しの直後に離し待ちが要る根拠。canceledの有無も合わせて見る
    Assert.That(_log, Does.Contain("performed"));
  }

  [Test]
  public void Repeatは握っている間performedを繰り返し離すとcanceled() {
    var action = CreateAction(InputActionType.Button, "<Gamepad>/dpad/up", "Repeat(_repeatDelay=0.5,_repeatInterval=0.1)");
    action.Enable();

    Press(_gamepad.dpad.up);
    // NOTE: タイムアウトは更新のたびにしか見られないため、時間を小刻みに進める
    for (int i = 0; i < 8; i++) {
      currentTime += 0.1;
      InputSystem.Update();
    }
    Release(_gamepad.dpad.up);

    Assert.That(_log[0], Is.EqualTo("started"));
    Assert.That(_log.FindAll(x => x == "performed").Count, Is.GreaterThanOrEqualTo(3));
    Assert.That(_log[^1], Is.EqualTo("canceled"));
  }

  [Test]
  public void 組み合わせキーは握ったままで再発火しない() {
    var action = new InputAction("Test", InputActionType.Button);
    action.AddCompositeBinding("OneModifier").With("Modifier", "<Gamepad>/leftShoulder").With("Binding", "<Gamepad>/rightTrigger");
    action.performed += _ => _log.Add("performed");
    action.Enable();

    Press(_gamepad.leftShoulder);
    Set(_gamepad.rightTrigger, 0.9f);
    Set(_gamepad.rightTrigger, 0.88f);

    Assert.That(_log, Is.EqualTo(new[] { "performed" }));
  }

  [Test]
  public void デジタルのButton型は押したまま有効にすると離してもcanceledが来ない() {
    var action = CreateAction(InputActionType.Button, "<Gamepad>/buttonSouth");
    Press(_gamepad.buttonSouth);
    action.Enable();
    InputSystem.Update();
    Release(_gamepad.buttonSouth);

    // NOTE: 離し待ちをcanceledで解けない根拠
    Assert.That(_log, Is.Empty);
  }

  [Test]
  public void コールバックの中のupdateCountはすでに今回の更新の値になっている() {
    var action = new InputAction("Test", InputActionType.Button, "<Gamepad>/buttonSouth");
    uint countInCallback = 0;
    action.performed += _ => countInCallback = InputState.updateCount;
    action.Enable();
    uint countBefore = InputState.updateCount;

    Press(_gamepad.buttonSouth);

    // NOTE: 持ち主の判定で、コールバックの中で押した回と今回の回が一致することの根拠
    Assert.That(countInCallback, Is.Not.EqualTo(countBefore));
    Assert.That(InputState.updateCount, Is.EqualTo(countInCallback));
  }

  [Test]
  public void onBeforeUpdateの中で作って有効にしたアクションは同じ更新の押下を受ける() {
    InputAction action = null;
    System.Action create = () => {
      if (action != null) return;
      action = CreateAction(InputActionType.Button, "<Gamepad>/buttonSouth");
      action.Enable();
    };
    InputSystem.onBeforeUpdate += create;
    try {
      Press(_gamepad.buttonSouth);
    } finally {
      InputSystem.onBeforeUpdate -= create;
    }

    // NOTE: 台帳のつなぎ直しを入力更新の前に行えば、その更新の押下に間に合うことの根拠
    Assert.That(_log, Is.EqualTo(new[] { "started", "performed" }));
  }

  [Test]
  public void onBeforeUpdateの中のupdateCountはこれから処理する更新の値になっている() {
    var action = new InputAction("Test", InputActionType.Button, "<Gamepad>/buttonSouth");
    uint countInCallback = 0;
    uint countBeforeUpdate = 0;
    action.performed += _ => countInCallback = InputState.updateCount;
    action.Enable();
    System.Action capture = () => countBeforeUpdate = InputState.updateCount;
    InputSystem.onBeforeUpdate += capture;
    try {
      Press(_gamepad.buttonSouth);
    } finally {
      InputSystem.onBeforeUpdate -= capture;
    }

    // NOTE: 入力更新の前につないだ受け手に有効になった印を押すと、その更新の押下を落とすことの根拠
    Assert.That(countBeforeUpdate, Is.EqualTo(countInCallback));
  }
}

}
