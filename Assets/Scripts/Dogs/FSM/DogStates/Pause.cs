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
#if UNITY_EDITOR
        if (doggo.m_isFocusDog) Debug.Log(doggo.name + ": Entering Pause State");
#endif
        doggo.StartCoroutine(doggo.Pause(3.0f));
        return null;
    }

    public override Type StateExit()
    {
#if UNITY_EDITOR
        if (doggo.m_isFocusDog) Debug.Log(doggo.name + ": Exiting Pause State");
#endif
        return null;
    }

    public override Type StateUpdate()
    {
        if (doggo.m_waiting || doggo.m_needsToFinishAnim || doggo.m_beingPet)
        {
            return typeof(Pause);
        }
        else
        {
            return typeof(Idle);
        }
    }
}