using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playful : State
{
    protected Dog doggo;

    public Playful(Dog subjectDog) { doggo = subjectDog; }

    public override Type StateEnter()
    {
#if UNITY_EDITOR
        if (doggo.m_isFocusDog) Debug.Log(doggo.name + ": Entering Playful State");
#endif
        return null;
    }

    public override Type StateExit()
    {
#if UNITY_EDITOR
        if (doggo.m_isFocusDog) Debug.Log(doggo.name + ": Exiting Playful State");
#endif
        if (doggo.m_usingItem)
        {
            doggo.SetRunning(false);
            doggo.EndItemUse();
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
                    doggo.PickUpTarget();
                    doggo.SetRunning(true);
                }
            }
            else { doggo.Wander(); }
        }
        else {  return typeof(Idle);  }

        return null;
    }
}