#if LIME_UNITASK
using System;
using System.Collections.Generic;
using LimeLibrary.Module;
using Cysharp.Threading.Tasks;
using LimeLibrary.Utility;

namespace LimeLibrary.Sound {

public class SoundManager : SingletonMonoBehaviour<SoundManager> {
  private readonly Dictionary<string, SoundKit> _soundKitDictionary = new(8);
  private readonly VolumeData _masterVolumeData = new();

  public async UniTask AddSoundKitWithInitialize<TKitType>(
    TKitType kitType,
    ISoundInitializer initializer,
    ISoundTerminator terminator,
    ISoundCreator creator)
    where TKitType : Enum {
    if (ExistsSoundKit(kitType)) {
      Assertion.Assert(false, $"SoundKit is already added. Type: {kitType}");
    }
    var soundKit = new SoundKit(initializer, terminator, creator, _masterVolumeData, gameObject);
    await soundKit.Initialize();
    _soundKitDictionary.Add(kitType.ToString(), soundKit);
  }

  public void RemoveSoundKitWithTerminate<TKitType>(TKitType kitType) where TKitType : Enum {
    if (!ExistsSoundKit(kitType)) {
      Assertion.Assert(false, $"SoundKit is not found. Type: {kitType}");
      return;
    }
    _soundKitDictionary[kitType.ToString()].Terminate();
    _soundKitDictionary.Remove(kitType.ToString());
  }

  public bool ExistsSoundKit<TKitType>(TKitType kitType) where TKitType : Enum {
    return _soundKitDictionary.ContainsKey(kitType.ToString());
  }

  private SoundKit GetSoundKit<TKitType>(TKitType kitType) where TKitType : Enum {
    if (!ExistsSoundKit(kitType)) {
      Assertion.Assert(false, $"SoundKit is not found. Type: {kitType}");
      return null;
    }
    return _soundKitDictionary[kitType.ToString()];
  }
  
  public SoundData Play<TKitType>(TKitType kitType, string id, float fadeDuration = 0f, SoundPlayType playType = SoundPlayType.Default) where TKitType : Enum {
    var soundKit = GetSoundKit(kitType);
    return soundKit?.Play(id, fadeDuration, playType);
  }

  public void Pause<TKitType>(TKitType kitType, string id) where TKitType : Enum {
    var soundKit = GetSoundKit(kitType);
    soundKit?.Pause(id);
  }

  public void Resume<TKitType>(TKitType kitType, string id) where TKitType : Enum {
    var soundKit = GetSoundKit(kitType);
    soundKit?.Resume(id);
  }

  public void Stop<TKitType>(TKitType kitType, string id) where TKitType : Enum {
    var soundKit = GetSoundKit(kitType);
    soundKit?.Stop(id);
  }

  public void SetMasterVolume(float volume) {
    _masterVolumeData.Volume = volume;
    UpdateVolume();
  }

  public void SetMasterIsMute(bool isMute) {
    _masterVolumeData.IsMute = isMute;
    UpdateVolume();
  }

  public void SetVolume<TKitType>(TKitType kitType, float volume) where TKitType : Enum {
    var soundKit = GetSoundKit(kitType);
    soundKit?.SetVolume(volume);
  }

  public void SetIsMute<TKitType>(TKitType kitType, bool isMute) where TKitType : Enum {
    var soundKit = GetSoundKit(kitType);
    soundKit?.SetIsMute(isMute);
  }

  private void UpdateVolume() {
    foreach (var (_, soundKit) in _soundKitDictionary) {
      soundKit.UpdateVolume(_masterVolumeData);
    }
  }
}

}
#endif