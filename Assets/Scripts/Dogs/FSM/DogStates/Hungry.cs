using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hungry : State
{
    protected Dog doggo;

    //Called in the Chuck script to set this state's reference to itself.
    public Hungry(Dog subjectDog)
    {
        doggo = subjectDog;
    }

    public override Type StateEnter()
    {
        Debug.Log("Entering Hungry State");
        return null;
    }

    public override Type StateExit()
    {
        Debug.Log("Exiting Hungry State");

        if (!doggo.UsingItemFor(DogCareValue.NONE))
        {
            doggo.EndItemUse();
            doggo.StopAllCoroutines();
        } 

        return null;
    }

    public override Type StateUpdate()
    {
        if ((doggo.Hungry() || !doggo.Full()) && (doggo.FindItem(ItemType.BOWL) != null))
        {
            if (!doggo.UsingItemFor(DogCareValue.Hunger))
            {
                if (doggo.AttemptToUseItem(ItemType.BOWL))
                {
                    doggo.StartCoroutine(doggo.UseItem(5.0f));
                    return null;
                }
            }
        }
        else if (doggo.Full() && doggo.Tired())
        {
            return typeof(Tired);
        }
        else 
        {
            return typeof(Idle);
        }



        //if ((doggo.Hungry() || (!doggo.Full() && !doggo.Tired())) && (doggo.FindItem(ItemType.BOWL) != null))
        //{
        //    if (doggo.UsingItemFor() != "Hunger")
        //    {
        //        if (doggo.AttemptToUseItem(ItemType.BOWL))
        //        {
        //            doggo.StartCoroutine(doggo.UseItem(5.0f));
        //            return null;
        //        }
        //    }
        //}
        //else 
        //{
        //    return typeof(Idle);
        //}

        return null;
    }
}