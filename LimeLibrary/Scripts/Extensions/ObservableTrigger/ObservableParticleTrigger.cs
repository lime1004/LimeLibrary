#if LIME_UNIRX
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace LimeLibrary.Extensions {

[DisallowMultipleComponent]
public class ObservableParticleTrigger : ObservableTriggerBase {
  private Subject<Unit> _onParticleSystemStopped;

  private void OnParticleSystemStopped() {
    _onParticleSystemStopped.OnNext(Unit.Default);
    _onParticleSystemStopped.OnCompleted();
  }

  public Subject<Unit> OnParticleSystemStoppedAsObservable() {
    return _onParticleSystemStopped ??= new Subject<Unit>();
  }

  protected override void RaiseOnCompletedOnDestroy() {
    _onParticleSystemStopped?.OnCompleted();
  }
}

}
#endif