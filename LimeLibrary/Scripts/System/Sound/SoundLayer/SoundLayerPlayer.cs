#if LIME_UNITASK
using System;
using System.Collections.Generic;

namespace LimeLibrary.Sound {

public class SoundLayerPlayer<T> where T : struct, Enum {
  private readonly Dictionary<T, SoundLayerData> _layerSoundDictionary = new();

  public void Play(T layer, SoundData soundData, float fadeDuration = 0f) {
    var prevSameLayerSoundLayerData = GetSoundLayerData(layer);
    if (prevSameLayerSoundLayerData != null && prevSameLayerSoundLayerData.SoundData != soundData) {
      prevSameLayerSoundLayerData.SoundData.FadeDuration = fadeDuration;
      prevSameLayerSoundLayerData.SoundData.Stop();
    }

    soundData.FadeDuration = fadeDuration;
    soundData.Play();

    var newSoundLayerData = new SoundLayerData(soundData);
    newSoundLayerData.LayerState = SoundLayerState.Play;
    _layerSoundDictionary[layer] = newSoundLayerData;

    UpdateLayer();
  }

  public void Pause(T layer, float fadeDuration = 0f) {
    var layerData = GetSoundLayerData(layer);
    if (layerData == null) return;

    layerData.SoundData.FadeDuration = fadeDuration;
    layerData.SoundData.Pause();
    layerData.LayerState = SoundLayerState.Pause;
    UpdateLayer();
  }

  public void Resume(T layer, float fadeDuration = 0f) {
    var layerData = GetSoundLayerData(layer);
    if (layerData == null) return;

    layerData.SoundData.FadeDuration = fadeDuration;
    layerData.SoundData.Resume();
    layerData.LayerState = SoundLayerState.Play;
    UpdateLayer();
  }

  public void Stop(T layer, float fadeDuration = 0f) {
    var layerData = GetSoundLayerData(layer);
    if (layerData == null) return;

    layerData.SoundData.FadeDuration = fadeDuration;
    layerData.SoundData.Stop();
    layerData.LayerState = SoundLayerState.Stop;
    UpdateLayer();
  }

  private void UpdateLayer() {
    bool isPlayed = false;
    foreach (T layer in Enum.GetValues(typeof(T))) {
      var layerData = GetSoundLayerData(layer);
      if (layerData == null) continue;
      if (isPlayed) {
        if (layerData.LayerState != SoundLayerState.Play) continue;
        layerData.LayerState = SoundLayerState.Hide;
        layerData.SoundData.Pause();
      } else {
        switch (layerData.LayerState) {
        case SoundLayerState.Play:
          isPlayed = true;
          break;
        case SoundLayerState.Hide:
          layerData.LayerState = SoundLayerState.Play;
          layerData.SoundData.Play();
          isPlayed = true;
          break;
        }
      }
    }
  }

  public SoundLayerData GetSoundLayerData(T layer) {
    if (!_layerSoundDictionary.ContainsKey(layer)) return null;
    return _layerSoundDictionary[layer];
  }
}

}
#endif