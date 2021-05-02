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
        Debug.Log(doggo.name + ": Entering: Tired State");
        return null;
    }

    public override Type StateExit()
    {
        Debug.Log(doggo.name + ": Exiting Tired State");

        if (doggo.UsingItemFor(DogCareValue.Rest))
        {
            doggo.m_animationCTRL.SetTrigger("WakingUp");
            doggo.EndItemUse();
            doggo.StopCoroutine(doggo.UseItem());
        }

        return null;
    }

    public override Type StateUpdate()
    {
        if (((!doggo.Tired() && (doggo.Starving() || doggo.Hungry()) && doggo.ItemOfTypeFound(ItemType.FOOD))) || doggo.Rejuvinated())
        {
            return typeof(Pause);
        }
        else if ((doggo.Tired() || !doggo.Rested()) && doggo.ItemOfTypeFound(ItemType.BED))
        {
            if (!doggo.UsingItemFor(DogCareValue.Rest))
            {
                if (doggo.LocateItemFor(ItemType.BED))
                {
                    doggo.StartCoroutine(doggo.UseItem());
                    doggo.m_animationCTRL.SetTrigger("GoingToSleep");
                    return null;
                }
            }
        }

        return null;
    }
}
