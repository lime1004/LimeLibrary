using UnityEngine;

namespace LimeLibrary.UI {

/// <summary>
/// WorldSpaceのUI管理
/// </summary>
[RequireComponent(typeof(Canvas))]
public class UIWorldSpaceManager : MonoBehaviour {
  [SerializeField]
  private Camera _camera;

  public GameObject RootObject { get; private set; }
  public Camera Camera => _camera;

  private void Awake() {
    var canvas = GetComponent<Canvas>();
    canvas.renderMode = RenderMode.WorldSpace;
    canvas.worldCamera = _camera;
    RootObject = gameObject;

    SetScale();
  }

  public void OnUpdate() {
    SetScale();
  }

  private void SetScale() {
    var resolution = new Vector2Int(Screen.width, Screen.height);
    float scale = _camera.orthographicSize * 2f / resolution.y;
    RootObject.transform.localScale = new Vector3(scale, scale, scale);
  }
}

}