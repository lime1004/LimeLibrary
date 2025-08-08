#if LIME_R3
using LimeLibrary.Module;
using System;
using R3;

namespace LimeLibrary.Scene {

/// <summary>
/// Sceneシステムアクセス用クラス
/// </summary>
public class SceneUpdaterInterface : SingletonMonoBehaviour<SceneUpdaterInterface> {
  private Type _requestSceneType;
  private Type _nowSceneType;
  private SceneState _sceneState;

  private readonly Subject<Type> _onChangeSceneTypeSubject = new();
  private readonly Subject<IScene> _onAddSceneSubject = new();
  private readonly Subject<IScene> _onStartSceneSubject = new();
  private readonly Subject<IScene> _onEndSceneSubject = new();

  public Observable<Type> OnChangeSceneTypeObservable => _onChangeSceneTypeSubject;
  internal Observable<IScene> OnAddSceneObservable => _onAddSceneSubject;
  public Observable<IScene> OnStartSceneObservable => _onStartSceneSubject;
  public Observable<IScene> OnEndSceneObservable => _onEndSceneSubject;

  private void OnDestroy() {
    _onChangeSceneTypeSubject.Dispose();
    _onAddSceneSubject.Dispose();
    _onStartSceneSubject.Dispose();
    _onEndSceneSubject.Dispose();
  }

  public void AddScene(IScene scene) => _onAddSceneSubject.OnNext(scene);

  public void RequestChangeScene<T>() => _requestSceneType = typeof(T);

  public Type GetNowSceneType() => _nowSceneType;

  public SceneState GetSceneState() => _sceneState;

  internal void ResetRequest() => _requestSceneType = null;
  internal bool IsRequested() => _requestSceneType != null;
  internal Type GetRequestSceneId() => _requestSceneType;

  internal void SetNowSceneType(Type sceneId) {
    _nowSceneType = sceneId;
    _onChangeSceneTypeSubject.OnNext(sceneId);
  }

  internal void SetSceneState(SceneState sceneState) => _sceneState = sceneState;

  internal void OnStartScene(IScene scene) => _onStartSceneSubject.OnNext(scene);
  internal void OnEndScene(IScene scene) => _onEndSceneSubject.OnNext(scene);
}

}
#endif