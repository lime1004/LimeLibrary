#if LIME_UNIRX
using LimeLibrary.Module;
using System;
using UniRx;

namespace LimeLibrary.Scene {

/// <summary>
/// Sceneシステムアクセス用クラス
/// </summary>
public class SceneUpdaterInterface : SingletonMonoBehaviour<SceneUpdaterInterface> {
  private Type _requestSceneType;
  private Type _nowSceneType;
  private SceneState _sceneState;

  private readonly Subject<Type> _onChangeSceneTypeSubject = new();
  public IObservable<Type> OnChangeSceneTypeObservable => _onChangeSceneTypeSubject;

  private readonly Subject<IScene> _onAddSceneSubject = new();
  internal IObservable<IScene> OnAddSceneObservable => _onAddSceneSubject;

  private void OnDestroy() {
    _onChangeSceneTypeSubject.Dispose();
  }

  /// <summary>
  /// Sceneの追加
  /// </summary>
  public void AddScene(IScene scene) => _onAddSceneSubject.OnNext(scene);

  /// <summary>
  /// Scene変更リクエスト
  /// </summary>
  public void RequestChangeScene<T>() => _requestSceneType = typeof(T);

  internal void ResetRequest() => _requestSceneType = null;
  internal bool IsRequested() => _requestSceneType != null;
  internal Type GetRequestSceneId() => _requestSceneType;

  /// <summary>
  /// 現在のSceneのTypeを取得
  /// </summary>
  public Type GetNowSceneType() => _nowSceneType;

  internal void SetNowSceneType(Type sceneId) {
    _nowSceneType = sceneId;
    _onChangeSceneTypeSubject.OnNext(sceneId);
  }

  /// <summary>
  /// 現在のSceneの状態を取得
  /// </summary>
  public SceneState GetSceneState() => _sceneState;

  internal void SetSceneState(SceneState sceneState) => _sceneState = sceneState;
}

}
#endif