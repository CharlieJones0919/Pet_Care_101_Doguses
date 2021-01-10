using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        dateTextbox.text = String.Format("Week: {0:D1}          Days: {1:D1}", gameTimeWeeks, gameTimeDays);
        timeTextbox.text = String.Format("Time: [ {0:D2}:{1:D2} ]", gameTimeHours, gameTimeMinutes);
    }

    public float GetGameTimeSeconds() { return gameTimeSeconds; }
    public int GetGameTimeMinutes() { return gameTimeMinutes; }
    public int GetGameTimeHours() { return gameTimeHours; }
    public int GetGameTimeDays() { return gameTimeDays; }
    public int GetGameTimeWeeks() { return gameTimeWeeks; }
}

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

    public void InitializeCareProperties(Dictionary<Property, float> propertyListRef)
    {    
        propertyListRef.Add(new Property("Hunger", hungerStates), 5.0f) ;
        propertyListRef.Add(new Property("Attention", attentionStates), 1.0f);
        propertyListRef.Add(new Property("Rest", restStates), 1.0f);
        propertyListRef.Add(new Property("Hygiene", hygieneStates), 1.0f);
        propertyListRef.Add(new Property("Health", healthStates), 1.0f);
        propertyListRef.Add(new Property("Happiness", happinessStates), 1.0f);
        propertyListRef.Add(new Property("Bond", bondStates), 1.0f);
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

    public void UpdateValue(float amount) { m_value = Mathf.Clamp(m_value + amount * Time.deltaTime, 0.0f, 100.0f); }
}