using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Input.Interaction;
using LimeLibrary.UI;
using LimeLibrary.UI.Module.Input;
using NUnit.Framework;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LimeLibrary.Tests.UI {

public class UISharedInputActionTests : InputTestFixture {
  private const string RepeatInteractions = "Repeat(_repeatDelay=0.5,_repeatInterval=0.1)";

  private Gamepad _gamepad;
  private GameObject _rootObject;
  private TestUI _ui;
  private readonly List<UIInputReceiver> _receiverList = new();
  private readonly List<string> _log = new();
  private readonly List<Object> _objectList = new();

  public override void Setup() {
    base.Setup();
    // NOTE: InputTestFixtureは入力の状態を差し替えるが台帳の静的な状態は残るため、テストごとに作り直す
    UISharedInputActionRegistry.ResetStaticState();
    // NOTE: InputTestFixtureが入力の状態を作り直してもRepeatを使えるよう、念のため登録し直す
    InputSystem.RegisterInteraction<RepeatInteraction>();
    _gamepad = InputSystem.AddDevice<Gamepad>();
    _rootObject = new GameObject("UISharedInputActionTests");
    _ui = new TestUI(_rootObject);
    _log.Clear();
  }

  public override void TearDown() {
    foreach (var receiver in _receiverList) {
      receiver.Dispose();
    }
    _receiverList.Clear();
    UISharedInputActionRegistry.ResetStaticState();
    foreach (var obj in _objectList) {
      Object.DestroyImmediate(obj);
    }
    _objectList.Clear();
    Object.DestroyImmediate(_rootObject);
    base.TearDown();
  }

  private UIInputReceiver CreateReceiver(string name, string bindingPath = null, string interactions = "") {
    var receiver = new UIInputReceiver(_ui, InputActionType.Button, interactions);
    receiver.OnInputObservable.Subscribe(_ => _log.Add($"{name}:press"));
    receiver.OnReleaseObservable.Subscribe(_ => _log.Add($"{name}:release"));
    receiver.OnPressCancelObservable.Subscribe(_ => _log.Add($"{name}:cancel"));
    if (bindingPath != null) receiver.AddInputBinding(bindingPath);
    _receiverList.Add(receiver);
    return receiver;
  }

  [Test]
  public void 割り当てを足した直後の入力更新から押下と離しが届く() {
    CreateReceiver("A", "<Gamepad>/buttonSouth");

    Press(_gamepad.buttonSouth);
    Release(_gamepad.buttonSouth);

    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:release" }));
  }

  [Test]
  public void 同じ割り当て一式の受け手は共有アクションを1つだけ使う() {
    CreateReceiver("A", "<Gamepad>/buttonSouth");
    CreateReceiver("B", "<Gamepad>/buttonSouth");
    CreateReceiver("C", "<Gamepad>/buttonEast");
    InputSystem.Update();

    Assert.That(InputSystem.ListEnabledActions().Count, Is.EqualTo(2));
  }

  [Test]
  public void 押した瞬間に有効だった受け手すべてに届く() {
    CreateReceiver("A", "<Gamepad>/buttonSouth");
    CreateReceiver("B", "<Gamepad>/buttonSouth");
    var disabled = CreateReceiver("C", "<Gamepad>/buttonSouth");
    disabled.Enabled = false;

    Press(_gamepad.buttonSouth);

    Assert.That(_log, Is.EquivalentTo(new[] { "A:press", "B:press" }));
  }

  [Test]
  public void 押下の処理中に有効になった受け手には同じ押下を届けず次の押下から届ける() {
    var opener = CreateReceiver("A", "<Gamepad>/buttonSouth");
    var opened = CreateReceiver("B", "<Gamepad>/buttonSouth");
    opened.Enabled = false;
    opener.OnInputObservable.Subscribe(_ => opened.Enabled = true);

    Press(_gamepad.buttonSouth);
    Release(_gamepad.buttonSouth);
    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:release" }));

    _log.Clear();
    Press(_gamepad.buttonSouth);
    Assert.That(_log, Is.EquivalentTo(new[] { "A:press", "B:press" }));
  }

  [Test]
  public void 押下の処理中に作った受け手には同じ押下を届けず次の押下から届ける() {
    var opener = CreateReceiver("A", "<Gamepad>/buttonSouth");
    opener.OnInputObservable.Take(1).Subscribe(_ => CreateReceiver("B", "<Gamepad>/buttonSouth"));

    Press(_gamepad.buttonSouth);
    Release(_gamepad.buttonSouth);
    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:release" }));

    _log.Clear();
    Press(_gamepad.buttonSouth);
    Assert.That(_log, Is.EquivalentTo(new[] { "A:press", "B:press" }));
  }

  [Test]
  public void UI以外の入力の処理中に有効になった受け手には同じ押下を届けない() {
    var opened = CreateReceiver("B", "<Gamepad>/buttonSouth");
    opened.Enabled = false;
    var gameAction = new InputAction("Game", InputActionType.Button, "<Gamepad>/buttonSouth");
    gameAction.performed += _ => opened.Enabled = true;
    gameAction.Enable();

    try {
      Press(_gamepad.buttonSouth);
      Release(_gamepad.buttonSouth);
      Assert.That(_log, Is.Empty);

      Press(_gamepad.buttonSouth);
      Assert.That(_log, Is.EqualTo(new[] { "B:press" }));
    } finally {
      gameAction.Dispose();
    }
  }

  [Test]
  public void 押下の処理中に無効になった受け手には押下も離しも届けない() {
    var closer = CreateReceiver("A", "<Gamepad>/buttonSouth");
    var closed = CreateReceiver("B", "<Gamepad>/buttonSouth");
    closer.OnInputObservable.Subscribe(_ => closed.Enabled = false);

    Press(_gamepad.buttonSouth);
    closed.Enabled = true;
    Release(_gamepad.buttonSouth);

    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:release" }));
  }

  [Test]
  public void 押下を受けたあと無効にした受け手には打ち切りを伝え有効に戻しても離しは届けない() {
    var receiver = CreateReceiver("A", "<Gamepad>/buttonSouth");

    Press(_gamepad.buttonSouth);
    receiver.Enabled = false;
    receiver.Enabled = true;
    Release(_gamepad.buttonSouth);

    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:cancel" }));
  }

  [Test]
  public void 押下の処理の中で自分を無効にしてから離された受け手には押下を届け終えてから打ち切りだけを伝える() {
    var receiver = CreateReceiver("A", "<Gamepad>/buttonSouth");
    receiver.OnInputObservable.Subscribe(_ => {
      receiver.Enabled = false;
      // NOTE: ResetDeviceは押下の処理の中で同期してcanceledを出すため、押下を届けている途中の離しになる
      InputSystem.ResetDevice(_gamepad);
      _log.Add("A:pressEnd");
    });

    Press(_gamepad.buttonSouth);
    InputSystem.Update();

    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:pressEnd", "A:cancel" }));
  }

  [Test]
  public void 押下の処理の中で離された受け手には押下を届け終えてから離しを伝える() {
    var receiver = CreateReceiver("A", "<Gamepad>/buttonSouth");
    receiver.OnInputObservable.Subscribe(_ => {
      InputSystem.ResetDevice(_gamepad);
      _log.Add("A:pressEnd");
    });

    Press(_gamepad.buttonSouth);
    InputSystem.Update();

    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:pressEnd", "A:release" }));
  }

  [Test]
  public void 押下の処理の中で自分を無効にした受け手には押下を届け終えてから打ち切りを伝える() {
    var receiver = CreateReceiver("A", "<Gamepad>/buttonSouth");
    receiver.OnInputObservable.Subscribe(_ => {
      receiver.Enabled = false;
      _log.Add("A:pressEnd");
    });

    Press(_gamepad.buttonSouth);

    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:pressEnd", "A:cancel" }));
  }

  [Test]
  public void Repeatの途中で非表示になった受け手は再表示しても反復を届けない() {
    CreateReceiver("A", "<Gamepad>/dpad/up", RepeatInteractions);
    InputSystem.Update();

    Press(_gamepad.dpad.up);
    AdvanceTime(1.0);
    Assert.That(_log.FindAll(x => x == "A:press").Count, Is.GreaterThanOrEqualTo(2));

    _log.Clear();
    // NOTE: 反復の合間に非表示と再表示を済ませ、届けるときの判定だけでは気づけない場面にする
    _ui.HideEnd();
    _ui.ShowEnd();
    AdvanceTime(1.0);
    Release(_gamepad.dpad.up);
    Assert.That(_log, Is.EqualTo(new[] { "A:cancel" }));

    _log.Clear();
    Press(_gamepad.dpad.up);
    Assert.That(_log, Is.EqualTo(new[] { "A:press" }));
  }

  [Test]
  public void Repeatの途中で受け付ける条件を外れた受け手は条件が戻っても反復を届けない() {
    var receiver = CreateReceiver("A", "<Gamepad>/dpad/up", RepeatInteractions);
    bool isConditionMet = true;
    receiver.SetEnableCondition(() => isConditionMet);
    InputSystem.Update();

    Press(_gamepad.dpad.up);
    AdvanceTime(1.0);
    Assert.That(_log.FindAll(x => x == "A:press").Count, Is.GreaterThanOrEqualTo(2));

    _log.Clear();
    isConditionMet = false;
    AdvanceTime(0.5);
    isConditionMet = true;
    AdvanceTime(1.0);
    Release(_gamepad.dpad.up);
    Assert.That(_log, Is.EqualTo(new[] { "A:cancel" }));
  }

  [Test]
  public void 無効の間に押し始めたRepeatは押したまま有効にしても届けない() {
    var receiver = CreateReceiver("A", "<Gamepad>/dpad/up", RepeatInteractions);
    InputSystem.Update();

    receiver.Enabled = false;
    Press(_gamepad.dpad.up);
    receiver.Enabled = true;
    AdvanceTime(1.0);
    Release(_gamepad.dpad.up);

    Assert.That(_log, Is.Empty);
  }

  [Test]
  public void 押したまま割り当てを外すと押下の打ち切りを伝え以後は届けない() {
    var receiver = CreateReceiver("A", "<Gamepad>/buttonSouth");

    Press(_gamepad.buttonSouth);
    receiver.ClearInputBinding();
    InputSystem.Update();
    Release(_gamepad.buttonSouth);
    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:cancel" }));

    _log.Clear();
    PressAndRelease(_gamepad.buttonSouth);
    Assert.That(_log, Is.Empty);
  }

  [Test]
  public void 押したまま割り当てを足すと押下の打ち切りを伝え押し直しから届ける() {
    var receiver = CreateReceiver("A", "<Gamepad>/buttonSouth");

    Press(_gamepad.buttonSouth);
    receiver.AddInputBinding("<Gamepad>/buttonEast");
    InputSystem.Update();
    Release(_gamepad.buttonSouth);
    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:cancel" }));

    _log.Clear();
    PressAndRelease(_gamepad.buttonSouth);
    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:release" }));
  }

  [Test]
  public void 押下の処理中に無効から有効へ戻した受け手には同じ押下を届けない() {
    var toggler = CreateReceiver("A", "<Gamepad>/buttonSouth");
    var toggled = CreateReceiver("B", "<Gamepad>/buttonSouth");
    toggler.OnInputObservable.Take(1).Subscribe(_ => {
      toggled.Enabled = false;
      toggled.Enabled = true;
    });

    PressAndRelease(_gamepad.buttonSouth);
    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:release" }));

    _log.Clear();
    PressAndRelease(_gamepad.buttonSouth);
    Assert.That(_log, Is.EquivalentTo(new[] { "A:press", "B:press", "A:release", "B:release" }));
  }

  [Test]
  public void 押されたまま作った共有アクションは離すまで押下を届けない() {
    Set(_gamepad.leftTrigger, 0.9f);
    CreateReceiver("A", "<Gamepad>/leftTrigger");
    InputSystem.Update();

    Set(_gamepad.leftTrigger, 0.89f);
    Set(_gamepad.leftTrigger, 0.0f);
    Assert.That(_log, Is.Empty);

    Set(_gamepad.leftTrigger, 0.9f);
    Assert.That(_log, Is.EqualTo(new[] { "A:press" }));
  }

  [Test]
  public void 押されたまま作った共有アクションでも同じ入力更新の中で離して押し直したボタンは届ける() {
    Press(_gamepad.buttonSouth);
    CreateReceiver("A", "<Gamepad>/buttonSouth");
    InputSystem.Update();
    Assert.That(_log, Is.Empty);

    Release(_gamepad.buttonSouth, queueEventOnly: true);
    Press(_gamepad.buttonSouth, queueEventOnly: true);
    InputSystem.Update();

    Assert.That(_log, Is.EqualTo(new[] { "A:press" }));
  }

  [Test]
  public void interactionの押下点が低くても押されたまま作った共有アクションは離すまで押下を届けない() {
    Set(_gamepad.leftTrigger, 0.3f);
    var receiver = new UIInputReceiver(_ui, InputActionType.Button, "Press(pressPoint=0.2)");
    receiver.OnInputObservable.Subscribe(_ => _log.Add("A:press"));
    receiver.AddInputBinding("<Gamepad>/leftTrigger");
    _receiverList.Add(receiver);
    InputSystem.Update();

    Set(_gamepad.leftTrigger, 0.31f);
    Set(_gamepad.leftTrigger, 0.0f);
    Assert.That(_log, Is.Empty);

    Set(_gamepad.leftTrigger, 0.3f);
    Assert.That(_log, Is.EqualTo(new[] { "A:press" }));
  }

  [Test]
  public void 押されたまま作った共有アクションでも同じ入力更新の中で離して握り直したトリガーは届ける() {
    Set(_gamepad.leftTrigger, 0.9f);
    CreateReceiver("A", "<Gamepad>/leftTrigger");
    InputSystem.Update();
    Set(_gamepad.leftTrigger, 0.89f);
    Assert.That(_log, Is.Empty);

    Set(_gamepad.leftTrigger, 0.0f, queueEventOnly: true);
    Set(_gamepad.leftTrigger, 0.9f, queueEventOnly: true);
    InputSystem.Update();

    Assert.That(_log, Is.EqualTo(new[] { "A:press" }));
  }

  [Test]
  public void 修飾キーだけを押したまま作った組み合わせの共有アクションは最初の組み合わせ入力から届ける() {
    var sourceAction = CreateModifierAction();
    try {
      Press(_gamepad.leftShoulder);
      CreateReceiver("A").AddInputBinding(sourceAction);
      InputSystem.Update();

      Press(_gamepad.buttonSouth);
      Assert.That(_log, Is.EqualTo(new[] { "A:press" }));
    } finally {
      sourceAction.Dispose();
    }
  }

  [Test]
  public void 修飾キーと組み合わせのキーを押したまま作った共有アクションは押し直すまで届けない() {
    var sourceAction = CreateModifierAction();
    try {
      Press(_gamepad.leftShoulder);
      Press(_gamepad.buttonSouth);
      CreateReceiver("A").AddInputBinding(sourceAction);
      InputSystem.Update();
      Assert.That(_log, Is.Empty);

      Release(_gamepad.buttonSouth);
      Press(_gamepad.buttonSouth);
      Assert.That(_log, Is.EqualTo(new[] { "A:press" }));
    } finally {
      sourceAction.Dispose();
    }
  }

  [Test]
  public void 修飾キーが押下点に届かないまま作った組み合わせの共有アクションは修飾キーを押し込んだ入力を届ける() {
    var sourceAction = new InputAction("Source", InputActionType.Button);
    sourceAction.AddCompositeBinding("OneModifier(modifiersOrder=2)").With("Modifier", "<Gamepad>/leftTrigger").With("Binding", "<Gamepad>/buttonSouth");
    try {
      // NOTE: 0.4は既定の離し点0.375を超えるが押下点0.5に届かず、コンポジットはまだ成立していない
      Set(_gamepad.leftTrigger, 0.4f);
      Press(_gamepad.buttonSouth);
      CreateReceiver("A").AddInputBinding(sourceAction);
      InputSystem.Update();
      Assert.That(_log, Is.Empty);

      Set(_gamepad.leftTrigger, 0.9f);
      Assert.That(_log, Is.EqualTo(new[] { "A:press" }));
    } finally {
      sourceAction.Dispose();
    }
  }

  [Test]
  public void 別名のパスでも押されたまま作った共有アクションは離すまで押下を届けない() {
    Press(_gamepad.buttonSouth);
    CreateReceiver("A", "<Gamepad>/a");
    InputSystem.Update();
    Assert.That(_log, Is.Empty);

    Release(_gamepad.buttonSouth);
    Press(_gamepad.buttonSouth);
    Assert.That(_log, Is.EqualTo(new[] { "A:press" }));
  }

  private static InputAction CreateModifierAction() {
    var action = new InputAction("Source", InputActionType.Button);
    action.AddCompositeBinding("OneModifier").With("Modifier", "<Gamepad>/leftShoulder").With("Binding", "<Gamepad>/buttonSouth");
    return action;
  }

  [Test]
  public void 元のアクションの割り当てを変えると共有アクションが写し直される() {
    var asset = ScriptableObject.CreateInstance<InputActionAsset>();
    _objectList.Add(asset);
    var sourceAction = asset.AddActionMap("Map").AddAction("Decide", InputActionType.Button, "<Gamepad>/buttonSouth");
    asset.Enable();

    var receiver = new UIInputReceiver(_ui);
    receiver.OnInputObservable.Subscribe(_ => _log.Add("A:press"));
    receiver.AddInputBinding(sourceAction);
    _receiverList.Add(receiver);

    PressAndRelease(_gamepad.buttonSouth);
    Assert.That(_log, Is.EqualTo(new[] { "A:press" }));

    _log.Clear();
    sourceAction.ApplyBindingOverride(0, "<Gamepad>/buttonEast");
    PressAndRelease(_gamepad.buttonSouth);
    Assert.That(_log, Is.Empty);
    PressAndRelease(_gamepad.buttonEast);
    Assert.That(_log, Is.EqualTo(new[] { "A:press" }));
    Assert.That(sourceAction.enabled, Is.True, "元のアクションの有効・無効は変えない");
  }

  [Test]
  public void 割り当てを外すと届かなくなる() {
    var receiver = CreateReceiver("A", "<Gamepad>/buttonSouth");
    PressAndRelease(_gamepad.buttonSouth);
    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:release" }));

    _log.Clear();
    receiver.ClearInputBinding();
    PressAndRelease(_gamepad.buttonSouth);
    Assert.That(_log, Is.Empty);
  }

  [Test]
  public void つないだあとに破棄した受け手には届かない() {
    var receiver = CreateReceiver("A", "<Gamepad>/buttonSouth");
    CreateReceiver("B", "<Gamepad>/buttonSouth");
    PressAndRelease(_gamepad.buttonSouth);
    Assert.That(_log, Is.EquivalentTo(new[] { "A:press", "A:release", "B:press", "B:release" }));

    _log.Clear();
    Press(_gamepad.buttonSouth);
    receiver.Dispose();
    Assert.That(CountReceiverReferences(receiver), Is.Zero, "破棄したら押下中でも共有アクションから参照を外す");
    Release(_gamepad.buttonSouth);
    PressAndRelease(_gamepad.buttonSouth);

    Assert.That(_log, Is.EqualTo(new[] { "A:press", "B:press", "B:release", "B:press", "B:release" }));
  }

  [Test]
  public void 押したまま割り当てを変えると前の共有アクションから参照を外す() {
    var receiver = CreateReceiver("A", "<Gamepad>/buttonSouth");
    Press(_gamepad.buttonSouth);
    receiver.ClearInputBinding();
    InputSystem.Update();

    Assert.That(CountReceiverReferences(receiver), Is.Zero);
  }

  private static int CountReceiverReferences(UIInputReceiver receiver) {
    // NOTE: 参照の外し漏れは通知のログには表れないため、共有アクションの一覧を直接数える
    const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    var map = (IDictionary)typeof(UISharedInputActionRegistry).GetField("s_sharedInputActionMap", flags).GetValue(null);
    int count = 0;
    foreach (var sharedInputAction in map.Values) {
      foreach (string fieldName in new[] { "_listenerList", "_pressCandidateList", "_pressOwnerList", "_pendingCancelList", "_pendingReleaseList" }) {
        var list = (IList)typeof(UISharedInputAction).GetField(fieldName, flags).GetValue(sharedInputAction);
        if (list.Contains(receiver)) count++;
      }
    }
    return count;
  }

  [Test]
  public void 押下の処理中にほかの受け手を破棄しても残りの受け手には届く() {
    var disposer = CreateReceiver("A", "<Gamepad>/buttonSouth");
    var disposed = CreateReceiver("B", "<Gamepad>/buttonSouth");
    CreateReceiver("C", "<Gamepad>/buttonSouth");
    disposer.OnInputObservable.Subscribe(_ => disposed.Dispose());

    PressAndRelease(_gamepad.buttonSouth);

    Assert.That(_log, Is.EqualTo(new[] { "A:press", "C:press", "A:release", "C:release" }));
  }

  [Test]
  public void 受け付けを止めた受け手には届かず再開した入力更新の押下も届かない() {
    var receiver = CreateReceiver("A", "<Gamepad>/buttonSouth");
    receiver.DisableInputAction();
    PressAndRelease(_gamepad.buttonSouth);
    Assert.That(_log, Is.Empty);

    var gameAction = new InputAction("Game", InputActionType.Button, "<Gamepad>/buttonSouth");
    gameAction.performed += _ => receiver.EnableInputAction();
    gameAction.Enable();
    try {
      PressAndRelease(_gamepad.buttonSouth);
      Assert.That(_log, Is.Empty);
    } finally {
      gameAction.Dispose();
    }

    PressAndRelease(_gamepad.buttonSouth);
    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:release" }));
  }

  [Test]
  public void 非表示の間に押し始めた長押しは表示してから成立しても届けない() {
    CreateReceiver("A", "<Gamepad>/buttonSouth", "Hold(duration=0.5)");
    InputSystem.Update();

    _ui.HideEnd();
    Press(_gamepad.buttonSouth);
    _ui.ShowEnd();
    AdvanceTime(1.0);
    Release(_gamepad.buttonSouth);
    Assert.That(_log, Is.Empty);

    Press(_gamepad.buttonSouth);
    AdvanceTime(1.0);
    Release(_gamepad.buttonSouth);
    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:release" }));
  }

  [Test]
  public void 非表示の間に押し始めた離しで成立する入力は表示してから離しても届けない() {
    CreateReceiver("A", "<Gamepad>/buttonSouth", "Press(behavior=1)");
    InputSystem.Update();

    _ui.HideEnd();
    Press(_gamepad.buttonSouth);
    _ui.ShowEnd();
    Release(_gamepad.buttonSouth);
    Assert.That(_log, Is.Empty);

    PressAndRelease(_gamepad.buttonSouth);
    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:release" }));
  }

  [Test]
  public void 非表示の間に少しずつ握り始めた離しで成立するトリガーは表示してから離しても届けない() {
    CreateReceiver("A", "<Gamepad>/leftTrigger", "Press(behavior=1)");
    InputSystem.Update();

    // NOTE: 押下点に届く前の0.1でStartedが来て、押下点を越えても次の通知は離すまで来ない
    _ui.HideEnd();
    Set(_gamepad.leftTrigger, 0.1f);
    Set(_gamepad.leftTrigger, 0.9f);
    _ui.ShowEnd();
    Set(_gamepad.leftTrigger, 0.0f);
    Assert.That(_log, Is.Empty);

    Set(_gamepad.leftTrigger, 0.1f);
    Set(_gamepad.leftTrigger, 0.9f);
    Set(_gamepad.leftTrigger, 0.0f);
    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:release" }));
  }

  [Test]
  public void 押下点に届く前に有効になった受け手にはトリガーを押し込んだ押下を届ける() {
    var receiver = CreateReceiver("A", "<Gamepad>/leftTrigger");
    InputSystem.Update();

    receiver.Enabled = false;
    Set(_gamepad.leftTrigger, 0.1f);
    receiver.Enabled = true;
    Set(_gamepad.leftTrigger, 0.9f);

    Assert.That(_log, Is.EqualTo(new[] { "A:press" }));
  }

  [Test]
  public void 押下点を上げたReleaseOnlyで非表示の間に握り始めたトリガーは表示してから離しても届けない() {
    CreateReceiver("A", "<Gamepad>/leftTrigger", "Press(behavior=1,pressPoint=0.9)");
    InputSystem.Update();

    // NOTE: 離しの0.6は離し点0.675を下回るが、コントロールの押下点0.5は上回る
    _ui.HideEnd();
    Set(_gamepad.leftTrigger, 0.1f);
    Set(_gamepad.leftTrigger, 0.95f);
    _ui.ShowEnd();
    Set(_gamepad.leftTrigger, 0.6f);
    Assert.That(_log, Is.Empty);

    Set(_gamepad.leftTrigger, 0.0f);
    Set(_gamepad.leftTrigger, 0.1f);
    Set(_gamepad.leftTrigger, 0.95f);
    Set(_gamepad.leftTrigger, 0.6f);
    Assert.That(_log, Is.EqualTo(new[] { "A:press", "A:release" }));
  }

  private void AdvanceTime(double seconds) {
    // NOTE: interactionのタイムアウトは入力更新のたびにしか見られないため、時間を小刻みに進める
    int stepCount = (int)System.Math.Round(seconds / 0.1);
    for (int i = 0; i < stepCount; i++) {
      currentTime += 0.1;
      InputSystem.Update();
    }
  }

  private sealed class TestUI : IUI {
    private readonly Subject<Unit> _onShowEndSubject = new();
    private readonly Subject<Unit> _onHideEndSubject = new();

    public GameObject RootObject { get; }
    public Observable<Unit> OnShowEndObservable => _onShowEndSubject;
    public Observable<Unit> OnHideEndObservable => _onHideEndSubject;
    public IUIInputObservables InputObservables => null;

    public TestUI(GameObject rootObject) {
      RootObject = rootObject;
    }

    public void ShowEnd() => _onShowEndSubject.OnNext(Unit.Default);
    public void HideEnd() => _onHideEndSubject.OnNext(Unit.Default);

    public UniTask Show(CancellationToken cancellationToken) => UniTask.CompletedTask;
    public UniTask Hide(CancellationToken cancellationToken) => UniTask.CompletedTask;
  }
}

}
