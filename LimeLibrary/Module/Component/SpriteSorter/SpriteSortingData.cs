using System.Collections.Generic;
using LimeLibrary.Extensions;
using LimeLibrary.Utility;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LimeLibrary.Module {

public readonly struct SortingLine {
  public SortingLine(Vector2 startPoint, Vector2 endPoint, Vector2 worldPosition) {
    StartPoint = startPoint + worldPosition;
    EndPoint = endPoint + worldPosition;
  }

  public Vector2 StartPoint { get; }
  public Vector2 EndPoint { get; }
  public float CenterHeight => (StartPoint.y + EndPoint.y) / 2;
}

[DisallowMultipleComponent]
public class SpriteSortingData : MonoBehaviour {
  [SerializeField]
  private SpriteSortType _sortType = SpriteSortType.Point;
  [SerializeField]
  private Vector2 _sorterPositionOffset1;
  [SerializeField]
  private Vector2 _sorterPositionOffset2;
  [SerializeField]
  private SpriteSortTarget _spriteSortTarget;
  [SerializeField]
  private bool _isTopPriority;

  private readonly List<SpriteSortingData> _dependencies = new List<SpriteSortingData>();

  private SpriteRenderer _spriteRenderer;
  private SortingGroup _sortingGroup;
  private bool _enabled = true;
  private Transform Transform { get; set; }

  public SpriteSortType SortType => _sortType;
  public Vector2 SortingPoint => _sorterPositionOffset1 + Transform.position.ToVector2();
  public SortingLine SortingLine => new SortingLine(_sorterPositionOffset1, _sorterPositionOffset2, Transform.position);
  public Bounds SpriteBounds => _spriteRenderer != null ? _spriteRenderer.bounds : new Bounds();
  public List<SpriteSortingData> Dependencies => _dependencies;

  private void Awake() {
    Transform = transform;
    _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    _sortingGroup = GetComponent<SortingGroup>();

    if (_spriteRenderer == null && _sortingGroup == null) {
      Assertion.Assert(false);
    }
  }

  public void SetEnabled(bool sortEnabled) {
    _enabled = sortEnabled;
  }

  private void OnEnable() {
    if (SpriteSorter.Attached && _enabled) {
      SpriteSorter.Instance.RegisterSpriteSortingData(this, _isTopPriority);
    }
  }

  private void OnDisable() {
    if (SpriteSorter.Attached) {
      SpriteSorter.Instance.UnregisterSpriteSortingData(this);
      SetSortingOrder(0);
    }
  }

  internal void SetSortingOrder(int order) {
    switch (_spriteSortTarget) {
    case SpriteSortTarget.SpriteRenderer:
      if (_spriteRenderer) _spriteRenderer.sortingOrder = order;
      break;
    case SpriteSortTarget.SortingGroup:
      if (_sortingGroup) _sortingGroup.sortingOrder = order;
      break;
    default:
      Assertion.Assert(false);
      break;
    }
  }

#if UNITY_EDITOR
  [CustomEditor(typeof(SpriteSortingData))]
  public class SpriteSorterEditor : Editor {
    public void OnSceneGUI() {
      var spriteSorter = (SpriteSortingData) target;

      var position = spriteSorter.transform.position;
      spriteSorter._sorterPositionOffset1 = Handles.FreeMoveHandle(
        position + spriteSorter._sorterPositionOffset1.ToVector3(),
        Quaternion.identity, 0.08f * HandleUtility.GetHandleSize(position),
        Vector3.zero, Handles.DotHandleCap) - position;
      if (spriteSorter._sortType == SpriteSortType.Line) {
        spriteSorter._sorterPositionOffset2 = Handles.FreeMoveHandle(
          position + spriteSorter._sorterPositionOffset2.ToVector3(), Quaternion.identity,
          0.08f * HandleUtility.GetHandleSize(position), Vector3.zero, Handles.DotHandleCap) - position;
        Handles.DrawLine(position + spriteSorter._sorterPositionOffset1.ToVector3(),
          position + spriteSorter._sorterPositionOffset2.ToVector3());
      }

      if (GUI.changed) {
        Undo.RecordObject(target, "Updated Sorting Offset");
        EditorUtility.SetDirty(target);
      }
    }
  }
#endif
}

}