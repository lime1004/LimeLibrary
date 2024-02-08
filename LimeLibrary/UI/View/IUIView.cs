using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LimeLibrary.UI.View {

public interface IUIView : IUI {
  public RectTransform RectTransform { get; }
  public Canvas Canvas { get; }
  public UIViewState State { get; }
  public bool IsFocus { get; }
  public IUIViewEventObservables EventObservables { get; }
  public CancellationToken ObjectCancellationToken { get; }

  bool IsEnable();

  public UniTask Show(UIViewShowOption showOption, CancellationToken cancellationToken);
  public UniTask Hide(UIViewHideOption hideOption, CancellationToken cancellationToken);

  public void Focus();
  public void Unfocus();
  public void OnDestroyView();

  public void SetSortingOrderFront();

  public void SetPosition(Vector2 position);
  public void SetAnchoredPosition(Vector2 anchoredPosition);
}

}