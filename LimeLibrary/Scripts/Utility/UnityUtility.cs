using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LimeLibrary.Utility {

public static class UnityUtility {
  /// <summary>
  /// アプリケーションの終了
  /// </summary>
  public static void QuitApplication() {
#if UNITY_EDITOR
    EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
  }

  /// <summary>
  /// Object.Instantiateのラップ関数
  /// (Clone)を付けたくないだけ
  /// </summary>
  public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation, Transform parent) where T : Object {
    if (original == null) {
      Assertion.Assert(false, "Instantiate元がnullです.");
      return null;
    }

    T gameObject = null;
    if (parent) {
      gameObject = Object.Instantiate<T>(original, position, rotation, parent);
    } else {
      gameObject = Object.Instantiate<T>(original, position, rotation);
    }
    gameObject.name = original.name;
    return gameObject;
  }

  public static T Instantiate<T>(T original, Transform parent, bool isWorldPositionStays = false) where T : Object {
    if (original == null) {
      Assertion.Assert(false, "Instantiate元がnullです.");
      return null;
    }

    T gameObject = null;
    if (parent) {
      gameObject = Object.Instantiate<T>(original, parent, isWorldPositionStays);
    } else {
      gameObject = Object.Instantiate<T>(original);
    }
    gameObject.name = original.name;
    return gameObject;
  }

  public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object {
    return Instantiate<T>(original, position, rotation, null);
  }

  public static T Instantiate<T>(T original) where T : Object {
    return Instantiate<T>(original, null);
  }

  /// <summary>
  /// GamaObjectの生成
  /// </summary>
  public static GameObject CreateGameObject(string name = null, GameObject parentObject = null, UnityEngine.SceneManagement.Scene? scene = null, bool worldPositionStays = true) {
    var gameObject = new GameObject(name);
    if (scene.HasValue) {
      SceneManager.MoveGameObjectToScene(gameObject, scene.Value);
    }
    if (parentObject) {
      gameObject.transform.SetParent(parentObject.transform, worldPositionStays);
    }
    return gameObject;
  }

  /// <summary>
  /// RectTransform付きのGamaObjectの生成
  /// </summary>
  public static GameObject CreateGameObjectWithRectTransform(string name = null, GameObject parentObject = null, UnityEngine.SceneManagement.Scene? scene = null, bool worldPositionStays = true) {
    var gameObject = CreateGameObject(name, parentObject, scene, worldPositionStays);
    var rectTransform = gameObject.AddComponent<RectTransform>();
    rectTransform.localPosition = Vector3.zero;
    rectTransform.localRotation = Quaternion.identity;
    rectTransform.localScale = Vector3.one;
    return gameObject;
  }

  /// <summary>
  /// Easing対応Lerp
  /// </summary>
  public static Vector2 Lerp(Vector2 a, Vector2 b, float t, EasingType easingType) {
    return Vector2.Lerp(a, b, Easing.Ease(easingType, t));
  }

  /// <summary>
  /// Easing対応Lerp
  /// </summary>
  public static Vector3 Lerp(Vector3 a, Vector3 b, float t, EasingType easingType) {
    return Vector3.Lerp(a, b, Easing.Ease(easingType, t));
  }
}

}