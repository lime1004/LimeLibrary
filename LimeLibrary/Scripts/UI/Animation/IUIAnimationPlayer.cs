using System.Threading;
using Cysharp.Threading.Tasks;

namespace LimeLibrary.UI.Animation {

public interface IUIAnimationPlayer {
  public string Id { get; }

  public void Play();
  public void PlayImmediate();
  public void Pause();
  public void Stop();
  public UniTask WaitPlaying(CancellationToken cancellationToken);
}

}