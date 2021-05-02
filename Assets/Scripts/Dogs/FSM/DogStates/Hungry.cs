﻿using System;
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

        if (doggo.UsingItemFor(DogCareValue.Hunger))
        {
            doggo.EndItemUse();
            doggo.StopCoroutine(doggo.UseItem());
        } 

        return null;
    }

    public override Type StateUpdate()
    {
        if ((doggo.Hungry() || !doggo.Fed()) && doggo.ItemOfTypeFound(ItemType.FOOD))
        {
            if (!doggo.UsingItemFor(DogCareValue.Hunger))
            {
                if (doggo.LocateItemFor(ItemType.FOOD))
                {
                    doggo.StartCoroutine(doggo.UseItem());
                    return null;
                }
            }
        }
        else if (doggo.Fed() && doggo.Tired())
        {
            return typeof(Tired);
        }
        else 
        {
            return typeof(Idle);
        }

        return null;
    }
}