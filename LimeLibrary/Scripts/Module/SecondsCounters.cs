using System.Collections.Generic;
using LimeLibrary.Extensions;
using LimeLibrary.Utility;
using UnityEngine;

namespace LimeLibrary.Module {

public class SecondsCounter {
  private float _secondsCounter;

  public float Get() => _secondsCounter;
  public void Reset() => _secondsCounter = 0f;
  public void AddDeltaTime() => _secondsCounter += Time.deltaTime;
}

public class SecondsCounters {
  private readonly List<SecondsCounter> _secondsCounterList = new(8);

  public void Alloc(int num) {
    _secondsCounterList.Clear();
    for (int i = 0; i < num; i++) {
      Add();
    }
  }

  public void Add() {
    _secondsCounterList.Add(new SecondsCounter());
  }

  private void AddUntilIndex(int index) {
    int currentCount = _secondsCounterList.Count;
    for (int i = 0; i <= index - currentCount; i++) {
      Add();
    }
  }

  public float Get(int index) {
    if (!_secondsCounterList.IsDefinedAt(index)) {
      AddUntilIndex(index);
    }
    return _secondsCounterList[index].Get();
  }

  public SecondsCounter GetSecondsCounter(int index) {
    if (!_secondsCounterList.IsDefinedAt(index)) {
      AddUntilIndex(index);
    }
    return _secondsCounterList[index];
  }

  public void Reset(int index) {
    if (!_secondsCounterList.IsDefinedAt(index)) {
      Assertion.Assert(false, index);
    }
    _secondsCounterList[index].Reset();
  }

  public void ResetAll() {
    for (int i = 0; i < _secondsCounterList.Count; i++) {
      _secondsCounterList[i].Reset();
    }
  }

  public void OnUpdate() {
    for (int i = 0; i < _secondsCounterList.Count; i++) {
      _secondsCounterList[i].AddDeltaTime();
    }
  }
}

}