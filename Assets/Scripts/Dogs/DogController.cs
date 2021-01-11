using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DogController : MonoBehaviour
{
    public GameTime gameTime = new GameTime();

    private List<string> hungerStates = new List<string>();
    private List<string> attentionStates = new List<string>();
    private List<string> restStates = new List<string>();
    private List<string> hygieneStates = new List<string>();
    private List<string> healthStates = new List<string>();
    private List<string> happinessStates = new List<string>();
    private List<string> bondStates = new List<string>();

    private List<string> toleranceStates = new List<string>();
    private List<string> affectionStates = new List<string>();
    private List<string> intelligenceStates = new List<string>();
    private List<string> energyStates = new List<string>();
    private List<string> obedienceStates = new List<string>();

    private List<GameObject> objectsForDeletion = new List<GameObject>();

    [SerializeField] private GameObject bowlPrefabRef;
    private GameObject bowlObjectDestination;
    private List<Vector3> bowlPositions = new List<Vector3>();
    private List<Consumable> foodBowls = new List<Consumable>();

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

        ////////////////////////////// Food Bowl Objects //////////////////////////////

        bowlObjectDestination = new GameObject("FoodBowls");
        bowlObjectDestination.transform.parent = transform;
        bowlPositions.Add(new Vector3(-20.0f, 0.125f, 20.0f));
        bowlPositions.Add(new Vector3(-5.0f, 0.125f, -10.0f));

        InstantiateFoodBowl();
        InstantiateFoodBowl();
    }

    public void InitializeCareProperties(List<Property> propertyListRef)
    {    
        propertyListRef.Add(new Property("Hunger", hungerStates, -0.015f)) ;
        propertyListRef.Add(new Property("Attention", attentionStates, -1.0f));
        propertyListRef.Add(new Property("Rest", restStates, -1.0f));
        propertyListRef.Add(new Property("Hygiene", hygieneStates, -1.0f));
        propertyListRef.Add(new Property("Health", healthStates, -1.0f));
        propertyListRef.Add(new Property("Happiness", happinessStates, -1.0f));
        propertyListRef.Add(new Property("Bond", bondStates, -1.0f));
    }

    public void InitializePersonalityProperties(List<Property> propertyListRef)
    {
        propertyListRef.Add(new Property("Tolerance", toleranceStates));
        propertyListRef.Add(new Property("Affection", affectionStates));
        propertyListRef.Add(new Property("Intelligence", intelligenceStates));
        propertyListRef.Add(new Property("Energy", energyStates));
        propertyListRef.Add(new Property("Obedience", obedienceStates));
    }

    private void Update()
    {
        gameTime.Update();
    }

    private void InstantiateFoodBowl()
    {
        Vector3 foundFreePos = Vector3.zero;

        foreach (Vector3 position in bowlPositions)
        {
            foundFreePos = position;
            foodBowls.Add(new Consumable(bowlPrefabRef, bowlObjectDestination, foundFreePos, "GenericFood", 1.00f, "Desc.", 0.05f, 5.0f));
            break;
        }

        if (foundFreePos != Vector3.zero)
        {
            bowlPositions.Remove(foundFreePos);
            return;
        }

        Debug.Log("Maximum Number of Food Bowls Already Placed");
    }

    public List<Consumable> GetActiveBowlObjects()
    {
        return foodBowls;
    }
}

public class GameTime
{
    public Text dateTextbox;
    public Text timeTextbox;

    [SerializeField] private const int timeAdjustment = 72; // 15 minutes of real time is 1 day in game time.
    [SerializeField] private float gameTimeSeconds;
    [SerializeField] private int gameTimeMinutes;
    [SerializeField] private int gameTimeHours;
    [SerializeField] private int gameTimeDays;
    [SerializeField] private int gameTimeWeeks;

    private void Start()
    {
        //Read previous game time from constant external save file.
    }

    public void Update()
    {
        gameTimeSeconds += Time.deltaTime * timeAdjustment;

        if (gameTimeSeconds >= 60)
        {
            gameTimeSeconds = 0.0f;
            gameTimeMinutes++;

            if (gameTimeMinutes >= 60)
            {
                gameTimeMinutes = 0;
                gameTimeHours++;

                if (gameTimeHours >= 24)
                {
                    gameTimeHours = 0;
                    gameTimeDays++;

                    if (gameTimeDays >= 7)
                    {
                        gameTimeDays = 0;
                        gameTimeWeeks++;
                    }
                }
            }
        }

        dateTextbox.text = String.Format("Week: {0:D1}     Days: {1:D1}", gameTimeWeeks, gameTimeDays);
        timeTextbox.text = String.Format("Time: [ {0:D2}:{1:D2} ]", gameTimeHours, gameTimeMinutes);
    }

    public float GetGameTimeSeconds() { return gameTimeSeconds; }
    public int GetGameTimeMinutes() { return gameTimeMinutes; }
    public int GetGameTimeHours() { return gameTimeHours; }
    public int GetGameTimeDays() { return gameTimeDays; }
    public int GetGameTimeWeeks() { return gameTimeWeeks; }
}

public class Property
{
    private string m_propertyName;
    private float m_value = 50;
    private float m_increment = 0;
    private Dictionary<string, bool> m_states = new Dictionary<string, bool>();

    public Property(string name, List<string> states, float increment)
    {
        m_propertyName = name;

        foreach (string newState in states)
        {
            m_states.Add(newState, false);
        }

        m_increment = increment;
    }

    public Property(string name, List<string> states)
    {
        m_propertyName = name;

        foreach (string newState in states)
        {
            m_states.Add(newState, false);
        }
    }

    public string GetPropertyName() { return m_propertyName; }
    public float GetValue() { return m_value; }
    public float GetIncrement() { return m_increment; }

    public void UpdateValue(float amount) { m_value = Mathf.Clamp(m_value + amount, 0.0f, 100.0f); }
}