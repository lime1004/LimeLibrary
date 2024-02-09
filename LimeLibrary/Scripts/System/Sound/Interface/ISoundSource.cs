using System.Threading;

namespace LimeLibrary.Sound {

public interface ISoundSource {
  public float Volume { get; set; }
  public CancellationToken CancellationToken { get; }

  public void Play();
  public void SetPause(bool isPause);
  public void Stop();
  
  public void Destroy();
  
  public bool IsPlaying();
  public bool IsPlayEnd();
  public bool IsPaused();
  public bool IsStopped();
}

}