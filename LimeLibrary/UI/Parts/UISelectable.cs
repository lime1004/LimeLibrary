using LimeLibrary.Extensions;
using LimeLibrary.UI.Module.Selectable;
using LimeLibrary.UI.View;
using UnityEngine;

namespace LimeLibrary.UI.Parts {

public class UISelectable : UnityEngine.UI.Selectable, IUIParts {
  private readonly ClickRangeAdjuster _clickRangeAdjuster = new();

  public IUIView ParentView { get; private set; }
  public RectTransform RectTransform => transform.AsRectTransform();

  public SelectableExtender SelectableExtender { get; private set; }

  public ExtendSelectionState ExtendSelectionState => SelectableExtender.ExtendSelectionState;

  public virtual void Initialize(IUIView parentView) {
    ParentView = parentView;
    SelectableExtender = new SelectableExtender(parentView, this);
  }

  public void AdjustButtonRect() => _clickRangeAdjuster.Enable(transform.AsRectTransform());
  public void DisableButtonRect() => _clickRangeAdjuster.Disable();
}

}