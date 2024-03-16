#if LIME_UNITASK
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.Utility;
using UnityEngine;

namespace LimeLibrary.Sound.Internal {

internal class SoundKit {
  private class SoundDataList {
    private readonly List<(string id, SoundData soundData)> _list = new();
    public IReadOnlyList<(string id, SoundData soundData)> List => _list;

    public void Add(string id, SoundData soundData) {
      _list.Add((id, soundData));
    }

    public void Remove(SoundData soundData) {
      _list.RemoveAll(x => x.soundData == soundData);
    }

    public void RemoveAll(string id) {
      _list.RemoveAll(x => x.id == id);
    }

    public bool Contains(string id) {
      return _list.Exists(x => x.id == id);
    }

    public SoundData Get(string id) {
      return _list.Find(x => x.id == id).soundData;
    }

    internal void Terminate() {
      foreach (var (_, soundData) in _list) {
        soundData.Destroy();
      }
      _list.Clear();
    }
  }

  private VolumeData VolumeData { get; set; } = new();
  private readonly SoundDataList _soundDataList = new();

  private ISoundInitializer Initializer { get; }
  private ISoundTerminator Terminator { get; }
  private ISoundCreator Creator { get; }
  private VolumeData MasterVolumeData { get; set; }
  private GameObject BindGameObject { get; }

  internal SoundKit(
    ISoundInitializer initializer,
    ISoundTerminator terminator,
    ISoundCreator creator,
    VolumeData masterVolumeData,
    GameObject bindGameObject) {
    Initializer = initializer;
    Terminator = terminator;
    Creator = creator;
    MasterVolumeData = masterVolumeData;
    BindGameObject = bindGameObject;
  }

  public async UniTask Initialize() {
    if (Initializer == null) return;
    await Initializer.Initialize();
  }

  internal SoundData Play(string id, float fadeDuration = 0f, SoundPlayType playType = SoundPlayType.Default) {
    SoundData soundData;
    switch (playType) {
    case SoundPlayType.Default:
      soundData = Create(id);
      break;
    case SoundPlayType.Stop:
      _soundDataList.Get(id)?.Stop();
      soundData = Create(id);
      break;
    case SoundPlayType.Reuse:
      soundData = GetOrCreateSound(id);
      break;
    default:
      soundData = Create(id);
      Assertion.Assert(false, playType);
      break;
    }

    UpdateVolume(soundData, MasterVolumeData);

    soundData.FadeDuration = fadeDuration;
    soundData.Play();

    WaitStopAndDestroy(soundData, BindGameObject.GetCancellationTokenOnDestroy()).RunHandlingError().Forget();
    return soundData;
  }

  internal void Pause(string id) {
    if (!_soundDataList.Contains(id)) return;
    var soundData = _soundDataList.Get(id);
    soundData.Pause();
  }

  internal void Resume(string id) {
    if (!_soundDataList.Contains(id)) return;
    var soundData = _soundDataList.Get(id);
    soundData.Resume();
  }

  internal void Stop(string id) {
    if (!_soundDataList.Contains(id)) return;
    var soundData = _soundDataList.Get(id);
    soundData.Stop();
  }

  private SoundData GetOrCreateSound(string id) {
    return !_soundDataList.Contains(id) ? Create(id) : _soundDataList.Get(id);
  }

  private SoundData Create(string id) {
    var soundSource = Creator.Create(id);
    var soundData = new SoundData(soundSource);
    _soundDataList.Add(id, soundData);
    return soundData;
  }

  private async UniTask WaitStopAndDestroy(SoundData soundData, CancellationToken cancellationToken) {
    await soundData.WaitPlayEndOrStop(cancellationToken);
    soundData.Destroy();
    _soundDataList.Remove(soundData);
  }

  internal void UpdateVolume(VolumeData masterVolumeData) {
    for (int i = 0; i < _soundDataList.List.Count; i++) {
      var (_, soundData) = _soundDataList.List[i];
      UpdateVolume(soundData, masterVolumeData);
    }
  }

  private void UpdateVolume(SoundData soundData, VolumeData masterVolumeData) {
    soundData.MasterVolume = GetVolume(masterVolumeData);
    soundData.UpdateVolume();
  }

  private float GetVolume(VolumeData masterVolumeData) => masterVolumeData.IsMute || VolumeData.IsMute ? 0 : VolumeData.Volume * masterVolumeData.Volume;

  internal void Terminate() {
    _soundDataList.Terminate();
    Terminator?.Terminate();
  }

  internal void SetVolume(float volume) {
    VolumeData.Volume = volume;
    UpdateVolume(MasterVolumeData);
  }

  internal void SetIsMute(bool isMute) {
    VolumeData.IsMute = isMute;
    UpdateVolume(MasterVolumeData);
  }
}

}
#endif