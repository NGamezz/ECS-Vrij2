using System;

public interface IState
{
    public bool IsActive { get; set; }
    public Func<bool> EnterCondition { get; set; }
    public void OnEnter ();
    public void OnExit ();
    public void OnFixedUpdate ();
}