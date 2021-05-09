using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : State
{
    protected Dog doggo;

    public Idle(Dog subjectDog) { doggo = subjectDog; }

    public override Type StateEnter()
    {
        doggo.m_facts["IDLE"] = true;
        doggo.m_facts["SWP_IDLE"] = false;

        Debug.Log(doggo.name + ": Entering Idle State");
        return null;
    }

    public override Type StateExit()
    {
        doggo.m_facts["IDLE"] = false;
        Debug.Log(doggo.name + ": Exiting Idle State");
        return null;
    }

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