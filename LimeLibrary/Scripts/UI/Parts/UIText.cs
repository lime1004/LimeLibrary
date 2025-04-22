using LimeLibrary.Extensions;
using LimeLibrary.UI.View;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LimeLibrary.UI.Parts {

[RequireComponent(typeof(TextMeshProUGUI))]
public class UIText : MonoBehaviour, IUIParts {
  private bool _isInitialized;

  public IUIView ParentView { get; private set; }
  public RectTransform RectTransform => transform.AsRectTransform();

  public TextMeshProUGUI Text { get; private set; }

  public void Initialize(IUIView parentView) {
    if (_isInitialized) return;

    ParentView = parentView;

    Text = GetComponent<TextMeshProUGUI>();

    _isInitialized = true;
  }

  public string GetText() {
    return Text.text;
  }

  public void SetText(string text) {
    text ??= string.Empty;

    Text.SetText(text);
  }

  public void SetText(int num) {
    Text.SetText("{0}", num);
  }

  public void SetAlignment(TextAlignmentOptions textAlignmentOptions) {
    Text.alignment = textAlignmentOptions;
  }

  public void SetAutoSize(float minSize, float maxSize) {
    Text.enableAutoSizing = true;
    Text.fontSizeMin = minSize;
    Text.fontSizeMax = maxSize;
  }

  public void SetFont(TMP_FontAsset fontAsset) {
    Text.font = fontAsset;
  }

#if UNITY_EDITOR
  [MenuItem("GameObject/UI/UIText")]
  public static void CreateUIText() {
    var activeGameObject = Selection.activeGameObject;

    var createGameObject = new GameObject();
    createGameObject.name = "UIText";
    createGameObject.transform.SetParent(activeGameObject.transform, false);
    createGameObject.AddComponent<TextMeshProUGUI>();
    createGameObject.AddComponent<UIText>();
  }
#endif
}

}