namespace LimeLibrary.UI {

public enum UIAppAwakeType {
  /// <summary>
  /// すでに起動済みのAppと平行して起動する
  /// </summary>
  Parallel,
  /// <summary>
  /// すでに起動済みのAppをHideして起動する
  /// 起動したAppがDestroyされればもとのAppをShowする
  /// </summary>
  Stack,
  /// <summary>
  /// ゲーム中常に表示されうるように起動する
  /// 他Appによって非表示になったりしない
  /// </summary>
  Resident,
}

}