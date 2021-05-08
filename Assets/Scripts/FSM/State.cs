/** \file State.cs
*  \brief Contains a single abstract class for a finite state machine to derive the structure of its "states" for.
*/
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** \class State
*  \brief This abstract class defines the structure of a "state" for the finite state machine in what it does on update, entry and exit. 
*/
public abstract class State
{
    public abstract Type StateUpdate();     //!< Pure virtual function which the implementation of would be called continuously on a loop until the FSM swaps to another state.
    public abstract Type StateEnter();      //!< Pure virtual function which the implementation of would be called when the state is entered.
    public abstract Type StateExit();       //!< Pure virtual function which the implementation of would be called when the state is exited in favour of another state.
}
