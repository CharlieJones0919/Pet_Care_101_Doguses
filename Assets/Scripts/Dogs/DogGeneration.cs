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

public struct ModelOrientation
{
    public DogDataField m_type;
    public string m_description;
    public Vector3 m_position;
    public Vector3 m_rotation;
    public bool m_usePosition;

    public Vector3 m_mirrorPos;
    public Vector3 m_mirrorRot;

    public ModelOrientation(DogDataField type, string description, Vector3 position, Vector3 rotation, bool usePosition = true)
    {
        m_type = type; m_description = description; m_position = position; m_rotation = rotation; m_usePosition = usePosition;

        m_mirrorPos = position;
        m_mirrorPos.x = -m_mirrorPos.x; 
        m_mirrorRot = -rotation;
        m_mirrorRot.x = rotation.x;
    }
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

    private List<ModelOrientation> modelOrientations = new List<ModelOrientation>();
    private Dictionary<string, float> modelScalers = new Dictionary<string, float>();
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
        string[] earOrientDescriptions = { "UpFront",                       "UpSide",                         "DownFront",                        "DownSide" };
        Vector3[] earOrientPositions =   { new Vector3(0.45f, 1.15f, 0.6f), new Vector3(0.45f, 1.15f, 0.55f), new Vector3(0.45f, 1.15f, 0.55f),   new Vector3(0.6f, 1.15f, 0.55f) };
        Vector3[] earOrientRotations =   { new Vector3(0.0f, 3.5f, 3.5f),   new Vector3(35.0f, 55.0f, 30.0f), new Vector3(50.0f, -20.0f, -55.0f), new Vector3(125.0f, 95.0f, 30.0f) };
        for (int i = 0; i < earOrientDescriptions.Length; i++) modelOrientations.Add(new ModelOrientation(DogDataField.Ear_Orientation, earOrientDescriptions[i], earOrientPositions[i], earOrientRotations[i]));
   
        string[] tailOrientDescriptions = { "Up",                       "Middle",                   "Down" };
        Vector3[] tailOrientRotations =   { new Vector3(35.0f, 0f, 0f), new Vector3(0.0f, 0f, 0f), new Vector3(45.0f, 0f, 0f) };
        for (int i = 0; i < tailOrientDescriptions.Length; i++) modelOrientations.Add(new ModelOrientation(DogDataField.Tail_Orientation, tailOrientDescriptions[i], Vector3.zero, tailOrientRotations[i], false));

        ////////////////// Model Component Scalers //////////////////
  
        string[] scalingDescriptions = { "X.Short", "X.Small", "Short", "Small", "Medium", "Long", "Large", "X.Long", "X.Large" };
        float[] scalingAmounts =       { -1.0f,     -1.0f,    -0.5f,    -0.5f,  -0.25f,    0.0f,   0.0f,    0.25f,   0.25f };
        for (int i = 0; i < scalingDescriptions.Length; i++)  modelScalers.Add(scalingDescriptions[i], scalingAmounts[i]);

        scalingDirections.Add(DogDataField.Size, new Vector3(1,1,1));
        scalingDirections.Add(DogDataField.Snout_Length, new Vector3(0, 0, 1));
        scalingDirections.Add(DogDataField.Body_Length, new Vector3(0,0,1));
        scalingDirections.Add(DogDataField.Leg_Length, new Vector3(0,1,0));

        ////////////////// Add Models to the Iteratable List of all Models //////////////////
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

        Debug.LogWarning("Requested data for that breed cannot be found: " + breed.ToString() + " " + dataField.ToString());
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
        if (names.Length != ranges.Length) { Debug.LogWarning("Inequal number of state names to ranges attempting to be added to: " + property); return; }
        Dictionary<string, Vector2> states = new Dictionary<string, Vector2>();
        for (int i = 0; i < names.Length; i++) states.Add(names[i], ranges[i]);
        careValueStates.Add(property, states);
    }

    private void AddStatesToProperty(DogPersonalityValue property, string[] names, Vector2[] ranges)
    {
        if (names.Length != ranges.Length) { Debug.LogWarning("Inequal number of state names to ranges attempting to be added to: " + property); return; }
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
                    else Debug.LogWarning("The dog breed data file did not contain a valid float value for the personality value: " + personalityValue);
                }
                else Debug.LogWarning(personalityValue + " is not defined as a data field in the dog data file.");
            }
        }
    }

    private void FinaliseDogBody(DogBreed breed, Dictionary<BodyPart, BodyComponent> dog)
    {
        Dictionary<BodyPart, GameObject> parentAssignments = new Dictionary<BodyPart, GameObject>();
        Dictionary<BodyPart, DogDataField[]> dataAssignments = new Dictionary<BodyPart, DogDataField[]>();

        DogDataField[] earData = { DogDataField.Ear_Kind, DogDataField.Ear_Orientation };
        DogDataField[] snoutData = { DogDataField.Snout_Kind, DogDataField.Snout_Length };
        DogDataField[] tailData = { DogDataField.Tail_Kind, DogDataField.Tail_Orientation };

        parentAssignments.Add(BodyPart.Ear0, dog[BodyPart.Head].m_component);
        parentAssignments.Add(BodyPart.Ear1, dog[BodyPart.Head].m_component);
        parentAssignments.Add(BodyPart.Snout, dog[BodyPart.Head].m_component);
        parentAssignments.Add(BodyPart.Tail, dog[BodyPart.Rear].m_component);

        dataAssignments.Add(BodyPart.Ear0, earData);
        dataAssignments.Add(BodyPart.Ear1, earData);
        dataAssignments.Add(BodyPart.Snout, snoutData);
        dataAssignments.Add(BodyPart.Tail, tailData);

        // Create Non-Existing Component Models
        foreach (BodyPart part in (BodyPart[])Enum.GetValues(typeof(BodyPart)))
        {
            if (!dog.ContainsKey(part))
            {
                foreach (DogDataField entry in dataAssignments[part])
                {
                    if (entry.ToString().Contains("Kind") && (entry != DogDataField.Coat_Kind))
                    {
                        dog.Add(part, new BodyComponent(part, CreateComponentModel(breed, parentAssignments[part], entry), parentAssignments[part], dataAssignments[part]));                   
                        if (dog[part].m_component == null) { Debug.Log("Component unsucessfully set: " + part.ToString()); }
                    }
                };
            }
        }

        //if (dog[part].GetDataList().Count > 0)
        //{
        foreach (BodyPart part in (BodyPart[])Enum.GetValues(typeof(BodyPart)))
        {
            foreach (DogDataField entry in dog[part].GetDataList())
            {
                if (entry.ToString().Contains("Orientation"))
                {
                    if (!dog[part].GetPartType().ToString().Contains("1")) { SetComponentOrientations(breed, dog[part], entry); }
                    else { SetComponentOrientations(breed, dog[part], entry, true); }
                }
                if (entry.ToString().Contains("Length") || entry.ToString().Contains("Size")) { SetComponentScale(breed, dog[part]); }
                //  { Debug.LogWarning("The " + component.GetType().ToString() + " doesn't define any dog data fields."); }
            }
        }
        //}


        //foreach (KeyValuePair<BodyPart, BodyComponent> component in dog)
        //{
        //    if (component.Value.m_component() == null) { Debug.LogWarning("No object found for the " + component.Value.GetType()); }

        //    else if (component.Value.GetDataList().Count > 0)
        //    {
        //        foreach (DogDataField entry in component.Value.GetDataList())
        //        {
        //            if (entry.ToString().Contains("Orientation")) { SetComponentOrientations(breed, component.Value); }
        //            if (entry.ToString().Contains("Size") || entry.ToString().Contains("Length")) { SetComponentScale(breed, dog[component.Key]); }
        //            { Debug.LogWarning("The " + component.GetType().ToString() + " doesn't define any dog data fields."); }
        //        }
        //    }
        //}

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

    private GameObject CreateComponentModel(DogBreed breed, GameObject parent, DogDataField modelKind)
    {
        List<GameObject> modelOptions = modelList[modelKind];

        foreach (GameObject model in modelOptions)
        {
            if (model.name == GetBreedValue(breed, modelKind))
            {              
                GameObject newComponent = Instantiate(model, model.transform.position, model.transform.rotation);
                if (parent == null) { Debug.LogWarning("The " + newComponent.name + " component hasn't been set a parent."); return null; }
                else
                {
                    newComponent.transform.parent = parent.transform;
                    return newComponent;
                }
            }
        }
        Debug.LogWarning("No " + modelKind + " model was found for the breed: " + breed); return null;
    }

    private void SetComponentOrientations(DogBreed breed, BodyComponent component, DogDataField orientationDesc, bool mirror = false)
    {
        // string orientDescription = GetBreedValue(breed, orientationDesc);
        //foreach (DogDataField entry in component.GetDataList())
        //{
        //    switch (entry)
        //    {
        //        case (DogDataField.Ear_Orientation):
        //            orientDescription = GetBreedValue(breed, DogDataField.Ear_Orientation);

        //            Dictionary<string, KeyValuePair<Vector3, Vector3>> orientationList = new Dictionary<string, KeyValuePair<Vector3, Vector3>>();
        //           /* if (component.GetPartType().ToString().Contains("0")) */ {orientationList = componentOrientations[]; }
        //            //else if (component.GetPartType().ToString().Contains("1"))
        //            //{
        //            //    //Vector3 leftEarScale = component.m_component.transform.localScale;
        //            //    //leftEarScale.x = -leftEarScale.x;
        //            //    //component.m_component.transform.localScale = leftEarScale;
        //            //    orientationList = mirroredEarOrientations;
        //            //}
        //          //  else { Debug.Log("This component isn't an ear: " + component.m_component.name); return; }

        //            foreach (KeyValuePair<string, KeyValuePair<Vector3, Vector3>> orientation in orientationList)
        //            {
        //                if (earOrient == orientation.Key)
        //                {                      
        //                    component.m_component.transform.localPosition = orientation.Value.Key;
        //                    component.m_component.transform.localEulerAngles = orientation.Value.Value;
        //                    return;
        //                }
        //            }
        //            Debug.LogWarning(breed + " has the following invalid data entry for ear orientation: " + earOrient); return;

        //        case (DogDataField.Tail_Orientation):
        //            orientation = GetBreedValue(breed, DogDataField.Tail_Orientation);

        //        foreach (KeyValuePair<string, float> orientation in tailOrientations)
        //        {
        //            if (tailOrient == orientation.Key)
        //            {
        //                component.m_component.transform.Rotate(orientation.Value, 0.0f, 0.0f);
        //                return;
        //            }
        //        }
        //        Debug.LogWarning(breed + " has the following invalid data entry for tail orientation: " + tailOrient); return;
        //}

        foreach (ModelOrientation orientation in modelOrientations)
        {
            if ((orientation.m_type == orientationDesc) && (orientation.m_description == GetBreedValue(breed, orientationDesc)))
            {
                if (orientation.m_usePosition)
                {
                    if (!mirror) { component.m_component.transform.localPosition = orientation.m_position; }
                    else { component.m_component.transform.localPosition = orientation.m_mirrorPos; }
                }
                if (!mirror) { component.m_component.transform.localEulerAngles = orientation.m_rotation; }
                else
                {
                    component.m_component.transform.localEulerAngles = orientation.m_mirrorRot;

                    Vector3 mirroredScale = component.m_component.transform.localScale;
                    mirroredScale.x = -mirroredScale.x;
                    component.m_component.transform.localScale = mirroredScale;
                }
                return;
            }
        }
        Debug.LogWarning(breed + " has the following invalid data entry for: " + orientationDesc); return;
    }

    private void SetComponentScale(DogBreed breed, BodyComponent component, bool scaleChildren = false)
    {
        foreach (DogDataField entry in component.GetDataList())
        {
            if (scalingDirections.ContainsKey(entry))
            {
                foreach (KeyValuePair<string, float> scale in modelScalers)
                {
                    if (scale.Key == GetBreedValue(breed, entry))
                    {
                        Vector3 newScale = component.m_component.transform.localScale;
                        newScale += scalingDirections[entry] * scale.Value;
                        for (int i = 0; i < 3; i++) { newScale[i] = Mathf.Abs(newScale[i]); }

                        if (!scaleChildren && (component.m_component.transform.childCount > 0))
                        {
                            List<Transform> children = new List<Transform>();
                            for (int i = 0; i < component.m_component.transform.childCount; i++) { children.Add(component.m_component.transform.GetChild(i).transform); }

                            component.m_component.transform.localScale = newScale;

                            foreach (Transform child in children) { child.localScale -= scalingDirections[entry] * scale.Value; }
                            return;
                        }
                        else { component.m_component.transform.localScale = newScale; return; }                   
                    }
                }
                Debug.LogWarning(entry + " is not a component with scale data."); return;
            }
        }
        Debug.LogWarning("No " + component.GetPartType() + " scale description was found for the breed: " + breed);
    }
}