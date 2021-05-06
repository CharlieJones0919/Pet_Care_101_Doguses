using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playful : State
{
    protected Dog doggo;

    public Playful(Dog subjectDog) { doggo = subjectDog; }

    public override Type StateEnter()
    {
        doggo.m_facts["PLAYFUL"] = true;
        doggo.m_facts["SWP_PLAYFUL"] = false;

        if (doggo.m_facts["IS_FOCUS"]) Debug.Log(doggo.name + ": Entering Playful State");
        return null;
    }

    public override Type StateExit()
    {
        doggo.m_facts["PLAYFUL"] = false;
        if (doggo.m_facts["IS_FOCUS"]) Debug.Log(doggo.name + ": Exiting Playful State");

        if (doggo.m_facts["USING_ITEM"])
        {
            doggo.MoveWalking();
            doggo.EndItemUse();
        }
        return null;
    }

    public override Type StateUpdate()
    {
        //if ((!doggo.Loved() || !doggo.Happy()) && doggo.FindItemType(ItemType.TOYS))
        //{
        //    if (!doggo.m_facts["USING_ITEM"])
        //    {
        //        if (doggo.ReachedTarget())
        //        {
        //            doggo.UseItem();
        //            doggo.PickUpTarget();
        //            doggo.SetRunning(true);
        //        }
        //    }
        //    else { doggo.Wander(); }
        //}
        //else {  return typeof(Idle);  }

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
        foreach (BTSequence sequenceCheck in doggo.PlayfulEndSequences)
        {
            if (sequenceCheck.Evaluate() == BTState.SUCCESS) { return null; }
        }

        // If the state wasn't exited, proceed with the regular state behaviour.
        if (doggo.FindItemType(ItemType.TOYS))
        {
            if (!doggo.m_facts["USING_ITEM"])
            {
                if (doggo.ReachedTarget())
                {
                    doggo.UseItem();
                    doggo.PickUpTarget();
                    doggo.MoveRunning();
                }
            }
            else { doggo.Wander(); }
        }
        return null;
    }
}