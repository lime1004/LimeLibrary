using LimeLibrary.Extensions;
using LimeLibrary.UI.View;
using UnityEngine;

namespace LimeLibrary.UI.Parts {

public interface IUIParts {
  public IUIView ParentView { get; }
  public RectTransform RectTransform { get; }

  public void Initialize(IUIView parentView);
}

}