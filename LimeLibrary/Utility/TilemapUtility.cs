using UnityEngine;
using UnityEngine.Tilemaps;

namespace LimeLibrary.Utility {

public static class TilemapUtility {
  /// <summary>
  /// ワールド座標からタイル座標を取得
  /// </summary>
  public static Vector3Int GetTilePos(Vector2 worldPos, int tileZ = 0, float scale = 1f) {
    float xx = 0.5f * scale;
    float yx = 0.25f * scale;
    float xy = -0.5f * scale;
    float yy = 0.25f * scale;

    float xi = ((yy * worldPos.x - xy * worldPos.y) / (xx * yy - xy * yx));
    float yi = ((yx * worldPos.x - xx * worldPos.y) / -(xx * yy - xy * yx));

    return new Vector3Int(Mathf.FloorToInt(xi) - Mathf.CeilToInt(tileZ / 2.0f), Mathf.FloorToInt(yi) - Mathf.CeilToInt(tileZ / 2.0f), tileZ);
  }

  /// <summary>
  /// ワールド座標からタイル座標を取得
  /// 小数点を切り捨てずに返す
  /// </summary>
  public static Vector3 GetTilePosFloat(Vector2 worldPos, int tileZ = 0, float scale = 1f) {
    float xx = 0.5f * scale;
    float yx = 0.25f * scale;
    float xy = -0.5f * scale;
    float yy = 0.25f * scale;

    float xi = ((yy * worldPos.x - xy * worldPos.y) / (xx * yy - xy * yx));
    float yi = ((yx * worldPos.x - xx * worldPos.y) / -(xx * yy - xy * yx));

    return new Vector3(xi - tileZ / 2.0f, yi - tileZ / 2.0f, tileZ);
  }

  /// <summary>
  /// タイル座標からワールド座標を取得
  /// </summary>
  /// <param name="tilePos">タイル座標</param>
  /// <param name="scale"></param>
  /// <returns>ワールド座標</returns>
  public static Vector2 GetWorldPos(Vector3Int tilePos, float scale = 1f) {
    float xx = 0.5f * scale;
    float yx = 0.25f * scale;
    float xy = -0.5f * scale;
    float yy = 0.25f * scale;

    float xs = tilePos.x * xx + tilePos.y * xy;
    float ys = tilePos.x * yx + tilePos.y * yy;

    float yz = tilePos.z * 0.5f * scale;

    return new Vector2(xs, ys + yz);
  }
}

}