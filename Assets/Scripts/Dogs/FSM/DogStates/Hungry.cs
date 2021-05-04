using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hungry : State
{
    protected Dog doggo;

    //Called in the Chuck script to set this state's reference to itself.
    public Hungry(Dog subjectDog)
    {
        doggo = subjectDog;
    }

    public override Type StateEnter()
    {
        doggo.currentState = "Hungry State";
        return null;
    }

    public override Type StateExit()
    {
        if (doggo.m_usingItem)
        {
            doggo.EndItemUse();
        } 

        return null;
    }

    public override Type StateUpdate()
    {
        if (!doggo.Overfed() && doggo.FindItemType(ItemType.FOOD))
        {
            if (!doggo.m_usingItem)
            {
                if (doggo.ReachedTarget())
                {
                    doggo.m_animationCTRL.SetTrigger("Eating");
                }
            }
            else { doggo.UseItem(); }
        }
        else 
        {
            return typeof(Idle);
        }

        return null;
    }
}