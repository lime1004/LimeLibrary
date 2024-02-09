#if LIME_UNITASK
namespace LimeLibrary.Sound {

public class SoundLayerData {
  public SoundData SoundData { get; }
  public SoundLayerState LayerState { get; set; } = SoundLayerState.Stop;

  public SoundLayerData(SoundData soundData) {
    SoundData = soundData;
  }
}

}
#endif