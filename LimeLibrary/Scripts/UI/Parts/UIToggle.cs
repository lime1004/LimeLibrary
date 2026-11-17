using LimeLibrary.Extensions;
using LimeLibrary.UI.Module.Input;
using LimeLibrary.UI.View;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace LimeLibrary.UI.Parts {

[RequireComponent(typeof(Toggle))]
public class UIToggle : MonoBehaviour, IUIParts {
  private UIInputReceiver _inputReceiver;

  private readonly Subject<bool> _onValueChangeSubject = new();
  private readonly ClickRangeAdjuster _clickRangeAdjuster = new();
  private bool _isInitialized;

  public IUIView ParentView { get; private set; }
  public RectTransform RectTransform => transform.AsRectTransform();

  public Toggle Toggle { get; private set; }
  public Observable<bool> OnValueChangeObservable => _onValueChangeSubject;

  public void Initialize(IUIView parentView) {
    if (_isInitialized) return;

    ParentView = parentView;

    Toggle = GetComponent<Toggle>();
    Toggle.OnValueChangedAsObservable().Subscribe(isOn => {
      if (!IsEnable()) return;
      _onValueChangeSubject.OnNext(isOn);
    }).AddTo(gameObject);

    _isInitialized = true;
  }

  private void CreateInputReceiverIfNeeded() {
    if (_inputReceiver != null) return;

    _inputReceiver = new UIInputReceiver(ParentView);
    // NOTE: コンポーネントだけを無効にした間もキー入力では切り替えない
    _inputReceiver.SetEnableCondition(() => IsEnable() && enabled);
    _inputReceiver.OnInputObservable.Subscribe(_ => Toggle.isOn = !Toggle.isOn).AddTo(gameObject);
    // NOTE: 一度もアクティブにならずに破棄されるとOnDestroyが呼ばれないため、破棄の検知はR3に任せる
    Disposable.Create(_inputReceiver.Dispose).AddTo(gameObject);
  }

  public void SetIsOn(bool isOn) {
    Toggle.SetIsOnWithoutNotify(isOn);
  }

  /// <summary>
  /// 切り替えのキー入力のパスを足す。反映は次の入力更新から
  /// </summary>
  public void AddInputBinding(string path) {
    CreateInputReceiverIfNeeded();
    _inputReceiver.AddInputBinding(path);
  }

  private bool IsEnable() {
    // 破棄の検知は次のフレームになることがあり、その間に入力のコールバックが走るため破棄済みを弾く
    if (this == null) return false;
    if (gameObject.activeInHierarchy == false) return false;
    if (ParentView == null) return false;
    if (!ParentView.IsEnable()) return false;

    return true;
  }

  public void AdjustButtonRect() => _clickRangeAdjuster.Enable(transform.AsRectTransform());
  public void DisableButtonRect() => _clickRangeAdjuster.Disable();
}

}
