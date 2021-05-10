/** \file Idle.cs */
using System;
using UnityEngine;

/** \class Idle
 * \brief Checks the dog's rules for if the state should be swapped out of, does regular speed check/bonus sequence evaluations, then checks this sequence's ending sequences which will set a state swap fact as true if successful, in which case, the update should be exited so the swap can be caught by the next rule check. If both these checks don't result in the state being exited, proceed with this state's intended behaviour.
 */
public class Idle : State
{
    protected Dog doggo; //!< The dog this state class instantiation is for.

    /** \fn Idle
     * \brief A constructor to require that the Dog class reference is set to an instance when the state is created. */
    public Idle(Dog subjectDog) { doggo = subjectDog; }

    /** \fn StateEnter 
     * \brief Function called by the FiniteStateMachine class when a state is first entered or swapped into. Sets the fact that the dog is in this state to true, and that the dog still needs to swap into it as false. */
    public override Type StateEnter()
    {
        doggo.m_facts["IDLE"] = true;
        doggo.m_facts["SWP_IDLE"] = false;

        Debug.Log(doggo.name + ": Entering Idle State");
        return null;
    }

    /** \fn StateExit 
     * \brief Function called by the FiniteStateMachine class when a state is exited or swapped out of. Sets the fact that the dog is in this state to false again. */
    public override Type StateExit()
    {
        doggo.m_facts["IDLE"] = false;
        Debug.Log(doggo.name + ": Exiting Idle State");
        return null;
    }

    /** \fn StateUpdate 
    * \brief Checks the dog's rules for if the state should be swapped out of, does regular speed check/bonus sequence evaluations, then checks this sequence's ending sequences which will set a state swap fact as true if successful, in which case, the update should be exited so the swap can be caught by the next rule check. 
    * If both these checks don't result in the state being exited, proceed with this state's intended behaviour.
    * Regular behaviour for this state is to follow paths to the Dog's Pathfining random point object, then once reached, move it to a new random position. This functionality is called upon in the Wander() function which uses the Pathfinding script. 
    */
    public override Type StateUpdate()
    {
        //Checking each rule in the rules list to see if a state change should occur.
        foreach (Rule rule in doggo.m_rules)
        {
            if (rule.CheckRule(doggo.m_facts) != null)
            {
                return rule.CheckRule(doggo.m_facts);
            }
        }

        // Check global conditional sequences. (These won't result in state changes directly).
        foreach (BTSequence sequenceCheck in doggo.GlobalSequences)
        {
            sequenceCheck.Evaluate(); 
        }

        // Check if the state should be exited. By returning null the state change will be caught by the next rule check.
        foreach (BTSequence sequenceCheck in doggo.IdleEndSequences)
        {
            if (sequenceCheck.Evaluate() == BTState.SUCCESS) { return null; }
        }

        // If the state wasn't exited, proceed with the regular state behaviour.
        doggo.Wander();
        return null;
    }
}