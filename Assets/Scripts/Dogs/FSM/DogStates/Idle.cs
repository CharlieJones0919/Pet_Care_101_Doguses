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
        Debug.Log(doggo.name + ": Entering Idle State");
        return null;
    }

    public override Type StateExit()
    {
        Debug.Log(doggo.name + ": Exiting Idle State");
        doggo.ClearCurrentTarget();
        return null;
    }

    public override Type StateUpdate()
    {
        //if ((doggo.Hungry() || doggo.Starving()) && doggo.ItemOfTypeFound(ItemType.FOOD))
        //{
        //    return typeof(Hungry);
        //}
        //else 
        if (!doggo.Rested() && doggo.FindPathToItem(ItemType.BED))
        {
            return typeof(Tired);
        }

        doggo.Wander();
        return null;
    }
}
