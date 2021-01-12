using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : State
{
    protected Dog doggo;

    //Called in the Chuck script to set this state's reference to itself.
    public Pause(Dog subjectDog)
    {
        doggo = subjectDog;
    }

    public override Type StateEnter()
    {
        Debug.Log("Entering Pause State");
        return null;
    }

    public override Type StateExit()
    {
        Debug.Log("Exiting Pause State");
        return null;
    }

    public override Type StateUpdate()
    {
        if (doggo.Waiting())
        {
            doggo.StartCoroutine(doggo.Pause(10.0f));
            return null;
        }
        else
        {
            return typeof(Idle);
        }


        //return null;
    }
}
