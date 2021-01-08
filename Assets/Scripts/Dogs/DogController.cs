using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogController : MonoBehaviour
{
    List<string> hungerStates = new List<string>();
    List<string> attentionStates = new List<string>();
    List<string> restStates = new List<string>();
    List<string> hygieneStates = new List<string>();
    List<string> healthStates = new List<string>();
    List<string> happinessStates = new List<string>();
    List<string> bondStates = new List<string>();

    List<string> toleranceStates = new List<string>();
    List<string> affectionStates = new List<string>();
    List<string> intelligenceStates = new List<string>();
    List<string> energyStates = new List<string>();
    List<string> obedienceStates = new List<string>();

    public List<GameObject> objectList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        ////////////////////////////// Care Property Values //////////////////////////////
        hungerStates.Add("Starving");
        hungerStates.Add("Fed");
        hungerStates.Add("Overfed");

        attentionStates.Add("Lonely");
        attentionStates.Add("Loved");
        attentionStates.Add("Overcrowded");

        restStates.Add("Exhausted");
        restStates.Add("Tired");
        restStates.Add("Rested");

        hygieneStates.Add("Filthy");
        hygieneStates.Add("Dirty");
        hygieneStates.Add("Clean");

        healthStates.Add("Dying");
        healthStates.Add("Sick");
        healthStates.Add("Good");

        happinessStates.Add("Distressed");
        happinessStates.Add("Upset");
        happinessStates.Add("Happy");

        bondStates.Add("Wary");
        bondStates.Add("Friendly");

        ////////////////////////////// Personality Property Values //////////////////////////////

        toleranceStates.Add("Nervous");
        toleranceStates.Add("Neutral");
        toleranceStates.Add("Calm");

        affectionStates.Add("Aggressive");
        affectionStates.Add("Grouchy");
        affectionStates.Add("Apathetic");
        affectionStates.Add("Friendly");
        affectionStates.Add("Loving");

        intelligenceStates.Add("Dumb");
        intelligenceStates.Add("Average");
        intelligenceStates.Add("Smart");

        energyStates.Add("Sleepy");
        energyStates.Add("Normal");
        energyStates.Add("Hyper");

        obedienceStates.Add("Bad");
        obedienceStates.Add("Good");
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void InitializeCareProperties(List<Property> propertyListRef)
    {    
        propertyListRef.Add(new Property("Hunger", hungerStates));
        propertyListRef.Add(new Property("Attention", attentionStates));
        propertyListRef.Add(new Property("Rest", restStates));
        propertyListRef.Add(new Property("Hygiene", hygieneStates));
        propertyListRef.Add(new Property("Health", healthStates));
        propertyListRef.Add(new Property("Happiness", happinessStates));
        propertyListRef.Add(new Property("Bond", bondStates));
    }

    public void InitializePersonalityProperties(List<Property> propertyListRef)
    {
        propertyListRef.Add(new Property("Tolerance", toleranceStates));
        propertyListRef.Add(new Property("Affection", affectionStates));
        propertyListRef.Add(new Property("Intelligence", intelligenceStates));
        propertyListRef.Add(new Property("Energy", energyStates));
        propertyListRef.Add(new Property("Obedience", obedienceStates));
    }
}

public class Property
{
    private string m_property;
    private float m_value = 5;
    private Dictionary<string, bool> m_states = new Dictionary<string, bool>();

    public Property(string property, List<string> states)
    {
        m_property = property;

        foreach (string newState in states)
        {
            m_states.Add(newState, false);
        }
    }

    public string GetPropertyName() { return m_property; }
    public float GetValue() { return m_value; }
    public void SetValue(float value) { m_value = value; }
}