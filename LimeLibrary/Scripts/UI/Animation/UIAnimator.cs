using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.UI.View;
using UnityEngine;

namespace LimeLibrary.UI.Animation {

[RequireComponent(typeof(IUIView))]
public class UIAnimator : MonoBehaviour {
  [SerializeReference]
  private List<IUIAnimationPlayer> _playerList = new();

  public async UniTask Play(string id, CancellationToken cancellationToken) {
    foreach (var player in _playerList) {
      if (player.Id != id) continue;
      player.Play();
    }
    await _playerList.
      Where(player => player.Id == id).
      Select(player => player.WaitPlaying(cancellationToken));
  }

  public void PlayImmediate(string id) {
    foreach (var player in _playerList) {
      if (player.Id != id) continue;
      player.PlayImmediate();
    }
  }

  public void Pause(string id) {
    foreach (var player in _playerList) {
      if (player.Id != id) continue;
      player.Pause();
    }
  }

  public void Stop(string id) {
    foreach (var player in _playerList) {
      if (player.Id != id) continue;
      player.Stop();
    }
  }

  public bool Exists(string id) {
    foreach (var player in _playerList) {
      if (player.Id != id) continue;
      return true;
    }
    return false;
  }

  public void Register(IUIAnimationPlayer player) {
    _playerList.Add(player);
  }
}

}