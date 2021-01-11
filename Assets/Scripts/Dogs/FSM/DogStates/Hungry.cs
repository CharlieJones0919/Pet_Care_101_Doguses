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
        Debug.Log("AAAAAAAAAAA");
        return null;
    }

    public override Type StateExit()
    {
        return null;
    }

    public override Type StateUpdate()
    {
        if ((doggo.Hungry()/* || !doggo.Full()*/) && !doggo.Eating())
        {
            if (doggo.AttemptToEat())
            {
                return null;
            }
        }

        return null;
    }
}