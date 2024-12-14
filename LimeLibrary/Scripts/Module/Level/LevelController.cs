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

  public LevelController(LevelTable levelTable, int minLevel, int maxLevel, int initialExperience, int? initialLevel = 0) {
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

  public int GetExperienceUntilNextLevel() {
    return Experience - _levelTable.CalculateTotalExperience(_level);
  }

  public int GetRequiredExperienceUntilNextLevel() {
    return _levelTable.GetNextRequiredExperience(_level);
  }

  public float GetExperienceRateUntilNextLevel() {
    int nextRequiredExperience = _levelTable.GetNextRequiredExperience(_level);
    int totalRequiredExperience = _levelTable.CalculateTotalExperience(_level);
    int currentExperience = _experience - totalRequiredExperience;
    return 1f - ((float) currentExperience / nextRequiredExperience);
  }
}

}