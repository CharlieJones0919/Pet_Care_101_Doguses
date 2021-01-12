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
        Debug.Log("Entering Hungry State");
        return null;
    }

    public override Type StateExit()
    {
        Debug.Log("Exiting Hungry State");
        return null;
    }

    public override Type StateUpdate()
    {
        if (doggo.Hungry() && (doggo.UsingItemFor() != "Hunger"))
        {
            if (doggo.AttemptToUseItem(ItemType.BOWL))
            {
                doggo.StartCoroutine(doggo.UseItem(5.0f, false));
                return null;
            }
        }
        else if (doggo.Tired())
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