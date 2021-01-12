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
        return null;
    }

    public override Type StateUpdate()
    {
        if (doggo.Tired() && (doggo.UsingItemFor() != "Rest"))
        {
            if (doggo.AttemptToUseItem(ItemType.BED))
            {
                doggo.StartCoroutine(doggo.UseItem(30.0f, true));
                return null;
            }
        }
        else if (doggo.Hungry())
        {
            return typeof(Hungry);
        }
        else
        {
            return typeof(Idle);
        }

        return null;
    }
}
