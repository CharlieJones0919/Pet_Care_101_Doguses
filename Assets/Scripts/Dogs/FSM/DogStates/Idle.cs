using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : State
{
    protected Dog doggo;

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
        if (doggo.Hungry() && doggo.FindItemType(ItemType.FOOD))
        {
            return typeof(Hungry);
        }
        else if (doggo.Tired() && doggo.FindItemType(ItemType.BED))
        {
            return typeof(Tired);
        }
        else if (doggo.Lonely() && doggo.FindItemType(ItemType.TOYS))
        {
            return typeof(Playful);
        }

        doggo.Wander();
        return null;
    }
}
