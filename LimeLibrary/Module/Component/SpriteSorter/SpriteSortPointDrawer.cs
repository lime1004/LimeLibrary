using LimeLibrary.Extensions;
using UnityEngine;

namespace LimeLibrary.Module {

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSortPointDrawer : MonoBehaviour {

  protected void OnDrawGizmosSelected() {
    var worldPosition = transform.position;

    var spriteRenderer = GetComponent<SpriteRenderer>();
    var bounds = spriteRenderer.bounds;

    float z = worldPosition.y + worldPosition.z * -0.26f;
    var centerPos = worldPosition.ToVector2();
    const float Length = 0.2f;
    Debug.DrawLine(centerPos + new Vector2(-Length, Length), centerPos + new Vector2(Length, -Length), Color.red);
    Debug.DrawLine(centerPos + new Vector2(Length, Length), centerPos + new Vector2(-Length, -Length), Color.red);

    var s = new Vector3(centerPos.x - bounds.size.x / 2, z, worldPosition.z);
    var e = new Vector3(centerPos.x + bounds.size.x / 2, z, worldPosition.z);
    Gizmos.color = Color.green;
    Gizmos.DrawLine(s, e);
  }
}

}