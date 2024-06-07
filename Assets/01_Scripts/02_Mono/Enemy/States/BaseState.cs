using System;

public class BaseState : IState
{
    private Action OnEnterD;
    private Action OnExitD;
    private Action OnFixedUpdateD;

    public BaseState ( Func<bool> enterCondition, Action OnEnter, Action OnExit, Action OnFixedUpdate )
    {
        OnEnterD = OnEnter;
        OnExitD = OnExit;
        OnFixedUpdateD = OnFixedUpdate;
        EnterCondition = enterCondition;
    }

    public bool IsActive { get; set; }
    public Func<bool> EnterCondition { get; set; }

    public void OnEnter () => OnEnterD?.Invoke();

    public void OnExit () => OnExitD?.Invoke();

    public void OnFixedUpdate () => OnFixedUpdateD?.Invoke();
}