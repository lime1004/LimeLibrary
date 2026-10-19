namespace LimeLibrary.Module {

public enum SubLevelExperienceType {
  // 下がった先のレベルの開始経験値にする
  LevelStart,
  // 経験値を増やさない範囲で、下がった先のレベルの次まで残り1にする
  BeforeNextLevel,
}

}