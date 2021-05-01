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
        Debug.Log("Entering Tired State");
        return null;
    }

    public override Type StateExit()
    {
        Debug.Log("Exiting Tired State");

        if (!doggo.UsingItemFor(DogCareValue.NONE) && !doggo.UsingItemFor(DogPersonalityValue.NONE))
        {
            doggo.EndItemUse();
            doggo.StopAllCoroutines();
        }

        return null;
    }

    public override Type StateUpdate()
    {
        if (!doggo.Tired() && doggo.Hungry() && doggo.ItemOfTypeFound(ItemType.FOOD))
        {
            return typeof(Hungry);
        }
        else if ((doggo.Tired() || !doggo.Rested()) && doggo.ItemOfTypeFound(ItemType.BED))
        {
            if (!doggo.UsingItemFor(DogCareValue.Rest))
            {
                if (doggo.LocateItemFor(ItemType.BED))
                {
                    doggo.StartCoroutine(doggo.UseItem());
               //     doggo.m_animationCTRL.SetBool("GoingToSleep", true);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        else if (doggo.Rested())
        {
            return typeof(Idle);
        }

        return null;
    }
}
