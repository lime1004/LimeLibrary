using System;

namespace LimeLibrary.Module {

public interface IState<TStateId, in TContext, in TEventType> where TStateId : Enum {
  public void Awake(TContext context) { }
  public void Enter(TContext context, TStateId fromStateId);
  public void ReceiveEvent(TEventType eventType);
  public OptionalEnum<TStateId> Update(TContext context);
  public void FixedUpdate(TContext context);
  public void Exit(TContext context);
}

}