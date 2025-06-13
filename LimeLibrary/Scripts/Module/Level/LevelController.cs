using UnityEngine;

namespace LimeLibrary.Module {

public class LevelController {
  private readonly LevelTable _levelTable;

  private readonly int _minLevel;
  private readonly int _maxLevel;
  private int _level;
  private int _experience;

  public int Level => _level;
  public int MinLevel => _minLevel;
  public int MaxLevel => _maxLevel;
  public int Experience => _experience;

  public LevelController(LevelTable levelTable, int minLevel, int maxLevel, int initialExperience, int? initialLevel = null) {
    _levelTable = levelTable;
    _minLevel = minLevel;
    _maxLevel = maxLevel;
    _experience = initialExperience;
    UpdateLevel();
    if (initialLevel.HasValue) {
      SetLevel(initialLevel.Value);
    }
  }

  public bool IsMaxLevel() {
    return _level == _maxLevel;
  }

  public bool IsMinLevel() {
    return _level == _minLevel;
  }

  public ChangeExperienceResult AddExperience(int value) {
    _experience = Mathf.Max(_experience + value, 0);
    return UpdateLevel();
  }

  public ChangeExperienceResult SubExperience(int value) {
    _experience = Mathf.Max(_experience - value, 0);
    return UpdateLevel();
  }

  public ChangeExperienceResult SetExperience(int value) {
    _experience = Mathf.Max(value, 0);
    return UpdateLevel();
  }

  private ChangeExperienceResult UpdateLevel() {
    int level = _levelTable.CalculateLevel(_experience, _maxLevel);

    ChangeExperienceResult result;
    if (level > _level) {
      result = ChangeExperienceResult.LevelUp;
    } else if (level < _level) {
      result = ChangeExperienceResult.LevelDown;
    } else {
      result = ChangeExperienceResult.None;
    }

    SetLevelInternal(level);

    return result;
  }

  public void AddLevel(int value) {
    int nextLevel = Mathf.Min(_level + value, _maxLevel);
    int totalExperienceUntilNextLevel = _levelTable.CalculateTotalExperience(nextLevel);
    int addExperience = totalExperienceUntilNextLevel - _experience;
    AddExperience(addExperience);
  }

  public void SubLevel(int value) {
    int nextLevel = Mathf.Max(_level - value, _minLevel);
    int totalExperienceThisNextLevel = _levelTable.CalculateTotalExperience(nextLevel);
    int subExperience = _experience - totalExperienceThisNextLevel;
    SubExperience(subExperience);
  }

  public void SetLevel(int level) {
    if (level < _level) {
      SubLevel(_level - level);
    } else if (level > _level) {
      AddLevel(level - _level);
    }
  }

  private void SetLevelInternal(int level) {
    if (level == _level) return;

    _level = Mathf.Clamp(level, _minLevel, _maxLevel);
  }

  /// <summary>
  /// 現在の経験値（差分）
  /// 現在のレベルに必要な経験値を0とした場合の現在の経験値
  /// </summary>
  public int GetCurrentExperienceDiff() {
    int totalExperienceForCurrentLevel = _levelTable.CalculateTotalExperience(_level);
    return _experience - totalExperienceForCurrentLevel;
  }

  /// <summary>
  /// 次のレベルに必要な累計経験値
  /// </summary>
  public int GetTotalExperienceForNextLevel() {
    return _levelTable.CalculateTotalExperience(_level + 1);
  }

  /// <summary>
  /// 次のレベルに必要な差分経験値（差分、現在の経験値は計算に入れない）
  /// 現在のレベルから次のレベルへの差分経験値
  /// </summary>
  public int GetDiffExperienceForNextLevel() {
    if (IsMaxLevel()) return 0;
    return _levelTable.GetNextRequiredExperience(_level);
  }

  /// <summary>
  /// 次のレベルに必要な残り経験値（差分、現在の経験値から算出）
  /// </summary>
  public int GetRemainingExperienceForNextLevel() {
    if (IsMaxLevel()) return 0;
    int diffExperience = GetDiffExperienceForNextLevel();
    int currentDiffExperience = GetCurrentExperienceDiff();
    return diffExperience - currentDiffExperience;
  }

  /// <summary>
  /// 現在の経験値率（差分）
  /// 現在の差分経験値 / 次のレベルに必要な差分経験値
  /// </summary>
  public float GetCurrentExperienceRate() {
    if (IsMaxLevel()) return 1f;
    int diffExperience = GetDiffExperienceForNextLevel();
    if (diffExperience == 0) return 0f;

    int currentDiffExperience = GetCurrentExperienceDiff();
    return (float) currentDiffExperience / diffExperience;
  }

  /// <summary>
  /// 次のレベルに必要な残り経験値率（差分、現在の経験値から算出）
  /// 次のレベルに必要な残り経験値 / 次のレベルに必要な差分経験値
  /// </summary>
  public float GetRemainingExperienceRate() {
    if (IsMaxLevel()) return 0f;
    return 1f - GetCurrentExperienceRate();
  }
}

}