using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class StateManager
{
    private List<IState> states = new();
    private IState currentState;

    public bool AreStatesActive ()
    {
        bool active = false;

        foreach ( var state in states )
        {
            if ( state.EnterCondition() )
            {
                active = true;
            }
        }

        return active;
    }

    public void AddState ( params IState[] states )
    {
        foreach ( var state in states )
        {
            if ( !this.states.Contains(state) )
            {
                this.states.Add(state);
            }
        }
    }

    public void RemoveState ( IState state )
    {
        if ( states.Contains(state) )
        {
            if ( currentState == state )
            {
                currentState?.OnExit();
                currentState = null;
            }

            states.Remove(state);
        }
    }

    private void SwitchState ( IState newState )
    {
        currentState?.OnExit();
        currentState = newState;
        currentState?.OnEnter();
    }

    public void OnFixedUpdate ()
    {
        currentState?.OnFixedUpdate();

        foreach ( var state in states )
        {
            if ( !state.IsActive && state.EnterCondition() )
            {
                SwitchState(state);
            }
        }
    }
}