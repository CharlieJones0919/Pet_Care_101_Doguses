/** \file Pause.cs */
using System;
using UnityEngine;

/** \class Pause
 * \brief Checks the dog's rules for if the state should be swapped out of, does regular speed check/bonus sequence evaluations, then checks this sequence's ending sequences which will set a state swap fact as true if successful, in which case, the update should be exited so the swap can be caught by the next rule check. If both these checks don't result in the state being exited, proceed with this state's intended behaviour.
 */
public class Pause : State
{
    protected Dog doggo; //!< The dog this state class instantiation is for.

    /** \fn Pause
     * \brief A constructor to require that the Dog class reference is set to an instance when the state is created. */
    public Pause(Dog subjectDog) { doggo = subjectDog; }

    /** \fn StateEnter
     * \brief Function called by the FiniteStateMachine class when a state is first entered or swapped into. Sets the fact that the dog is in this state to true, and that the dog still needs to swap into it as false.
     * Upon entry this state will start the Pause couroutine which just sets the fact WAITING to true until its timer ends.
     */
    public override Type StateEnter()
    {
        doggo.m_facts["SWP_PAUSE"] = false;
        if (doggo.m_facts["IS_FOCUS"]) Debug.Log(doggo.name + ": Entering Pause State");
        doggo.StartCoroutine(doggo.Pause(2.5f));
        return null;
    }

    /** \fn StateExit 
     * \brief Function called by the FiniteStateMachine class when a state is exited or swapped out of. Sets the fact that the dog is in this state to false again. */
    public override Type StateExit()
    {
        if (doggo.m_facts["IS_FOCUS"]) Debug.Log(doggo.name + ": Exiting Pause State");
        return null;
    }

    /** \fn StateUpdate 
    * \brief Does regular speed check/bonus sequence evaluations then checks the dog's rules for if the state should be swapped out of. For the pause state the only conditions for exiting are that the WAITING and NEEDS_2_FINISH_ANIM are both false.
    * When these conditions have been met, the dog will swap back into the Idle state to identify which state to enter next, otherwise, the state will be re-entered to re-trigger the Pause couroutine timer. Regular behaviour for this state is to do nothing. The 
    */
    public override Type StateUpdate()
    {
        // Check global conditional sequences. (These won't result in state changes directly).
        foreach (BTSequence sequenceCheck in doggo.GlobalSequences)
        {
            sequenceCheck.Evaluate();
        }

        // Wait until timer has finished or animation is done.
        if (doggo.m_facts["WAITING"] || doggo.m_facts["NEEDS_2_FINISH_ANIM"])
        {
            return typeof(Pause);
        }
        return typeof(Idle);
    }
}