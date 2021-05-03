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
        Debug.Log(doggo.name + ": Entering Hungry State");
        return null;
    }

    public override Type StateExit()
    {
        Debug.Log(doggo.name + ": Exiting Hungry State");

        if (doggo.m_usingItem)
        {
            doggo.EndItemUse();
        } 

        return null;
    }

    public override Type StateUpdate()
    {
        //if ((doggo.Hungry() || !doggo.Fed() ) && doggo.ItemOfTypeFound(ItemType.FOOD))
        //{
        //    if (!doggo.TargetItemIsFor(DogCareValue.Hunger))
        //    {
        //        if (doggo.LocateItemType(ItemType.FOOD))
        //        {
        //            doggo.StartCoroutine(doggo.UseItem());
        //            return null;
        //        }
        //    }
        //    else { doggo.StartCoroutine(doggo.UseItem()); }
        //}
         if (doggo.Tired())
        {
            return typeof(Tired);
        }
        else 
        {
            return typeof(Idle);
        }
    }
}