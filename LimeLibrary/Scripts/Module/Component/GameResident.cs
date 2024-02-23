using UnityEngine;

namespace LimeLibrary.Module {

public class GameResident : MonoBehaviour {
  private void Awake() {
    DontDestroyOnLoad(gameObject);
  }
}

}