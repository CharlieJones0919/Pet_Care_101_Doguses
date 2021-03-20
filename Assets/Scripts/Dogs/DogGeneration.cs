using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System.IO;
using System.Linq;

public enum DogCareValue
{
   NONE, Hunger, Attention, Rest, Hygiene, Health, Happiness, Bond
}

public enum DogPersonalityValue
{
   NONE, Obedience, Tolerance, Affection, Intelligence, Energy
}

public enum DogBreed
{
    NONE,
    French_Bulldog,
    German_Shepard,
    Labrador,
    Shih_Tzu,
    Beagle,
    Bulldog,
    Rottweiler,
    Welsh_Corgi,
    Dachshund,
    Yorkshire_Terrier,
    Siberian_Husky,
    Pomeranian,
    Scottish_Terrier,
    Pug,
    Chihuahua,
    Whippet,
    Basset_Hound,
    Springer_Spaniel,
    Icelandic_Sheepdog,
    Pekingese,
    Saint_Bernard,
    Xoloitzcuintli,
    Great_Dane,
    Irish_Wolfhound,
    Shiba_Inu
}

public enum DogDataField
{
    Max_Age,
    Breed,
    Affection,
    Tolerance,
    Intelligence,
    Energy,
    Obedience,
    Coat_Kind,
    Grooming_Diff,
    Size,
    Body_Length,
    Leg_Length,
    Ear_Kind,
    Ear_Orientation,
    Snout_Kind,
    Snout_Length,
    Tail_Kind,
    Tail_Orientation
}

public class DogGeneration : MonoBehaviour
{
    private Dictionary<DogCareValue, Dictionary<string, Vector2>> careValueStates = new Dictionary<DogCareValue, Dictionary<string, Vector2>>();
    private Dictionary<DogPersonalityValue, Dictionary<string, Vector2>> personalityValueStates = new Dictionary<DogPersonalityValue, Dictionary<string, Vector2>>();

    public GameObject dogPrefabBase;
    public List<GameObject> currentDogs;
    private DogController dogController;

    [SerializeField] private GameObject groundObject;
    [SerializeField] private GameObject dogInfoPanel;

    private string breedDataFileDir = "/Scripts/Dogs/ReferenceFiles/BreedData.txt";
    private DataTable breedData = new DataTable();
    private int numDataFields = 0;
    private int numDogBreedsDefined = 0;

    private void Start()
    {
        dogController = GetComponent<DogController>();

        string[] data = System.IO.File.ReadAllLines(Application.dataPath + breedDataFileDir);
        for (int i = 0; i < data.Length; i++) //Remove Blank Spaces from Data
        {
            data[i] = string.Concat(data[i].Where(c => !char.IsWhiteSpace(c)));
        }

        numDataFields = DogDataField.GetNames(typeof(DogDataField)).Length;
        numDogBreedsDefined = data.Length;

        breedData.Clear();

        string[] headers = new string[numDataFields];
        headers = data[0].Split('?');
        foreach (string column in headers) { breedData.Columns.Add(column); }

        for (int i = 1; i < numDogBreedsDefined; i++)
        {
            string[] field = new string[numDataFields];
            field = data[i].Split('?');

            DataRow newData = breedData.NewRow();
            for (int j = 0; j < numDataFields; j++)
            {
                newData[breedData.Columns[j].ToString()] = field[j];
            }
            breedData.Rows.Add(newData);
        }

        DefineDogPropStates();
       
    }

    private string GetBreedValue(DogBreed breed, DogDataField dataField)
    {
        for (int i = 0; i < numDogBreedsDefined; i++)
        {
            if (breed.ToString() == breedData.Rows[i]["Breed"].ToString())
            {
                return breedData.Rows[i][dataField.ToString()].ToString();
            }
        }

        Debug.Log("Requested data for that breed cannot be found: " + breed.ToString() + " " + dataField.ToString());
        return null;
    }

    private void DefineDogPropStates()
    {
        //////////////////// Care Value States ////////////////////
        string[] hungerStateNames = { "Starving", "Fed", "Overfed" };
        Vector2[] hungerStateRanges = { new Vector2(0,20), new Vector2(20,85), new Vector2(85,100) };

        string[] attentionStateNames = { "Lonely", "Loved", "Overcrowded" };
        Vector2[] attentionStateRanges = { new Vector2(0,50), new Vector2(50,95), new Vector2(95,100) };

        string[] restStateNames = { "Exhausted", "Tired", "Rested" };
        Vector2[] restStateRanges = { new Vector2(0,15), new Vector2(15,50), new Vector2(50,100) };

        string[] hygieneStateNames = { "Filthy", "Dirty", "Clean" };
        Vector2[] hygieneStateRanges = { new Vector2(0, 20), new Vector2(20, 60), new Vector2(60, 100) };

        string[] healthStateNames = { "Dying", "Sick", "Good" };
        Vector2[] healthStateRanges = { new Vector2(0, 10), new Vector2(10, 35), new Vector2(35, 100) };

        string[] happinessStateNames = { "Distressed", "Upset", "Happy" };
        Vector2[] happinessStateRanges = { new Vector2(0, 15), new Vector2(15, 40), new Vector2(40, 100) };

        string[] bondStateNames = { "Wary", "Friendly" };
        Vector2[] bondStateRanges = { new Vector2(0, 50), new Vector2(50, 100) };

        AddStatesToProperty(DogCareValue.Hunger, hungerStateNames, hungerStateRanges);
        AddStatesToProperty(DogCareValue.Attention, attentionStateNames, attentionStateRanges);
        AddStatesToProperty(DogCareValue.Rest, restStateNames, restStateRanges);
        AddStatesToProperty(DogCareValue.Hygiene, hygieneStateNames, hygieneStateRanges);
        AddStatesToProperty(DogCareValue.Health, healthStateNames, healthStateRanges);
        AddStatesToProperty(DogCareValue.Happiness, happinessStateNames, happinessStateRanges);
        AddStatesToProperty(DogCareValue.Bond, bondStateNames, bondStateRanges);

        //////////////////// Personality Value States ////////////////////
        string[] obedienceStateNames = { "Bad", "Average", "Good" };
        Vector2[] obedienceStateRanges = { new Vector2(0.0f,2.5f), new Vector2(2.5f,3.5f), new Vector2(3.5f,5.0f) };
       
        string[] affectionStates = { "Aggressive", "Grouchy", "Apathetic", "Friendly", "Loving" };
        Vector2[] affectionStateRanges = { new Vector2(0.0f, 1.0f), new Vector2(1.0f, 2.0f), new Vector2(2.0f, 3.0f), new Vector2(3.0f, 4.0f), new Vector2(4.0f, 5.0f) };

        string[] toleranceStates = { "Nervous", "Neutral", "Calm" };
        Vector2[] toleranceStateRanges = { new Vector2(0.0f, 2.0f), new Vector2(2.0f, 3.0f), new Vector2(4.0f, 5.0f) };

        string[] intelligenceStates = { "Dumb", "Average", "Smart" };
        Vector2[] intelligenceStateRanges = { new Vector2(0.0f, 2.5f), new Vector2(2.5f, 4.0f), new Vector2(4.0f, 5.0f) };

        string[] energyStates = { "Sleepy", "Normal", "Hyper" };
        Vector2[] energyStateRanges = { new Vector2(0.0f, 2.25f), new Vector2(2.25f, 4.25f), new Vector2(4.25f, 5.0f) };

        AddStatesToProperty(DogPersonalityValue.Obedience, obedienceStateNames, obedienceStateRanges);
        AddStatesToProperty(DogPersonalityValue.Affection, affectionStates, affectionStateRanges);
        AddStatesToProperty(DogPersonalityValue.Tolerance, toleranceStates, toleranceStateRanges);
        AddStatesToProperty(DogPersonalityValue.Intelligence, intelligenceStates, intelligenceStateRanges);
        AddStatesToProperty(DogPersonalityValue.Energy, energyStates, energyStateRanges);
    }

    private void AddStatesToProperty(DogCareValue property, string[] names, Vector2[] ranges)
    {
        if (names.Length != ranges.Length) { Debug.Log("Inequal number of state names to ranges attempting to be added to: " + property); return; }
        Dictionary<string, Vector2> states = new Dictionary<string, Vector2>();
        for (int i = 0; i < names.Length; i++) states.Add(names[i], ranges[i]);
        careValueStates.Add(property, states);
    }

    private void AddStatesToProperty(DogPersonalityValue property, string[] names, Vector2[] ranges)
    {
        if (names.Length != ranges.Length) { Debug.Log("Inequal number of state names to ranges attempting to be added to: " + property); return; }
        Dictionary<string, Vector2> states = new Dictionary<string, Vector2>();
        for (int i = 0; i < names.Length; i++) states.Add(names[i], ranges[i]);
        personalityValueStates.Add(property, states);
    }

    public void GenerateRandomNewDog()
    {
        GameObject newDog = Instantiate(dogPrefabBase, new Vector3(0, 0, 0), Quaternion.identity);
        newDog.transform.parent = transform;
        newDog.name = "Unnamed_Dog";
        currentDogs.Add(newDog);

        Dog dogScript = newDog.GetComponent<Dog>();

        newDog.GetComponent<Pathfinding>().groundPlane = groundObject;
        dogScript.infoPanelObject = dogInfoPanel;
        dogScript.m_controller = dogController;
  
        dogScript.m_breed = (DogBreed)UnityEngine.Random.Range(0, numDogBreedsDefined);

        int maxBreedAge = int.Parse(GetBreedValue(dogScript.m_breed, DogDataField.Max_Age));
        dogScript.m_age = UnityEngine.Random.Range(1, maxBreedAge);

        DefineDogProperties(dogScript);
    }

    private void DefineDogProperties(Dog dog)
    {
        foreach (DogCareValue careValue in (DogCareValue[])DogCareValue.GetValues(typeof(DogCareValue)))
        {
            if (careValue != DogCareValue.NONE)
            dog.m_careValues.Add(new CareProperty(careValue, careValueStates[careValue], -0.01f));
        }

        foreach (DogPersonalityValue personalityValue in (DogPersonalityValue[])DogPersonalityValue.GetValues(typeof(DogPersonalityValue)))
        {
            if (personalityValue != DogPersonalityValue.NONE)
            {
                if (Enum.IsDefined(typeof(DogDataField), personalityValue.ToString()))
                {
                    float breedValueFloat = 2.5f;
                    string breedPersonalityValue = GetBreedValue(dog.m_breed, (DogDataField)Enum.Parse(typeof(DogDataField), personalityValue.ToString()));

                    if (float.TryParse(breedPersonalityValue, out breedValueFloat))
                    {
                        dog.m_personalityValues.Add(new PersonalityProperty(personalityValue, personalityValueStates[personalityValue], breedValueFloat));
                    }
                    else Debug.Log("The dog breed data file did not contain a valid float value for the personality value: " + personalityValue);
                }
                else Debug.Log(personalityValue + " is not defined as a data field in the dog data file.");
            }
        }
    }
}