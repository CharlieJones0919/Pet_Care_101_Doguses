using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tired : State
{
    protected Dog doggo;

    //Called in the Chuck script to set this state's reference to itself.
    public Tired(Dog subjectDog)
    {
        doggo = subjectDog;
    }

    public override Type StateEnter()
    {
        doggo.currentState = "Tired State";
        return null;
    }

    public override Type StateExit()
    {
        if (doggo.m_usingItem)
        {
            doggo.m_animationCTRL.SetTrigger("WakingUp");
            doggo.EndItemUse();
        }

        return null;
    }

    public override Type StateUpdate()
    {
        //if (((!doggo.Tired() && doggo.Hungry()) && doggo.ItemOfTypeFound(ItemType.FOOD))
        //    || ((!doggo.Exhausted() && doggo.Starving()) && doggo.ItemOfTypeFound(ItemType.FOOD))
        //    || doggo.Rejuvinated())
        //{
        //    return typeof(Pause);
        //}
        if (!doggo.Rejuvinated() && doggo.FindItemType(ItemType.BED))
        {
            if (!doggo.m_usingItem)
            {
                if (doggo.ReachedTarget())
                {
                    doggo.m_animationCTRL.SetTrigger("GoingToSleep");
                }
            }
            else { doggo.UseItem(); }
        }
        else { return typeof(Pause); }

        return null;
    }
}
