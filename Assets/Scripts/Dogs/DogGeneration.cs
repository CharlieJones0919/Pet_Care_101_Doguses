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
    public string modelsFilePath = "/Art/3D Models/DogComponents";

    public GameObject dogPrefabBase;
    public List<GameObject> snoutMdls = new List<GameObject>();
    public List<GameObject> earMdls = new List<GameObject>();
    public List<GameObject> tailMdls = new List<GameObject>();
    private Dictionary<string, KeyValuePair<Vector3, Quaternion>> earOrientations = new Dictionary<string, KeyValuePair<Vector3, Quaternion>>();
    private Dictionary<string, KeyValuePair<Vector3, Quaternion>> mirroredEarOrientations = new Dictionary<string, KeyValuePair<Vector3, Quaternion>>();
    private Dictionary<string, float> tailOrientations = new Dictionary<string, float>();

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

        ////////////////// Read the Breed Data from the Dog Data Text File //////////////////
        string[] data = System.IO.File.ReadAllLines(Application.dataPath + breedDataFileDir);
        for (int i = 0; i < data.Length; i++) //Removes Blank Spaces from Data
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

        ////////////////// Model Component Orienations //////////////////
        {
            string[] earOrientDescriptions = { "UpFront", "UpSide", "DownFront", "DownSide" };
            Vector3[] earOrientPositions = { new Vector3(0.1f, 1.475f, 0.5f), new Vector3(0.1f, 1.475f, 0.535f), new Vector3(0.115f, 1.475f, 0.555f), new Vector3(0.125f, 1.45f, 0.55f) };
            Quaternion[] earOrientRotations = { Quaternion.Euler(0.0f, 0.0f, 0.0f), Quaternion.Euler(25.0f, 37.5f, -3.75f), Quaternion.Euler(33.25f, 31.25f, -7.75f), Quaternion.Euler(45.0f, -50.0f, -115.0f) };

            if ((earOrientDescriptions.Length == earOrientPositions.Length) && (earOrientDescriptions.Length == earOrientRotations.Length))
                for (int i = 0; i < earOrientDescriptions.Length; i++)
                {
                    earOrientations.Add(earOrientDescriptions[i], new KeyValuePair<Vector3, Quaternion>(earOrientPositions[i], earOrientRotations[i]));

                    Vector3 mirroredPos = earOrientPositions[i];
                    mirroredPos.x = -mirroredPos.x;

                    Quaternion mirroredRot = Quaternion.identity;
                    mirroredRot.Set(earOrientRotations[i].x, -earOrientRotations[i].y, -earOrientRotations[i].z, earOrientRotations[i].w);
                    mirroredEarOrientations.Add(earOrientDescriptions[i], new KeyValuePair<Vector3, Quaternion>(mirroredPos, mirroredRot));
                }
            else Debug.Log("Inequal number of ear orientation positions and rotations to defined descriptions.");
        }
        {
            string[] tailOrientDescriptions = { "Up", "Middle", "Down" };
            float[] tailOrientRotations = { 60.0f, 0.0f, -40.0f };

            if (tailOrientDescriptions.Length == tailOrientRotations.Length)
                for (int i = 0; i < tailOrientDescriptions.Length; i++) tailOrientations.Add(tailOrientDescriptions[i], tailOrientRotations[i]); 
            else Debug.Log("Inequal number of tail orientation rotations to defined descriptions.");
        }
         
        ////////////////// Initialise the States Possessed by Each Dog Property Value //////////////////
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
        Vector2[] hungerStateRanges = { new Vector2(0, 20), new Vector2(20, 85), new Vector2(85, 100) };

        string[] attentionStateNames = { "Lonely", "Loved", "Overcrowded" };
        Vector2[] attentionStateRanges = { new Vector2(0, 50), new Vector2(50, 95), new Vector2(95, 100) };

        string[] restStateNames = { "Exhausted", "Tired", "Rested" };
        Vector2[] restStateRanges = { new Vector2(0, 15), new Vector2(15, 50), new Vector2(50, 100) };

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
        Vector2[] obedienceStateRanges = { new Vector2(0.0f, 2.5f), new Vector2(2.5f, 3.5f), new Vector2(3.5f, 5.0f) };

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

    public void GenerateAllDogsLineUp()
    {
        foreach (DogBreed dogBreed in (DogBreed[])DogBreed.GetValues(typeof(DogBreed)))
        {
            if (dogBreed != DogBreed.NONE) GenerateDog(dogBreed);
        }
    }

    public void GenerateRandomNewDog()
    {     
        GenerateDog((DogBreed)UnityEngine.Random.Range(0, numDogBreedsDefined));
    }

    float firstPos = -25f;

    public void GenerateDog(DogBreed breed)
    {
        GameObject newDog = Instantiate(dogPrefabBase, dogPrefabBase.transform.position, Quaternion.identity);
        newDog.GetComponent<Pathfinding>().groundPlane = groundObject;
        newDog.transform.parent = transform.GetChild(0);
        newDog.name = "Unnamed_" + breed;

        currentDogs.Add(newDog);

        Dog dogScript = newDog.GetComponent<Dog>();
        dogScript.infoPanelObject = dogInfoPanel;
        dogScript.m_controller = dogController;

        dogScript.m_breed = breed;
        dogScript.m_age = UnityEngine.Random.Range(1, int.Parse(GetBreedValue(breed, DogDataField.Max_Age)));

        DefineDogProperties(dogScript);
        CreateDogBody(dogScript);

        newDog.transform.Translate(firstPos, 0.0f, 0.0f);
        firstPos += 2.5f;
    }

    private void DefineDogProperties(Dog dog)
    {
        foreach (DogCareValue careValue in (DogCareValue[])DogCareValue.GetValues(typeof(DogCareValue)))
        {
            if (careValue != DogCareValue.NONE) dog.m_careValues.Add(new CareProperty(careValue, careValueStates[careValue], -0.01f));
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

    private void CreateDogBody(Dog dog)
    {
        /*                              <- Waist -> <-ChestPiv-> <- Chest -> <--------- Neck ---------> <------ Head ------>           */
        GameObject head = dog.transform.GetChild(0).GetChild(0).GetChild(0).transform.Find("NeckJoint").transform.GetChild(0).gameObject;
        /*                                   <- Waist -> <-RearPiv-> <- Rear ->  <------- TailJoint ------->          */
        GameObject tailJoint = dog.transform.GetChild(0).GetChild(1).GetChild(0).transform.Find("TailJoint").gameObject;
        GameObject leftEarPivot = head.transform.Find("EarPivotLeft").gameObject;
        GameObject rightEarPivot = head.transform.Find("EarPivotRight").gameObject;

        GameObject leftEarModel = CreateModelComponent(dog.m_breed, DogDataField.Ear_Kind, leftEarPivot, earMdls);
        leftEarModel.transform.localScale = new Vector3(-1, 1, 1);
        CreateModelComponent(dog.m_breed, DogDataField.Ear_Kind, rightEarPivot, earMdls);

        CreateModelComponent(dog.m_breed, DogDataField.Snout_Kind, head, snoutMdls);
        CreateModelComponent(dog.m_breed, DogDataField.Tail_Kind, tailJoint, tailMdls);

        SetComponentOrientations(dog.m_breed, DogDataField.Ear_Orientation, leftEarPivot);
        SetComponentOrientations(dog.m_breed, DogDataField.Ear_Orientation, rightEarPivot);
        SetComponentOrientations(dog.m_breed, DogDataField.Tail_Orientation, tailJoint);
    }

    private GameObject CreateModelComponent(DogBreed breed, DogDataField componentType, GameObject parentObj, List<GameObject> modelList)
    {
        GameObject newComponent;
        foreach (GameObject model in modelList)
        {
            if (model.name == GetBreedValue(breed, componentType))
            {
                newComponent = Instantiate(model, dogPrefabBase.transform.position, Quaternion.identity);
                newComponent.transform.parent = parentObj.transform;
                return newComponent;
            }
        }
        Debug.Log("No " + componentType + " model was found for the breed: " + breed);
        return null;
    }

    private void SetComponentOrientations(DogBreed breed, DogDataField componentType, GameObject component)
    {
        string orientDesc = GetBreedValue(breed, componentType);

        switch (componentType)
        {
            case (DogDataField.Ear_Orientation):
                Dictionary<string, KeyValuePair<Vector3, Quaternion>> orientationList = new Dictionary<string, KeyValuePair<Vector3, Quaternion>>();
                if (component.name.Contains("Right")) { orientationList = earOrientations; }
                else if (component.name.Contains("Left")) { orientationList = mirroredEarOrientations; }

                foreach (KeyValuePair<string, KeyValuePair<Vector3, Quaternion>> orientation in orientationList)
                {
                    if (orientDesc == orientation.Key)
                    {
                        component.transform.localPosition = orientation.Value.Key;
                        component.transform.localRotation = orientation.Value.Value;
                        return;
                    }
                }
                break;
            case (DogDataField.Tail_Orientation):
                foreach (KeyValuePair<string, float> orientation in tailOrientations)
                {
                    if (orientDesc == orientation.Key) { component.transform.Rotate(orientation.Value, 0.0f, 0.0f); return; }
                }
                break;
            default: Debug.Log(componentType + " is not a component with orientation data."); return;
        }

        Debug.Log(breed + " has the following invalid data entry for " + componentType + " orientation: " + orientDesc); return;
    }

    private void SetComponentScale(DogBreed breed, DogDataField componentType, GameObject component)
    {
        Vector3 vectorMod = Vector3.zero;
        bool scaling = true;
        bool up = false;
        bool side = false;

        switch (componentType)
        {
            case (DogDataField.Size):
                vectorMod = new Vector3(1, 1, 1);
                break;
            case (DogDataField.Leg_Length):
                vectorMod = new Vector3(0, 1, 0);
                break;
            case (DogDataField.Body_Length):
            case (DogDataField.Snout_Length):
                vectorMod = new Vector3(0, 0, 1);
                break;

            case (DogDataField.Ear_Orientation):
            case (DogDataField.Tail_Orientation):
                string orientation = GetBreedValue(breed, componentType);

                up = (orientation.Substring(0, 1) == "Up");
                if (up) orientation.Replace("Up", "");
                else orientation.Replace("Down", "");

                side = (orientation.Substring(0, 3) == "Down");

                scaling = false;
                break;
            default:
                Debug.Log(componentType + " is not a scalable or rotatable component.");
                return;
        }

        /////////////// SCALABLE COMPONENT ///////////////
        if (scaling)
        {
            switch (GetBreedValue(breed, componentType))
            {
                case ("X.Short"):
                case ("X.Small"):
                    vectorMod *= -0.5f;
                    break;
                case ("Short"):
                case ("Small"):
                    vectorMod *= -0.25f;
                    break;
                case ("Medium"):
                    vectorMod *= 0.0f;
                    break;
                case ("Long"):
                case ("Large"):
                    vectorMod *= 0.25f;
                    break;
                case ("X.Long"):
                case ("X.Large"):
                    vectorMod *= 0.5f;
                    break;
                default:
                    Debug.Log("No " + componentType + " scale description was found for the breed: " + breed);
                    break;
            }
            component.transform.localScale += vectorMod;
            return;
        }
    }
}