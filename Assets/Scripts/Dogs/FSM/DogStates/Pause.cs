﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : State
{
    protected Dog doggo;

    //Called in the Chuck script to set this state's reference to itself.
    public Pause(Dog subjectDog)
    {
        doggo = subjectDog;
    }

    public override Type StateEnter()
    {
        Debug.Log(doggo.name + ": Entering Pause State");
        doggo.StartCoroutine(doggo.Pause(3.0f));
        return null;
    }

    public override Type StateExit()
    {
        Debug.Log(doggo.name + ": Exiting Pause State");
        return null;
    }

    public override Type StateUpdate()
    {
        if (doggo.m_waiting || doggo.m_needsToFinishAnim)
        {
            return typeof(Pause);
        }
        else
        {
            return typeof(Idle);
        }
    }
}
