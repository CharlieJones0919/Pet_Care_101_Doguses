using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tired : State
{
    protected Dog doggo;

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
        if (!doggo.Rejuvinated())
        {
            if (!doggo.m_usingItem)
            {
                if (doggo.FindItemType(ItemType.BED))
                {
                    if (doggo.ReachedTarget())
                    {
                        doggo.m_animationCTRL.SetTrigger("GoingToSleep");
                        doggo.UseItem();
                    }
                }
                else { return typeof(Pause); } // Change to be conditional on other facts.
            }
        }
        else { return typeof(Pause); }

        return null;
    }
}
