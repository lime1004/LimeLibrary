using System.Collections.Generic;
using UnityEngine;

namespace LimeLibrary.Module {

/// <summary>
/// Spriteのソートを行うComponent
/// </summary>
public class SpriteSorter : SingletonMonoBehaviour<SpriteSorter> {
  [SerializeField]
  private int _dataListCapacity = 256;

  private List<SpriteSortingData> _spriteSortingDataList;
  private List<SpriteSortingData> _sortSpriteSortingDataList;

  protected override void Awake() {
    _spriteSortingDataList = new List<SpriteSortingData>(_dataListCapacity);
    _sortSpriteSortingDataList = new List<SpriteSortingData>(_dataListCapacity);
  }

  public void RegisterSpriteSortingData(SpriteSortingData spriteSortingData, bool isInsertTop) {
    if (_spriteSortingDataList.Contains(spriteSortingData)) return;
    if (isInsertTop) {
      _spriteSortingDataList.Insert(0, spriteSortingData);
    } else {
      _spriteSortingDataList.Add(spriteSortingData);
    }
  }

  public void UnregisterSpriteSortingData(SpriteSortingData spriteSortingData) {
    if (!_spriteSortingDataList.Contains(spriteSortingData)) return;
    _spriteSortingDataList.Remove(spriteSortingData);
  }

  private void Update() {
    foreach (var spriteSortingData in _spriteSortingDataList) {
      spriteSortingData.SetSortingOrder(0);
    }

    if (_spriteSortingDataList.Count <= 1) return;

    _sortSpriteSortingDataList.Clear();

    foreach (var spriteSortingData in _spriteSortingDataList) {
      _sortSpriteSortingDataList.Add(spriteSortingData);
    }
    // NOTE: GCAlloc回避のためメソッドグループを使わない
    _sortSpriteSortingDataList.Sort((data1, data2) => CompareSpriteOrder(data1, data2));
    int orderInLayer = 0;
    foreach (var spriteSortingData in _sortSpriteSortingDataList) {
      spriteSortingData.SetSortingOrder(orderInLayer++);
    }
  }

  private static int CompareSpriteOrder(SpriteSortingData spriteSortingData1, SpriteSortingData spriteSortingData2) {
    return spriteSortingData1.SortType switch {
      SpriteSortType.Point when spriteSortingData2.SortType == SpriteSortType.Point => spriteSortingData2.SortingPoint.y.CompareTo(spriteSortingData1.SortingPoint.y),
      SpriteSortType.Line when spriteSortingData2.SortType == SpriteSortType.Line => CompareLineAndLine(spriteSortingData1.SortingLine, spriteSortingData2.SortingLine),
      SpriteSortType.Point when spriteSortingData2.SortType == SpriteSortType.Line => ComparePointAndLine(spriteSortingData1.SortingPoint, spriteSortingData2.SortingLine),
      SpriteSortType.Line when spriteSortingData2.SortType == SpriteSortType.Point => -ComparePointAndLine(spriteSortingData2.SortingPoint, spriteSortingData1.SortingLine),
      _ => 0
    };
  }

  private static int CompareLineAndLine(SortingLine line1, SortingLine line2) {
    var line1Point1 = line1.StartPoint;
    var line1Point2 = line1.EndPoint;
    var line2Point1 = line2.StartPoint;
    var line2Point2 = line2.EndPoint;

    int comp1 = ComparePointAndLine(line1Point1, line2);
    int comp2 = ComparePointAndLine(line1Point2, line2);
    int oneVsTwo = int.MinValue;
    if (comp1 == comp2) {
      oneVsTwo = comp1;
    }

    int comp3 = ComparePointAndLine(line2Point1, line1);
    int comp4 = ComparePointAndLine(line2Point2, line1);
    int twoVsOne = int.MinValue;
    if (comp3 == comp4) {
      twoVsOne = -comp3;
    }

    if (oneVsTwo != int.MinValue && twoVsOne != int.MinValue) {
      return oneVsTwo == twoVsOne ? oneVsTwo : CompareLineCenters(line1, line2);
    } else if (oneVsTwo != int.MinValue) {
      return oneVsTwo;
    } else if (twoVsOne != int.MinValue) {
      return twoVsOne;
    } else {
      return CompareLineCenters(line1, line2);
    }
  }

  private static int CompareLineCenters(SortingLine line1, SortingLine line2) {
    return -line1.CenterHeight.CompareTo(line2.CenterHeight);
  }

  private static int ComparePointAndLine(Vector3 point, SortingLine line) {
    float pointY = point.y;
    if (pointY > line.StartPoint.y && pointY > line.EndPoint.y) {
      return -1;
    } else if (pointY < line.StartPoint.y && pointY < line.EndPoint.y) {
      return 1;
    } else {
      float slope = (line.EndPoint.y - line.StartPoint.y) / (line.EndPoint.x - line.StartPoint.x);
      float intercept = line.StartPoint.y - (slope * line.StartPoint.x);
      float yOnLineForPoint = (slope * point.x) + intercept;
      return yOnLineForPoint > point.y ? 1 : -1;
    }
  }
}

}