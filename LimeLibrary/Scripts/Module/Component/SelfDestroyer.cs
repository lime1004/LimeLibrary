using UnityEngine;

namespace LimeLibrary.Module {

public class SelfDestroyer : MonoBehaviour {
  private void Awake() {
    Destroy(gameObject);
  }
}

}