#if LIME_R3 && LIME_UNITASK
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LimeLibrary.Extensions;
using LimeLibrary.Utility;
using R3;
using UnityEngine;

namespace LimeLibrary.Scene.Internal {

[DefaultExecutionOrder(-5)]
[RequireComponent(typeof(SceneUpdaterInterface))]
internal class SceneUpdater : MonoBehaviour {
  private readonly HashSet<IScene> _scenes = new();

  private SceneUpdaterInterface _interface;

  private Type _prevSceneType;
  private UniTask _sceneCreateTask;

  private void Awake() {
    _interface = GetComponent<SceneUpdaterInterface>();

    _interface.OnAddSceneObservable.Subscribe(scene => {
      _scenes.Add(scene);
    }).AddTo(this);
  }

  private void Update() {
    switch (_interface.GetSceneState()) {
    case SceneState.CheckNextScene: {
      if (_interface.IsRequested()) {
        var requestSceneType = _interface.GetRequestSceneId();
        _interface.ResetRequest();
        if (CheckRequest(requestSceneType)) {
          _interface.SetNowSceneType(requestSceneType);
          _interface.SetSceneState(SceneState.Create);
        }
      }
      break;
    }

    case SceneState.Create: {
      var scene = GetScene(_interface.GetNowSceneType());
      if (scene != null) {
        scene.Initialize();
        _sceneCreateTask = scene.CreateAsync(_prevSceneType, this.GetCancellationTokenOnDestroy()).RunHandlingError();
        _interface.SetSceneState(SceneState.CreateWait);
      }
      break;
    }

    case SceneState.CreateWait: {
      if (_sceneCreateTask.GetAwaiter().IsCompleted) {
        _interface.SetSceneState(SceneState.Running);
        _interface.OnStartScene(GetScene(_interface.GetNowSceneType()));
      }
      break;
    }

    case SceneState.Running: {
      GetScene(_interface.GetNowSceneType())?.OnUpdate();

      if (_interface.IsRequested()) {
        var requestSceneType = _interface.GetRequestSceneId();

        var scene = GetScene(_interface.GetNowSceneType());
        _interface.OnEndScene(GetScene(_interface.GetNowSceneType()));
        scene?.Destroy(requestSceneType);
        scene?.Dispose();

        _prevSceneType = _interface.GetNowSceneType();

        _interface.SetSceneState(SceneState.CheckNextScene);
      }
      break;
    }

    default:
      Assertion.Assert(false);
      break;
    }
  }

  private void FixedUpdate() {
    switch (_interface.GetSceneState()) {
    case SceneState.Running:
      GetScene(_interface.GetNowSceneType())?.OnFixedUpdate();
      break;
    }
  }

  private void LateUpdate() {
    switch (_interface.GetSceneState()) {
    case SceneState.Running:
      GetScene(_interface.GetNowSceneType())?.OnLateUpdate();
      break;
    }
  }

  private void OnDestroy() {
    GetScene(_interface.GetNowSceneType())?.Dispose();
  }

  private IScene GetScene(Type sceneType) {
    var scene = _scenes.FirstOrDefault(scene => scene.GetType() == sceneType || scene.GetType().GetInterfaces().Contains(sceneType));
    return scene;
  }

  private bool CheckRequest(Type requestSceneType) {
    if (requestSceneType == null) {
      Assertion.Assert(false, "RequestSceneType is null.");
      return false;
    }

    if (!_scenes.Any(scene => scene.GetType() == requestSceneType || scene.GetType().GetInterfaces().Contains(requestSceneType))) {
      Assertion.Assert(false, "RequestSceneType is not found. " + requestSceneType);
      return false;
    }

    if (requestSceneType == _interface.GetNowSceneType()) {
      Assertion.Assert(false, "RequestSceneType is same as NowSceneType. " + requestSceneType);
      return false;
    }

    return true;
  }
}

}
#endif