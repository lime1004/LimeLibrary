using LimeLibrary.Extensions;
using UnityEngine;
using UnityEngine.AI;

namespace LimeLibrary.Utility {

public static class NavMeshUtility {
  public static Vector2 CorrectPositionFromNavMesh(Vector2 position, int areaMask = NavMesh.AllAreas) {
    return NavMesh.SamplePosition(position, out var hit, 100f, areaMask) ? hit.position : position;
  }

  public static bool ExistNavMesh(Vector2 position, int areaMask = NavMesh.AllAreas) {
    return NavMesh.SamplePosition(position, out var hit, 0.1f, areaMask);
  }

  public static bool IsCanReachPath(Vector2 nowPosition, Vector2 targetPosition, int areaMask = NavMesh.AllAreas) {
    var path = new NavMeshPath();
    NavMesh.CalculatePath(nowPosition, targetPosition, areaMask, path);

    if (path.corners.Length == 0) return false;

    // パスの到達地点がゴール付近であれば、到達するルートがある
    var length = path.corners[^1] - targetPosition.ToVector3();
    return length.magnitude < 0.1f;
  }
}

}