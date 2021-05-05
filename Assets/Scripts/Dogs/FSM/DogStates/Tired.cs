using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tired : State
{
    protected Dog doggo;

    public Tired(Dog subjectDog) { doggo = subjectDog; }

    public override Type StateEnter()
    {
#if UNITY_EDITOR
        if (doggo.m_isFocusDog) Debug.Log(doggo.name + ": Entering Tired State");
#endif
        return null;
    }

    public override Type StateExit()
    {
#if UNITY_EDITOR
        if (doggo.m_isFocusDog) Debug.Log(doggo.name + ": Exiting Tired State");
#endif
        if (doggo.m_usingItem)
        {
            doggo.m_animationCTRL.SetTrigger("WakingUp");
            doggo.EndItemUse();
        }
        return null;
    }

    public override Type StateUpdate()
    {
        if (!doggo.Rejuvinated() && (doggo.FindItemType(ItemType.BED)))
        {
            if (!doggo.m_usingItem)
            {
                if (doggo.ReachedTarget())
                {
                    doggo.m_animationCTRL.SetTrigger("GoingToSleep");
                    doggo.UseItem();
                }
            }
        }
        else { return typeof(Pause); }

        return null;
    }
}