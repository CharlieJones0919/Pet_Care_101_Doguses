using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playful : State
{
    protected Dog doggo;

    public Playful(Dog subjectDog)
    {
        doggo = subjectDog;
    }

    public override Type StateEnter()
    {
        doggo.currentState = "Playful State";
        return null;
    }

    public override Type StateExit()
    {
        if (doggo.m_usingItem)
        {
            doggo.EndItemUse();
           // doggo.StopPlaying();
        }
        return null;
    }

    public override Type StateUpdate()
    {
        if ((!doggo.Loved() || !doggo.Happy()) && doggo.FindItemType(ItemType.TOYS))
        {
            if (!doggo.m_usingItem)
            {
                if (doggo.ReachedTarget())
                {
                    doggo.UseItem();
                }
            }
            else { doggo.UseItem(); doggo.Play(); }
        }
        else { return typeof(Idle); }

        return null;
    }
}