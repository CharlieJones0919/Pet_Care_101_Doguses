using System;
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
        doggo.currentState = "Idle State";
        return null;
    }

    public override Type StateExit()
    {
        return null;
    }

    public override Type StateUpdate()
    {
        if (!doggo.Fed() && doggo.FindItemType(ItemType.FOOD))
        {
            return typeof(Hungry);
        }
        else if (doggo.Tired() && doggo.FindItemType(ItemType.BED))
        {
            return typeof(Tired);
        }

        doggo.Wander();
        return null;
    }
}
