#if LIME_UNIRX
using System;
using LimeLibrary.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LimeLibrary.Extensions {

/// <summary>
/// UniRxの拡張メソッド
/// </summary>
public static class UniRxExtensions {
  public enum Lifespan {
    Scene,
    Application,
  }

  private static GameObject s_sceneLifespanGameObject;
  private static GameObject s_applicationLifespanGameObject;

  /// <summary>
  /// シーンの寿命に紐付けるGameObject
  /// </summary>
  public static GameObject SceneLifespanGameObject {
    get {
      if (s_sceneLifespanGameObject == null) s_sceneLifespanGameObject = new GameObject("SceneLifespan");
      return s_sceneLifespanGameObject;
    }
  }

  /// <summary>
  /// アプリケーションの寿命に紐付けるGameObject
  /// </summary>
  public static GameObject ApplicationLifespanGameObject {
    get {
      if (s_applicationLifespanGameObject == null) {
        s_applicationLifespanGameObject = new GameObject("ApplicationLifespan");
        Object.DontDestroyOnLoad(s_applicationLifespanGameObject);
      }
      return s_applicationLifespanGameObject;
    }
  }

  /// <summary>
  /// Disposableをシーンやアプリケーションの寿命に紐付ける
  /// </summary>
  public static T AddTo<T>(this T disposable, Lifespan lifespan) where T : IDisposable {
    switch (lifespan) {
    case Lifespan.Scene:
      disposable.AddTo(SceneLifespanGameObject);
      return disposable;

    case Lifespan.Application:
      disposable.AddTo(ApplicationLifespanGameObject);
      return disposable;

    default:
      Assertion.Assert(false);
      return disposable;
    }
  }

  /// <summary>
  /// ParticleSystemのStop時のObservableを取得
  /// </summary>
  public static IObservable<Unit> OnParticleSystemStoppedAsObservable(this ParticleSystem particleSystem) {
    if (particleSystem == null) {
      Assertion.Assert(false);
      return Observable.Empty<Unit>();
    }

    return ChangeParticleSystemStopActionCallback(particleSystem);
  }

  private static IObservable<Unit> ChangeParticleSystemStopActionCallback(ParticleSystem particleSystem) {
    var observable = particleSystem.gameObject.OnParticleSystemStoppedAsObservable();
    var mainModule = particleSystem.main;
    switch (mainModule.stopAction) {
    case ParticleSystemStopAction.Disable:
      observable.Subscribe(_ => particleSystem.gameObject.SetActive(false)).AddTo(particleSystem);
      break;
    case ParticleSystemStopAction.Destroy:
      observable.Subscribe(_ => Object.Destroy(particleSystem.gameObject)).AddTo(particleSystem);
      break;
    }
    mainModule.stopAction = ParticleSystemStopAction.Callback;
    return observable;
  }

  private static IObservable<Unit> OnParticleSystemStoppedAsObservable(this GameObject gameObject) {
    if (gameObject == null) {
      Assertion.Assert(false);
      return Observable.Empty<Unit>();
    }
    return gameObject.GetOrAddComponent<ObservableParticleTrigger>().OnParticleSystemStoppedAsObservable();
  }

  /// <summary>
  /// Buttonを右クリックした時のObservableを取得
  /// </summary>
  public static IObservable<Unit> OnRightClickAsObservable(this Button button) {
    return Observable.Create<Unit>(observer => {
      var rightClickHandler = button.gameObject.GetOrAddComponent<ObservableRightClickTrigger>();
      return rightClickHandler.OnRightClickAsObservable().Subscribe(observer).AddTo(button);
    });
  }
}

}
#endif