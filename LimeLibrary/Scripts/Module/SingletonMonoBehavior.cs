using UnityEngine;
using Logger = LimeLibrary.Utility.Logger;

namespace LimeLibrary.Module {

/// <summary>
/// Singletonパターン
/// MonoBehaviour版
/// </summary>
public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {
  private static T s_instance;
  public static T Instance {
    get {
      if (s_instance == null) {
        var t = typeof(T);

        s_instance = (T) FindObjectOfType(t);
        if (s_instance == null) {
          Logger.LogError(t + " をアタッチしているGameObjectはありません");
        }
      }

      return s_instance;
    }
  }

  public static bool Attached {
    get {
      if (s_instance != null) return true;
      var instance = FindObjectOfType(typeof(T));
      return instance != null;
    }
  }

  protected virtual void Awake() {
    // 他のGameObjectにアタッチされているか調べて、アタッチされている場合は破棄する
    if (this == Instance) return;

    Destroy(this);
    Logger.LogError(
      typeof(T) +
      " は既に他のGameObjectにアタッチされているため、コンポーネントを破棄しました." +
      " アタッチされているGameObjectは " +
      Instance.gameObject.name +
      " です.");
  }
}

}