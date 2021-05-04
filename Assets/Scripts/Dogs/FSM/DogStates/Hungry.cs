using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hungry : State
{
    protected Dog doggo;

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
        if (!doggo.Overfed())
        {
            if (!doggo.m_usingItem)
            {
                if (doggo.FindItemType(ItemType.FOOD))
                {
                    if (doggo.ReachedTarget())
                    {
                        doggo.UseItem();
                        doggo.m_animationCTRL.SetTrigger("Eating");
                    }
                }
            }
        }
        else 
        {
            return typeof(Pause);
        }

        return null;
    }
}