using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

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
    NONE,
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
    private Dictionary<DogDataField, List<GameObject>> modelList = new Dictionary<DogDataField, List<GameObject>>();
    public List<GameObject> snoutMdls = new List<GameObject>();
    public List<GameObject> earMdls = new List<GameObject>();
    public List<GameObject> tailMdls = new List<GameObject>();
    private Dictionary<string, KeyValuePair<Vector3, Quaternion>> earOrientations = new Dictionary<string, KeyValuePair<Vector3, Quaternion>>();
    private Dictionary<string, KeyValuePair<Vector3, Quaternion>> mirroredEarOrientations = new Dictionary<string, KeyValuePair<Vector3, Quaternion>>();
    private Dictionary<string, float> tailOrientations = new Dictionary<string, float>();
    private Dictionary<string, float> scalingIncrements = new Dictionary<string, float>();
    private Dictionary<DogDataField, Vector3> scalingDirections = new Dictionary<DogDataField, Vector3>();

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

        numDataFields = DogDataField.GetNames(typeof(DogDataField)).Length - 1;
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

        ////////////////// Model Component Scales //////////////////
        {
            string[] scalingDescriptions = { "X.Short", "X.Small", "Short", "Small", "Medium", "Long", "Large", "X.Long", "X.Large" };
            float[] scalingAmounts = { -0.15f, -0.15f, -0.1f, -0.075f, -0.025f, 0.0f, 0.0f, 0.075f, 0.075f };

            if (scalingDescriptions.Length == scalingAmounts.Length)
                for (int i = 0; i < scalingDescriptions.Length; i++) scalingIncrements.Add(scalingDescriptions[i], scalingAmounts[i]);
            else Debug.Log("Inequal number of scaling increments to defined descriptions.");

            scalingDirections.Add(DogDataField.Size, new Vector3(1,1,1));
            scalingDirections.Add(DogDataField.Snout_Length, new Vector3(0, 0, 1));
            scalingDirections.Add(DogDataField.Body_Length, new Vector3(0,0,1));
            scalingDirections.Add(DogDataField.Leg_Length, new Vector3(0,1,0));
        }

        modelList.Add(DogDataField.Snout_Kind, snoutMdls);
        modelList.Add(DogDataField.Ear_Kind, earMdls);
        modelList.Add(DogDataField.Tail_Kind, tailMdls);

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
        FinaliseDogBody(dogScript.m_breed, dogScript.m_body);

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

    private void FinaliseDogBody(DogBreed breed, Dictionary<BodyPart, BodyComponent> dog)
    {

        DogDataField[] earData = { DogDataField.Ear_Kind, DogDataField.Ear_Orientation };
        dog.Add(BodyPart.Ear0, new BodyComponent(BodyPart.Ear0, dog[BodyPart.Head].GetComponent(), true));
        dog.Add(BodyPart.Ear1, new BodyComponent(BodyPart.Ear1, dog[BodyPart.Head].GetComponent(), true));
        dog[BodyPart.Ear0].SetData(earData);
        dog[BodyPart.Ear1].SetData(earData);

        DogDataField[] snoutData = { DogDataField.Snout_Kind, DogDataField.Snout_Length };
        dog.Add(BodyPart.Snout, new BodyComponent(BodyPart.Snout, dog[BodyPart.Head].GetComponent(), true));
        dog[BodyPart.Snout].SetData(snoutData);

        DogDataField[] tailData = { DogDataField.Tail_Kind, DogDataField.Tail_Orientation };
        dog.Add(BodyPart.Tail, new BodyComponent(BodyPart.Tail, dog[BodyPart.Rear].GetComponent(), true));
        dog[BodyPart.Tail].SetData(tailData);

        foreach (KeyValuePair<BodyPart, BodyComponent> component in dog)
        {
            if (component.Value.GetComponent() == null)
            {
                dog[component.Key].SetComponent(CreateComponentModel(breed, dog[component.Key]));
            }
        }

        dog[BodyPart.Ear0].GetComponent().transform.localScale = new Vector3(-1, 1, 1);

        foreach (KeyValuePair<BodyPart, BodyComponent> component in dog)
        {
            ApplyComponentDataFields(breed, component.Value);
        }

        //dog.leftEar.SetComponent(SetComponentModel(breed, dog.leftEar, earMdls));
        //dog.leftEar.GetComponent().transform.localScale = new Vector3(-1, 1, 1);
        //dog.rightEar.SetComponent(CreateModelComponent(breed, dog.rightEar, earMdls));

        //dog.snout.SetComponent(CreateModelComponent(breed, dog.snout, snoutMdls));

        //dog.tail.SetComponent(CreateModelComponent(breed, dog.tail, tailMdls));

        //BodyComponent[] orientationComponents = { dog.leftEar, dog.rightEar, dog.tail };
        //foreach (BodyComponent component in orientationComponents) { SetComponentOrientations(breed, component); };

        //BodyComponent[] scalableComponents = { dog.snoutPivot, dog.legs[0], dog.legs[1], dog.legs[2], dog.legs[3], dog.chestPivot, dog.rearPivot, dog.waist };
        //foreach (BodyComponent component in scalableComponents) { SetComponentScale(breed, component); };

        //Renderer[] renderers = dog.waist.m_component.transform.parent.GetComponentsInChildren<Renderer>();
        //Bounds bounds = renderers[0].bounds;
        //bounds.center = dog.waist.m_component.transform.position;
        //for (int i = 1, ni = renderers.Length; i < ni; i++) { bounds.Encapsulate(renderers[i].bounds); }

        //dog.collider.center = dog.waist.m_component.transform.localPosition;
        //dog.collider.size = bounds.size;
     
        //dog.waist.m_component.transform.parent.Translate(0, groundObject.transform.position.y - dog.collider.bounds.min.y, 0);
    }

    private void ApplyComponentDataFields(DogBreed breed, BodyComponent component)
    {
        if (component.GetDataList().Count > 0)
        {
            foreach (DogDataField entry in component.GetDataList())
            {
               // if (entry.ToString().Contains("Kind")) { SetComponentModel(breed, component); }
                 if (entry.ToString().Contains("Orientation")) { SetComponentOrientations(breed, component); }
                else if (entry.ToString().Contains("Size") || entry.ToString().Contains("Length")) { SetComponentScale(breed, component); }
                else { Debug.Log("The " + component.GetComponent().name + " doesn't define any dog data fields."); }
            }
        }
    }

    private GameObject CreateComponentModel(DogBreed breed, BodyComponent component)
    {
        List<GameObject> modelOptions = new List<GameObject>();
        DogDataField dataField = DogDataField.NONE;

        foreach (DogDataField entry in component.GetDataList())
        {
            if (entry.ToString().Contains("Kind"))
            {
                modelOptions = modelList[entry];
                dataField = entry;
            }
        };
        if (modelOptions.Count == 0) { Debug.Log("The " + component.GetComponent().name + " doesn't define a model or has no models to choose from."); return null; }

        GameObject newComponent;
        
        foreach (GameObject model in modelOptions)
        {
            if (model.name == GetBreedValue(breed, dataField))
            {
                newComponent = Instantiate(model, dogPrefabBase.transform.position, Quaternion.identity);
                if (component.GetParent() == null) { Debug.Log("The " + newComponent.name + " component hasn't been set a parent."); return null; }
                else
                {
                    newComponent.transform.parent = component.GetParent().transform;
                    return newComponent;
                }
            }
        }
        Debug.Log("No " + dataField + " model was found for the breed: " + breed);
        return null;
    }

    private void SetComponentOrientations(DogBreed breed, BodyComponent component)
    {
        foreach (DogDataField entry in component.GetDataList())
        {
            switch (entry)
            {
                case (DogDataField.Ear_Orientation):
                    Dictionary<string, KeyValuePair<Vector3, Quaternion>> orientationList = new Dictionary<string, KeyValuePair<Vector3, Quaternion>>();
                    if (component.GetComponent().name.Contains("Right")) { orientationList = earOrientations; }
                    else if (component.GetComponent().name.Contains("Left")) { orientationList = mirroredEarOrientations; }

                    foreach (KeyValuePair<string, KeyValuePair<Vector3, Quaternion>> orientation in orientationList)
                    {
                        if (GetBreedValue(breed, DogDataField.Ear_Orientation) == orientation.Key)
                        {
                            component.GetComponent().transform.localPosition = orientation.Value.Key;
                            component.GetComponent().transform.localRotation = orientation.Value.Value;
                            return;
                        }
                    }
                    Debug.Log(breed + " has the following invalid data entry for ear orientation: " + GetBreedValue(breed, DogDataField.Ear_Orientation)); return;

                case (DogDataField.Tail_Orientation):
                    foreach (KeyValuePair<string, float> orientation in tailOrientations)
                    {
                        if (GetBreedValue(breed, DogDataField.Tail_Orientation) == orientation.Key) { component.GetComponent().transform.Rotate(orientation.Value, 0.0f, 0.0f); return; }
                    }
                    Debug.Log(breed + " has the following invalid data entry for tail orientation: " + GetBreedValue(breed, DogDataField.Tail_Orientation)); return;

                default: Debug.Log(component.GetComponent() + " is not a component with orientation data."); return;
            }
        }
    }

    private void SetComponentScale(DogBreed breed, BodyComponent component)
    {
        foreach (DogDataField entry in component.GetDataList())
        {
            if (scalingDirections.ContainsKey(entry))
            {
                foreach (KeyValuePair<string, float> scale in scalingIncrements)
                {
                    if (scale.Key == GetBreedValue(breed, entry))
                    {
                        component.GetComponent().transform.localScale += scalingDirections[entry] * scale.Value;
                        return;
                    }
                }
                Debug.Log(entry + " is not a component with scale data."); return;
            }
        }
        Debug.Log("No " + component.GetType() + " scale description was found for the breed: " + breed);
    }
}