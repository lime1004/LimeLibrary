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
  }

  public void OnUpdate() { }
}

}