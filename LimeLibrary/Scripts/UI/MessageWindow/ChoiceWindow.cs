using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.UI.Module.Input;
using LimeLibrary.UI.Module.SelectableGroup;
using LimeLibrary.UI.Parts;
using LimeLibrary.UI.View;
using LimeLibrary.Utility;
using TMPro;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LimeLibrary.UI.MessageWindow {

public class ChoiceWindow : UISingleView {
  [SerializeField]
  private GameObject _choiceTextPrefab;
  [SerializeField]
  private int _maxChoice = 8;
  [SerializeField]
  private InputAction _cancelInputAction;

  private class ChoiceData {
    public GameObject GameObject { get; set; }
    public TextMeshProUGUI Text { get; set; }
    public UIButton Button { get; set; }
  }

  private readonly List<ChoiceData> _choiceDataList = new List<ChoiceData>(32);
  private SelectableGroup _selectableGroup;
  private UIInputReceiver _inputReceiver;

  public bool IsEnableCancel { get; set; }

  protected override UniTask OnInitialize(CancellationToken cancellationToken) {
    Animator.RegisterShowHideFadeAnimation(CanvasGroup, 0.1f);

    _selectableGroup = new SelectableGroupVertical(this, SelectableGroupSelectMode.Auto);
    _inputReceiver = new UIInputReceiver(this);
    _inputReceiver.AddInputBinding(_cancelInputAction);
    _choiceTextPrefab.SetActive(false);

    EventObservables.GetObservable(UIViewEventType.ShowEnd).Subscribe(_ => {
      _selectableGroup.Select(0);
    }).AddTo(gameObject);

    return UniTask.CompletedTask;
  }

  public void AddChoice(string text) {
    if (_choiceDataList.Count >= _maxChoice) {
      Assertion.Assert(false, $"Choice count is over {_maxChoice}.");
      return;
    }

    var choiceData = new ChoiceData();
    choiceData.GameObject = UnityUtility.Instantiate(_choiceTextPrefab, gameObject.transform);
    choiceData.GameObject.SetActive(true);
    choiceData.Text = choiceData.GameObject.GetComponent<TextMeshProUGUI>();
    choiceData.Text.text = text;
    choiceData.Button = choiceData.GameObject.GetComponent<UIButton>();
    choiceData.Button.Initialize(this);

    _choiceDataList.Add(choiceData);
    _selectableGroup.AddSelectable(choiceData.Button.Button);
    _selectableGroup.SetupNavigation();
  }

  public void ClearChoice() {
    foreach (var choiceData in _choiceDataList) {
      Destroy(choiceData.GameObject);
    }
    _choiceDataList.Clear();
    _selectableGroup.ClearSelectable();
  }

  public async UniTask<int> WaitSelect(CancellationToken cancellationToken) {
    var buttonTaskList = _choiceDataList.Select(choiceData => choiceData.Button.Button.OnClickAsync(cancellationToken)).ToList();
    if (IsEnableCancel) {
      buttonTaskList.Add(_inputReceiver.OnInputObservable.FirstAsync(cancellationToken).AsUniTask());
    }
    int result = await UniTask.WhenAny(buttonTaskList);
    if (IsEnableCancel && result == buttonTaskList.Count - 1) {
      result = buttonTaskList.Count - 2;
    }
    return result;
  }
}

}