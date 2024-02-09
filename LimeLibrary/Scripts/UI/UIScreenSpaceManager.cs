using UnityEngine;

namespace LimeLibrary.UI {

/// <summary>
/// ScreenSpaceのUI管理
/// </summary>
[RequireComponent(typeof(Canvas))]
public class UIScreenSpaceManager : MonoBehaviour {
  [SerializeField]
  private Camera _camera;

  public GameObject RootObject { get; private set; }
  public Camera Camera => _camera;

  private void Awake() {
    var canvas = GetComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceCamera;
    canvas.worldCamera = _camera;
    RootObject = gameObject;
  }
}

}