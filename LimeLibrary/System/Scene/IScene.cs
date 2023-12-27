using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace LimeLibrary.Scene {

public interface IScene : IDisposable {
  public void Initialize();
  public UniTask CreateAsync(Type prevSceneType, CancellationToken cancellationToken);
  public void OnUpdate();
  public void OnFixedUpdate();
  public void OnLateUpdate();
  public void Destroy(Type nextSceneType);
}

}