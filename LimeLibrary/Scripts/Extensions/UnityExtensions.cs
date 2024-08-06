using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LimeLibrary.Extensions {

/// <summary>
/// Unityの拡張メソッド
/// </summary>
public static class UnityExtensions {
  /// <summary>
  /// GameObjectのHierarchy内のパスを取得
  /// </summary>
  public static string GetHierarchyPath(this GameObject gameObject) {
    string path = "";
    var current = gameObject.transform;
    while (current != null) {
      // 同じ階層に同名のオブジェクトがある場合があるので、それを回避する
      int index = current.GetSiblingIndex();
      path = "/" + current.name + index + path;
      current = current.parent;
    }

    return "/" + gameObject.scene.name + path;
  }

  /// <summary>
  /// Layerのセット
  /// </summary>
  public static void SetLayer(this GameObject gameObject, int layer, bool isSetChildren = false) {
    gameObject.layer = layer;

    if (!isSetChildren) return;

    foreach (Transform n in gameObject.transform) {
      SetLayer(n.gameObject, layer);
    }
  }

  /// <summary>
  /// 子階層のGameObjectを全てリストに格納
  /// 自分も含む
  /// </summary>
  public static void GetGameObjectAll(this GameObject gameObject, ref List<GameObject> gameObjectList) {
    for (int i = 0; i < gameObject.transform.childCount; i++) {
      var childObject = gameObject.transform.GetChild(i).gameObject;
      if (childObject.transform.childCount == 0) {
        gameObjectList.Add(childObject);
        continue;
      }

      GetGameObjectAll(childObject, ref gameObjectList);
    }

    gameObjectList.Add(gameObject);
  }

  /// <summary>
  /// 指定した名前のオブジェクトを取得
  /// </summary>
  public static Transform FindAll(this Transform transform, string name) {
    if (transform.name == name) return transform;
    foreach (Transform childTransform in transform) {
      var result = FindAll(childTransform, name);
      if (result) return result;
    }
    return null;
  }

  /// <summary>
  /// 指定したComponentがあれば取得、なければAddする
  /// </summary>
  public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component {
    return gameObject.TryGetComponent<T>(out var component) ? component : gameObject.AddComponent<T>();
  }

  /// <summary>
  /// 指定したComponentを削除
  /// </summary>
  public static void RemoveComponent<T>(this GameObject gameObject) where T : Component {
    if (gameObject.TryGetComponent<T>(out var component)) {
      Object.Destroy(component);
    }
  }

  /// <summary>
  /// Nullかどうか
  /// </summary>
  public static bool IsNull(this Component component) {
    return component == null;
  }

  /// <summary>
  /// 子オブジェクトのリストを取得
  /// </summary>
  public static List<Transform> GetChildList(this Transform transform) {
    var childList = new List<Transform>();
    for (int i = 0; i < transform.childCount; i++) {
      childList.Add(transform.GetChild(i));
    }
    return childList;
  }

  /// <summary>
  /// RectTransformを取得
  /// </summary>
  public static RectTransform GetRectTransform(this GameObject gameObject) {
    return gameObject.transform.AsRectTransform();
  }

  /// <summary>
  /// TransformをRectTransformとして取得
  /// </summary>
  public static RectTransform AsRectTransform(this Transform transform) {
    return transform as RectTransform;
  }

  /// <summary>
  /// Pivotを変更する
  /// </summary>
  public static void SetPivot(this RectTransform rectTransform, Vector2 pivot, bool isKeepPosition = true) {
    var diffPivot = pivot - rectTransform.pivot;
    rectTransform.pivot = pivot;

    if (!isKeepPosition) return;

    var sizeDelta = rectTransform.sizeDelta;
    var diffPos = new Vector2(sizeDelta.x * diffPivot.x, sizeDelta.y * diffPivot.y);
    rectTransform.anchoredPosition += diffPos;
  }

  /// <summary>
  /// Pivotを変更する
  /// </summary>
  public static void SetPivot(this RectTransform rectTransform, float x, float y, bool isKeepPosition = true) {
    rectTransform.SetPivot(new Vector2(x, y), isKeepPosition);
  }

  /// <summary>
  /// Anchorを変更する
  /// </summary>
  public static void SetAnchor(this RectTransform rectTransform, Vector2 targetMinAnchor, Vector2 targetMaxAnchor, bool isKeepPosition = true) {
    var parent = rectTransform.parent as RectTransform;
    if (parent == null) {
      return;
    }

    var diffMin = targetMinAnchor - rectTransform.anchorMin;
    var diffMax = targetMaxAnchor - rectTransform.anchorMax;
    // anchorの更新
    rectTransform.anchorMin = targetMinAnchor;
    rectTransform.anchorMax = targetMaxAnchor;

    if (!isKeepPosition) return;

    // 上下左右の距離の差分を計算
    var rect = parent.rect;
    float diffLeft = rect.width * diffMin.x;
    float diffRight = rect.width * diffMax.x;
    float diffBottom = rect.height * diffMin.y;
    float diffTop = rect.height * diffMax.y;
    // サイズと座標の修正
    rectTransform.sizeDelta += new Vector2(diffLeft - diffRight, diffBottom - diffTop);
    var pivot = rectTransform.pivot;
    rectTransform.anchoredPosition -= new Vector2(
      (diffLeft * (1 - pivot.x)) + (diffRight * pivot.x),
      (diffBottom * (1 - pivot.y)) + (diffTop * pivot.y)
    );
  }

  /// <summary>
  /// Anchorを変更する
  /// </summary>
  public static void SetAnchor(this RectTransform rectTransform, Vector2 targetAnchor, bool isKeepPosition = true) {
    SetAnchor(rectTransform, targetAnchor, targetAnchor, isKeepPosition);
  }

  /// <summary>
  /// RectTransformのWorldRectを取得
  /// </summary>
  public static Rect GetWorldRect(this RectTransform rectTransform) {
    var worldPosition = rectTransform.position.ToVector2();
    var screenRect = rectTransform.rect;
    var lossyScale = rectTransform.lossyScale;
    var worldOffset = screenRect.position * lossyScale;
    var worldSize = screenRect.size * lossyScale;
    return new Rect(worldPosition + worldOffset, worldSize);
  }

  /// <summary>
  /// 相対座標移動
  /// </summary>
  public static void MovePositionRelative(this Rigidbody2D rigidbody2D, Vector2 moveVec) {
    rigidbody2D.MovePosition(rigidbody2D.position + moveVec);
  }

  /// <summary>
  /// Rectの範囲を均等に広げる
  /// </summary>
  public static void Expand(this ref Rect rect, float value) {
    rect.xMin -= value;
    rect.yMin -= value;
    rect.xMax += value;
    rect.yMax += value;
  }

  /// <summary>
  /// RectIntの範囲を均等に広げる
  /// </summary>
  public static void Expand(this ref RectInt rect, int value) {
    rect.xMin -= value;
    rect.yMin -= value;
    rect.xMax += value;
    rect.yMax += value;
  }

  /// <summary>
  /// Rectの範囲をotherの分だけ広げる
  /// </summary>
  public static Rect Add(this Rect rect, Rect other) {
    if (other.xMin < rect.xMin)
      rect.xMin = other.xMin;
    if (other.xMax > rect.xMax)
      rect.xMax = other.xMax;
    if (other.yMin < rect.yMin)
      rect.yMin = other.yMin;
    if (other.yMax > rect.yMax)
      rect.yMax = other.yMax;
    return rect;
  }

  /// <summary>
  /// float -> Vector2の変換
  /// </summary>
  public static Vector2 ToVector2(this float value) {
    return new Vector2(value, value);
  }

  /// <summary>
  /// float -> Vector3の変換
  /// </summary>
  public static Vector3 ToVector3(this float value) {
    return new Vector3(value, value, value);
  }

  /// <summary>
  /// Vector3Int -> Vector2Intの変換
  /// </summary>
  public static Vector2Int ToVector2Int(this Vector3Int v) {
    return new Vector2Int(v.x, v.y);
  }

  /// <summary>
  /// Vector3Int -> Vector2Intの変換
  /// XとZの値を使用する
  /// </summary>
  public static Vector2Int ToVector2IntXZ(this Vector3Int v) {
    return new Vector2Int(v.x, v.z);
  }

  /// <summary>
  /// Vector2Int -> Vector3Intの変換
  /// </summary>
  public static Vector3Int ToVector3Int(this Vector2Int v, int z = 0) {
    return new Vector3Int(v.x, v.y, z);
  }

  /// <summary>
  /// Vector3 -> Vector2の変換
  /// </summary>
  public static Vector2 ToVector2(this Vector3 v) {
    return new Vector2(v.x, v.y);
  }

  /// <summary>
  /// Vector3 -> Vector2の変換
  /// XとZの値を使用する
  /// </summary>
  public static Vector2 ToVector2XZ(this Vector3 v) {
    return new Vector2(v.x, v.z);
  }

  /// <summary>
  /// Vector2 -> Vector3の変換
  /// </summary>
  public static Vector3 ToVector3(this Vector2 v, float z = 0.0f) {
    return new Vector3(v.x, v.y, z);
  }

  /// <summary>
  /// Vector2 -> Vector2Intの変換（切り捨て）
  /// </summary>
  public static Vector2Int ToVector2Int(this Vector2 v) {
    return new Vector2Int((int) v.x, (int) v.y);
  }

  /// <summary>
  /// Vector2Int -> Vector2の変換
  /// </summary>
  public static Vector2 ToVector2(this Vector2Int v) {
    return new Vector2(v.x, v.y);
  }

  /// <summary>
  /// Vector3 -> Vector3Intの変換（切り捨て）
  /// </summary>
  public static Vector3Int ToVector3Int(this Vector3 v) {
    return new Vector3Int((int) v.x, (int) v.y, (int) v.z);
  }

  /// <summary>
  /// Vector3Int -> Vector3の変換
  /// </summary>
  public static Vector3 ToVector3(this Vector3Int v) {
    return new Vector3(v.x, v.y);
  }

  /// <summary>
  /// Vector3 -> Colorの変換
  /// </summary>
  public static Color ToColor(this Vector3 v, float a = 1f) {
    return new Color(v.x, v.y, v.z, a);
  }

  /// <summary>
  /// Color -> Vector3の変換
  /// </summary>
  public static Vector3 ToVector3(this Color c) {
    return new Vector3(c.r, c.g, c.b);
  }

  /// <summary>
  /// Vector4 -> Colorの変換
  /// </summary>
  /// <param name="v"></param>
  /// <returns></returns>
  public static Color ToColor(this Vector4 v) {
    return new Color(v.x, v.y, v.z, v.w);
  }

  /// <summary>
  /// Color -> Vector4の変換
  /// </summary>
  /// <param name="c"></param>
  /// <returns></returns>
  public static Vector4 ToVector4(this Color c) {
    return new Vector4(c.r, c.g, c.b, c.a);
  }

  /// <summary>
  /// Y座標を指定数上げる
  /// </summary>
  public static Vector2Int Up(this ref Vector2Int vec, int value = 1) {
    return vec += new Vector2Int(0, value);
  }

  /// <summary>
  /// Y座標を指定数下げる
  /// </summary>
  public static Vector2Int Down(this ref Vector2Int vec, int value = 1) {
    return vec += new Vector2Int(0, -value);
  }

  /// <summary>
  /// X座標を指定数上げる
  /// </summary>
  public static Vector2Int Right(this ref Vector2Int vec, int value = 1) {
    return vec += new Vector2Int(value, 0);
  }

  /// <summary>
  /// X座標を指定数下げる
  /// </summary>
  public static Vector2Int Left(this ref Vector2Int vec, int value = 1) {
    return vec += new Vector2Int(-value, 0);
  }

  /// <summary>
  /// ほぼ等しいか
  /// </summary>
  public static bool Approximately(this Vector2 a, Vector2 b) {
    return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
  }

  /// <summary>
  /// ほぼ等しいか
  /// </summary>
  public static bool Approximately(this Vector3 a, Vector3 b) {
    return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
  }

  /// <summary>
  /// 距離を取得
  /// </summary>
  public static float ToLength(this Vector2 vec1, Vector2 vec2) {
    return (vec1 - vec2).magnitude;
  }

  /// <summary>
  /// 距離を取得
  /// </summary>
  public static float ToLength(this Vector3 vec1, Vector3 vec2) {
    return (vec1 - vec2).magnitude;
  }

  /// <summary>
  /// 距離の二乗を取得
  /// こっちの方が軽い
  /// </summary>
  public static float ToSqrLength(this Vector2 vec1, Vector2 vec2) {
    return (vec1 - vec2).sqrMagnitude;
  }

  /// <summary>
  /// 距離の二乗を取得
  /// こっちの方が軽い
  /// </summary>
  public static float ToSqrLength(this Vector3 vec1, Vector3 vec2) {
    return (vec1 - vec2).sqrMagnitude;
  }

  /// <summary>
  /// 距離の二乗を取得
  /// こっちの方が軽い
  /// </summary>
  public static float ToSqrLength(this Vector2Int vec1, Vector2Int vec2) {
    return (vec1 - vec2).sqrMagnitude;
  }

  /// <summary>
  /// 距離の二乗を取得
  /// こっちの方が軽い
  /// </summary>
  public static float ToSqrLength(this Vector3Int vec1, Vector3Int vec2) {
    return (vec1 - vec2).sqrMagnitude;
  }

  /// <summary>
  /// float値を同じ値でColorに変換
  /// </summary>
  public static Color ToColor(this float value) {
    return new Color(value, value, value);
  }

  /// <summary>
  /// ColorからColor32に変換
  /// </summary>
  public static Color32 ToColor32(this Color color) {
    return new Color32((byte) (color.r * 255), (byte) (color.g * 255), (byte) (color.b * 255), (byte) (color.a * 255));
  }

  /// <summary>
  /// Color32からColorに変換
  /// </summary>
  public static Color ToColor(this Color32 color) {
    return new Color(color.r / 255f, color.g * 255f, color.b * 255f, color.a * 255f);
  }

  /// <summary>
  /// Colorの設定
  /// </summary>
  public static void SetColor(this ref Color color, float value, bool withAlpha = false) {
    color.r = Mathf.Clamp01(value);
    color.g = Mathf.Clamp01(value);
    color.b = Mathf.Clamp01(value);
    if (withAlpha) color.a = Mathf.Clamp01(value);
  }

  /// <summary>
  /// Colorの加算
  /// </summary>
  public static void AddColor(this ref Color color, float value, bool withAlpha = false) {
    color.r = Mathf.Clamp01(color.r + value);
    color.g = Mathf.Clamp01(color.g + value);
    color.b = Mathf.Clamp01(color.b + value);
    if (withAlpha) color.a = Mathf.Clamp01(color.a + value);
  }

  /// <summary>
  /// Colorのスクリーン乗算
  /// </summary>
  public static Color MultiplyColorScreen(Color color, float value, bool withAlpha = false) {
    color.r -= (1f - color.r) * value;
    color.g -= (1f - color.g) * value;
    color.b -= (1f - color.b) * value;
    if (withAlpha) color.a -= (1f - color.a) * value;
    return color;
  }

  /// <summary>
  /// HSLからColorに変換
  /// </summary>
  public static Color FromHSL(float h, float s, float l, float a = 1f) {
    float r = l;
    float g = l;
    float b = l;
    if (s != 0) {
      float v2 = (l < 0.5f) ? l * (1 + s) : (l + s) - (l * s);
      float v1 = 2 * l - v2;

      r = HueToRGB(v1, v2, h + 1f / 3f);
      g = HueToRGB(v1, v2, h);
      b = HueToRGB(v1, v2, h - 1f / 3f);
    }
    return new Color(r, g, b, a);
  }

  private static float HueToRGB(float v1, float v2, float vH) {
    if (vH < 0)
      vH += 1;
    if (vH > 1)
      vH -= 1;
    if ((6 * vH) < 1)
      return (v1 + (v2 - v1) * 6 * vH);
    if ((2 * vH) < 1)
      return (v2);
    if ((3 * vH) < 2)
      return (v1 + (v2 - v1) * ((2f / 3f) - vH) * 6);
    return (v1);
  }

  /// <summary>
  /// アルファ値の設定
  /// </summary>
  public static void SetAlpha(this Graphic graphic, float alpha) {
    var color = graphic.color;
    color.a = alpha;
    graphic.color = color;
  }

  /// <summary>
  /// アルファ値の設定
  /// </summary>
  public static void SetAlpha(this SpriteRenderer spriteRenderer, float alpha) {
    var color = spriteRenderer.color;
    color.a = alpha;
    spriteRenderer.color = color;
  }

  /// <summary>
  /// GridLayoutGroupの開始要素数を計算
  /// </summary>
  public static int CalcGridAxisCount(this GridLayoutGroup gridLayoutGroup) {
    var sizeDelta = gridLayoutGroup.transform.AsRectTransform().sizeDelta;
    var cellSize = gridLayoutGroup.cellSize;
    var spacing = gridLayoutGroup.spacing;
    return gridLayoutGroup.startAxis switch {
      GridLayoutGroup.Axis.Horizontal => Mathf.RoundToInt((sizeDelta.x + spacing.x) / (cellSize.x + spacing.x)),
      GridLayoutGroup.Axis.Vertical => Mathf.RoundToInt((sizeDelta.y + spacing.y) / (cellSize.y + spacing.y)),
      _ => 0
    };
  }

  /// <summary>
  /// Navigationを設定
  /// </summary>
  public static void EnableNavigation(this Selectable selectable, Navigation.Mode navigationMode) {
    var navigation = selectable.navigation;
    navigation.mode = navigationMode;
    selectable.navigation = navigation;
  }

  /// <summary>
  /// Navigationを無効化
  /// </summary>
  public static void DisableNavigation(this Selectable selectable) {
    var navigation = selectable.navigation;
    navigation.mode = Navigation.Mode.None;
    selectable.navigation = navigation;
  }
}

}