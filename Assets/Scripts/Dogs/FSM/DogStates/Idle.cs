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
#if UNITY_EDITOR
        if (doggo.m_isFocusDog) Debug.Log(doggo.name + ": Entering Idle State");
#endif 
        return null;
    }

    public override Type StateExit()
    {
#if UNITY_EDITOR
        if (doggo.m_isFocusDog) Debug.Log(doggo.name + ": Exiting Idle State");
#endif
        return null;
    }

    public override Type StateUpdate()
    {
        //if (doggo.beingInteractedWith) { return typeof(Pause); }

        //if (doggo.Hungry() && doggo.FindItemType(ItemType.FOOD))
        //{
        //    return typeof(Hungry);
        //}
        //else if (doggo.Tired() && doggo.FindItemType(ItemType.BED))
        //{
        //    return typeof(Tired);
        //}
        //else 
        if (doggo.Lonely() && doggo.FindItemType(ItemType.TOYS))
        {
            return typeof(Playful);
        }

        doggo.Wander();
        return null;
    }
}