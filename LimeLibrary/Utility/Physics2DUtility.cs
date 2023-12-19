using UnityEngine;

namespace LimeLibrary.Utility {

public static class Physics2DUtility {
  private const float RayDisplayTime = 3;

  public static RaycastHit2D RaycastAndDraw(Vector2 origin, Vector2 direction, float maxDistance, int layerMask) {
    var hit = Physics2D.Raycast(origin, direction, maxDistance, layerMask);

    if (hit.collider) {
      Debug.DrawRay(origin, hit.point - origin, Color.blue, RayDisplayTime, false);
    } else {
      Debug.DrawRay(origin, direction * maxDistance, Color.green, RayDisplayTime, false);
    }

    return hit;
  }
}

}