using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType { BOWL, BED };

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

    [SerializeField] private GameObject bowlPrefab;
    [SerializeField] private GameObject bedPrefab;

    private class ItemPool
    {
        public ItemType type;
        private GameObject prefabRef;
        private GameObject destinationRef;
        private List<Vector3> allowedPositions;
        public List<Item> itemList;

        public ItemPool(ItemType iType, GameObject prefab, GameObject parentObj, List<Vector3> spawnPositions)
        {
            type = iType;
            prefabRef = prefab;
            destinationRef = new GameObject(type.ToString() + " OBJECTS");
            destinationRef.transform.parent = parentObj.transform;
            allowedPositions = spawnPositions;
            itemList = new List<Item>();
        }

        public void InstantiateNewToList()
        {
            string propertySubject = null;
            Vector3 foundFreePos = Vector3.zero;
            bool singleUse = false;
            bool centrePref = false;

            foreach (Vector3 position in allowedPositions)
            {
                foundFreePos = position;

                switch (type)
                {
                    case (ItemType.BOWL):
                        propertySubject = "Hunger";
                        singleUse = true;
                        centrePref = false;
                        break;
                    case (ItemType.BED):
                        propertySubject = "Rest";
                        centrePref = true;
                        break; 
                }

                itemList.Add(new Item(prefabRef, destinationRef, foundFreePos, "GENERIC " + type.ToString(), 1.00f, "Desc.", 0.05f, 0.015f, propertySubject, singleUse, centrePref));
                break;
            }

            if ((propertySubject != null))
            {
                allowedPositions.Remove(foundFreePos);
                return;
            }
            Debug.Log("Maximum Number of " + type.ToString() + "'s Already Placed");
        }
    };

    List<ItemPool> itemPool = new List<ItemPool>();

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

        ////////////////////////////// Object Pool Testing //////////////////////////////

        float yHeight = bowlPrefab.transform.localScale.y / 2.0f;

        List<Vector3> tempBowlPositions = new List<Vector3>();
        tempBowlPositions.Add(new Vector3(-20.0f, yHeight, 20.0f));
        tempBowlPositions.Add(new Vector3(-5.0f, yHeight, -10.0f));

        yHeight = bedPrefab.transform.localScale.y / 2.0f;
        List<Vector3> tempBedPositions = new List<Vector3>();
        tempBedPositions.Add(new Vector3(20.0f, yHeight, 10.0f));


        itemPool.Add(new ItemPool(ItemType.BOWL, bowlPrefab, gameObject, tempBowlPositions));
        itemPool.Add(new ItemPool(ItemType.BED, bedPrefab, gameObject, tempBedPositions));

        foreach (ItemPool pool in itemPool)
        {
            pool.InstantiateNewToList();
            pool.InstantiateNewToList();
        }
    }

    public void InitializeCareProperties(List<Property> propertyListRef)
    {    
        propertyListRef.Add(new Property("Hunger", hungerStates, -0.01f)) ;
        propertyListRef.Add(new Property("Attention", attentionStates, -0.01f));
        propertyListRef.Add(new Property("Rest", restStates, -0.01f));
        propertyListRef.Add(new Property("Hygiene", hygieneStates, -0.01f));
        propertyListRef.Add(new Property("Health", healthStates, -0.01f));
        propertyListRef.Add(new Property("Happiness", happinessStates, -0.01f));
        propertyListRef.Add(new Property("Bond", bondStates, 0.0f));
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

    public List<Item> GetActiveObjects(ItemType type)
    {
        foreach (ItemPool pool in itemPool)
        {
            if (pool.type == type)
            {
                return pool.itemList;
            }
        }
        Debug.Log("No Objects of that Type Available");
        return null;
    }
}

public class GameTime
{
    public Text dateTextbox;
    public Text timeTextbox;

    [SerializeField] private const int timeAdjustment = 72; // 20 minutes of real time is 1 day in game time.
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

    public float getSecondMultiplier() { return timeAdjustment; }
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

