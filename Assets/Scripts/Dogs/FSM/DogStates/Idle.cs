﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : State
{
    protected Dog doggo;

    //Called in the Chuck script to set this state's reference to itself.
    public Idle(Dog subjectDog)
    {
        doggo = subjectDog;
    }

    public override Type StateEnter()
    {
        Debug.Log("Entering Idle State");
        return null;
    }

    public override Type StateExit()
    {
        Debug.Log("Exiting Idle State");
        return null;
    }

    public override Type StateUpdate()
    {
        if (doggo.Hungry() && doggo.ItemOfTypeFound(ItemType.FOOD))
        {
            return typeof(Hungry);
        }
        else if (doggo.Tired() && doggo.ItemOfTypeFound(ItemType.BED))
        {
            return typeof(Tired);
        }

        doggo.Wander();
        return null;
    }
}
