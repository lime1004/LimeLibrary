namespace LimeLibrary.Sound {

/// <summary>
/// サウンド再生時の挙動
/// </summary>
public enum SoundPlayType {
  /// <summary>
  /// 同じIDのサウンドが鳴っていても通常通り再生する
  /// </summary>
  Default,
  /// <summary>
  /// 同じIDのサウンドが鳴っていたら再生中のものは停止する
  /// </summary>
  Stop,
  /// <summary>
  /// 同じIDのサウンドが鳴っていたら同じサウンドを再度Playする
  /// </summary>
  Reuse,
}

}