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

        if (doggo.UsingItemFor() != null)
        {
            doggo.EndItemUse();
            doggo.StopAllCoroutines();
        }

        return null;
    }

    public override Type StateUpdate()
    {
        if (!doggo.Tired() && doggo.Hungry() && (doggo.FindItem(ItemType.BOWL) != null))
        {
            return typeof(Hungry);
        }
        else if ((doggo.Tired() || !doggo.Rested()) && (doggo.FindItem(ItemType.BED) != null))
        {
            if (doggo.UsingItemFor() != DogCareValue.Rest)
            {
                if (doggo.AttemptToUseItem(ItemType.BED))
                {
                    doggo.StartCoroutine(doggo.UseItem(20.0f));
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        else if (doggo.Rested() || (doggo.FindItem(ItemType.BED) == null))
        {
            return typeof(Idle);
        }

        return null;
    }
}
