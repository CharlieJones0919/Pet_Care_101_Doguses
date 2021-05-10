/** \file Tired.cs */
using System;
using UnityEngine;

/** \class Tired
 * \brief Checks the dog's rules for if the state should be swapped out of, does regular speed check/bonus sequence evaluations, then checks this sequence's ending sequences which will set a state swap fact as true if successful, in which case, the update should be exited so the swap can be caught by the next rule check. If both these checks don't result in the state being exited, proceed with this state's intended behaviour.
 */
public class Tired : State
{
    protected Dog doggo; //!< The dog this state class instantiation is for.

    /** \fn Tired
     * \brief A constructor to require that the Dog class reference is set to an instance when the state is created. */
    public Tired(Dog subjectDog) { doggo = subjectDog; }

    /** \fn StateEnter
    * \brief Function called by the FiniteStateMachine class when a state is first entered or swapped into. Sets the fact that the dog is in this state to true, and that the dog still needs to swap into it as false. */
    public override Type StateEnter()
    {
        doggo.m_facts["TIRED"] = true;
        doggo.m_facts["SWP_TIRED"] = false;

        if (doggo.m_facts["IS_FOCUS"]) Debug.Log(doggo.name + ": Entering Tired State");
        return null;
    }

    /** \fn StateExit 
     * \brief Function called by the FiniteStateMachine class when a state is exited or swapped out of. Sets the fact that the dog is in this state to false again. Ends Item use if the dog was using one, and triggers the Waking Up animation. */
    public override Type StateExit()
    {
        doggo.m_facts["TIRED"] = false;
        if (doggo.m_facts["IS_FOCUS"]) Debug.Log(doggo.name + ": Exiting Tired State");

        if (doggo.m_facts["USING_ITEM"])
        {
            doggo.m_animationCTRL.SetTrigger("WakingUp");
            doggo.EndItemUse();
        }
        return null;
    }

    /** \fn StateUpdate 
    * \brief Checks the dog's rules for if the state should be swapped out of, does regular speed check/bonus sequence evaluations, then checks this sequence's ending sequences which will set a state swap fact as true if successful, in which case, the update should be exited so the swap can be caught by the next rule check. 
    * If both these checks don't result in the state being exited, proceed with this state's intended behaviour.
    * Regular behaviour for this state is to find bed items, travel towards them, then use the items once they have been reached. 
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
        foreach (BTSequence sequenceCheck in doggo.TiredEndSequences)
        {
            if (sequenceCheck.Evaluate() == BTState.SUCCESS) { return null; }
        }

        // If the state wasn't exited, proceed with the regular state behaviour.
        if (doggo.FindItemType(ItemType.BED))
        {
            if (!doggo.m_facts["USING_ITEM"])
            {
                if (doggo.ReachedTarget())
                {
                    doggo.m_animationCTRL.SetTrigger("GoingToSleep");
                    doggo.UseItem();
                }
            }
        }
        return null;
    }
}