using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LimeLibrary.Module.Level {

[CreateAssetMenu(
  fileName = "LevelTable",
  menuName = "LimeLibrary/Module/LevelTable")]
public class LevelTable : DictionaryScriptableObject<int, int> {
  public int GetNextRequiredExperience(int currentLevel) {
    if (currentLevel <= 0) return 0;
    if (Exists(currentLevel)) return Get(currentLevel);
    return GetNextRequiredExperience(currentLevel - 1);
  }

  public int CalculateLevel(int totalExperience, int maxLevel) {
    int level = 1;
    int totalRequiredExperience = 0;
    for (int i = 1; i <= maxLevel; i++) {
      int requiredExperience = GetNextRequiredExperience(i);
      totalRequiredExperience += requiredExperience;
      if (totalExperience < totalRequiredExperience) break;

      level++;
    }
    return level;
  }

  public int CalculateTotalExperience(int levelInclusive) {
    int totalExperience = 0;
    for (int i = 1; i <= levelInclusive - 1; i++) {
      totalExperience += GetNextRequiredExperience(i);
    }
    return totalExperience;
  }

#if LIME_ODIN_INSPECTOR
  [Button]
  private void FromSpreadSheet() {
    // クリップボードのデータを取得
    string clipboardData = GUIUtility.systemCopyBuffer;

    var rows = ParseSpreadSheetData(clipboardData);
    foreach (string[] row in rows) {
      if (row.Length < 2) continue;
      if (int.TryParse(row[0], out int level) && int.TryParse(row[1], out int requiredExperience)) {
        _dictionary[level] = requiredExperience;
      }
    }
  }

  private static List<string[]> ParseSpreadSheetData(string data) {
    var rows = new List<string[]>();
    string[] lines = data.Split('\n');

    foreach (string line in lines) {
      string[] cells = line.Split('\t');
      rows.Add(cells);
    }

    return rows;
  }
#endif
}

}