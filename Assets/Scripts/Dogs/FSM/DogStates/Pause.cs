using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : State
{
    protected Dog doggo;

    public Pause(Dog subjectDog) { doggo = subjectDog; }

    public override Type StateEnter()
    {
        doggo.m_facts["SWP_PAUSE"] = false;
        if (doggo.m_facts["IS_FOCUS"]) Debug.Log(doggo.name + ": Entering Pause State");
        doggo.StartCoroutine(doggo.Pause(2.5f));
        return null;
    }

    public override Type StateExit()
    {
        if (doggo.m_facts["IS_FOCUS"]) Debug.Log(doggo.name + ": Exiting Pause State");
        return null;
    }

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