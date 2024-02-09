using System;
using System.Collections.Generic;
using System.Linq;
using LimeLibrary.Utility;
#if LIME_UNIRX
using UniRx;
#endif

namespace LimeLibrary.Module {

public class StateMachine<TStateId, TContext, TEventType> where TStateId : Enum, IComparable {
  private class StateData {
    public StateData(IState<TStateId, TContext, TEventType> state) {
      State = state;
    }

    public IState<TStateId, TContext, TEventType> State { get; }
    public bool IsAwaked { get; set; }

    public void ReceiveEvent(TEventType eventType) {
      State.ReceiveEvent(eventType);
    }

    public void Enter(TContext context, TStateId fromStateId) {
      if (!IsAwaked) {
        State.Awake(context);
        IsAwaked = true;
      }
      State.Enter(context, fromStateId);
    }

    public OptionalEnum<TStateId> Update(TContext context) {
      return State.Update(context);
    }

    public void FixedUpdate(TContext context) {
      State.FixedUpdate(context);
    }

    public void Exit(TContext context) {
      State.Exit(context);
    }
  }

  private TStateId _defaultStateId;
  private OptionalEnum<TStateId> _requestStateId;
  private readonly Dictionary<TStateId, StateData> _stateHashTable = new Dictionary<TStateId, StateData>();
  private bool _isFirst = false;

  public TStateId NowStateId { get; private set; }

#if LIME_UNIRX
  private readonly Subject<TStateId> _onEnterSubject = new();
  private readonly Subject<TStateId> _onExitSubject = new();
  public IObservable<TStateId> OnEnterObservable => _onEnterSubject;
  public IObservable<TStateId> OnExitObservable => _onExitSubject;
#endif

  public IReadOnlyDictionary<TStateId, IState<TStateId, TContext, TEventType>> StateHashTable =>
    _stateHashTable.ToDictionary(pair => pair.Key, pair => pair.Value.State);
  public bool IsStopTransitionNextState { get; set; }

  public void SetState(TStateId stateId, IState<TStateId, TContext, TEventType> state) {
    _stateHashTable[stateId] = new StateData(state);
  }

  public IState<TStateId, TContext, TEventType> GetState(TStateId stateId) {
    if (!_stateHashTable.ContainsKey(stateId)) {
      Assertion.Assert(false, stateId);
      return null;
    }
    return _stateHashTable[stateId].State;
  }

  public IState<TStateId, TContext, TEventType> GetNowState() {
    return _stateHashTable[NowStateId].State;
  }

  public void SetInitState(TStateId stateId) {
    _defaultStateId = stateId;
    NowStateId = stateId;
    _isFirst = true;
  }

  public void InitializeAwake() {
    foreach (var (_, stateData) in _stateHashTable) {
      stateData.IsAwaked = false;
    }
  }

  public void RequestEvent(TEventType eventType) {
    _stateHashTable[NowStateId].ReceiveEvent(eventType);
  }

  public void RequestReset() {
    _requestStateId = _defaultStateId;
  }

  public void RequestState(TStateId stateId) {
    _requestStateId = stateId;
  }

  public void OnUpdate(TContext context) {
    if (_isFirst) {
      if (IsStopTransitionNextState && !_requestStateId.HasValue) return;
      _stateHashTable[NowStateId].Enter(context, NowStateId);

#if LIME_UNIRX
      _onEnterSubject.OnNext(NowStateId);
#endif
      _isFirst = false;
    }

    var newStateId = _stateHashTable[NowStateId].Update(context);
    bool isRequestState = _requestStateId.HasValue;
    if (isRequestState) {
      newStateId = _requestStateId;
      _requestStateId = OptionalEnum<TStateId>.None;
    }
    if (!newStateId.HasValue) return;
    if (IsStopTransitionNextState && !isRequestState) return;

    _stateHashTable[NowStateId].Exit(context);

#if LIME_UNIRX
    _onExitSubject.OnNext(NowStateId);
#endif
    if (_stateHashTable.TryGetValue(newStateId.Value, out var value)) {
      NowStateId = newStateId.Value;
      value.Enter(context, NowStateId);

#if LIME_UNIRX
      _onEnterSubject.OnNext(newStateId.Value);
#endif
    } else {
      Assertion.Assert(false, newStateId.Value);
    }
  }

  public void OnFixedUpdate(TContext context) {
    _stateHashTable[NowStateId].FixedUpdate(context);
  }

  public void ExitState(TContext context) {
    _stateHashTable[NowStateId].Exit(context);

#if LIME_UNIRX
    _onExitSubject.OnNext(NowStateId);
#endif
  }

  public void TransitionState(TStateId stateId, TContext context) {
    _stateHashTable[NowStateId].Exit(context);

#if LIME_UNIRX
    _onExitSubject.OnNext(NowStateId);
#endif
    if (!_stateHashTable.ContainsKey(stateId)) {
      Assertion.Assert(false, stateId);
      return;
    }

    var fromStateId = NowStateId;
    _stateHashTable[stateId].Enter(context, fromStateId);

#if LIME_UNIRX
    _onEnterSubject.OnNext(stateId);
#endif
    NowStateId = stateId;
  }
}

}