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
        return null;
    }

    public override Type StateExit()
    {
        return null;
    }

    public override Type StateUpdate()
    {
        //doggo.UpdateProperties();

        //if (doggo.CheckFact()) 
        //{
        //    return typeof(NewState);
        //}
        //else
        //{
        //    return null;
        //}
        return null;
    }
}