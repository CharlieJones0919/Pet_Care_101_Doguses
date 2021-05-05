using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hungry : State
{
    protected Dog doggo;

    public Hungry(Dog subjectDog) { doggo = subjectDog; }

    public override Type StateEnter()
    {
#if UNITY_EDITOR
        if (doggo.m_isFocusDog) Debug.Log(doggo.name + ": Entering Hungry State");
#endif
        return null;
    }

    public override Type StateExit()
    {
#if UNITY_EDITOR
        if (doggo.m_isFocusDog) Debug.Log(doggo.name + ": Exiting Hungry State");
#endif
        return null;
    }

    public override Type StateUpdate()
    {
        if (!doggo.Overfed() && doggo.FindItemType(ItemType.FOOD))
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
        else { return typeof(Pause); }

        return null;
    }
}