/** \file FiniteStateMachine.cs
*  \brief Contains an a class in which the abstract functions from BaseState are set to the implementation of the current state.
*/
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** \class FiniteStateMachine
*  \brief Stores a list of states and a current state. Calls the current state's function implementations from the base State class' abstractions, and allows the FSM to swap into another state. 
*/
public class FiniteStateMachine : MonoBehaviour
{
    private Dictionary<Type, State> states; //!< List of all the FSM's states. 
    public State currentState;              //!< Stores the value of the current state.

    /** \fn CurrentState
    *  \brief Gets and sets the FSM's current state.
    */
    public State CurrentState
    {
        get { return currentState; }
        private set { currentState = value; }
    }

    /** \fn CurrentState
    *  \brief Sets the states passed in as a parameter as the FSM's list of states.
    */
    public void SetStates(Dictionary<Type, State> states)
    {
        this.states = states;
    }

    /** \fn Update
    *  \brief Calls the current state's update function on a loop and checks if it has returned a next state to swap to.
    */
    void Update()
    {
        //If the CurrentState hasn't been set yet, set it to the first state in the list. 
        if (CurrentState == null)
        {
            CurrentState = states.Values.First();
            SwitchToState(currentState.GetType());
        }
        else //If the CurrentState has been set, get what it has returned from its latest update.
        {
            //Get the state if any that the current state has returned from its update. If it returns as null, the FSM will remain in this state.
            var nextState = CurrentState.StateUpdate();

            //If the current state has returned a state to swap to from its latest update, swap to that state.
            if ((nextState != null) && (nextState != currentState.GetType()))
            {
                SwitchToState(nextState);
            }
        }
    }

    /** \fn SwitchToState
    *  \brief Calls the current state's end function, swaps the current state to the new state, then calls that new state's enter function.
    *  \param nextState The state to set the current state as.
    */
    void SwitchToState(Type nextState)
    {
        CurrentState.StateExit();
        CurrentState = states[nextState];
        CurrentState.StateEnter();
    }
}
