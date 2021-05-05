using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public enum DogCareValue
{
   NONE, Hunger, Attention, Rest, Hygiene, Health, Happiness
} 

public enum DogPersonalityValue
{
   NONE, Obedience, Tolerance, Affection, Intelligence, Energy, Bond
}

public enum DogBreed
{
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
    Shiba_Inu,
    Test
}

public enum DogDataField
{
    Breed,
    Max_Age,
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

public enum BodyPart
{
    Neck, Head, Eye0, Eye1,
    Snout,
    Ear0, Ear1,
    Tail,

    Shoulder0, Shoulder1, Shoulder2, Shoulder3,
    UpperLeg0, UpperLeg1, UpperLeg2, UpperLeg3,

    Knee0, Knee1, Knee2, Knee3,

    LowerLeg0, LowerLeg1, LowerLeg2, LowerLeg3,
    Ankle0, Ankle1, Ankle2, Ankle3,
    Foot0, Foot1, Foot2, Foot3,

    Chest, Rear, Waist
}

public struct BodyComponent
{
    private BodyPart m_part;
    public GameObject m_component;
    private GameObject m_parent;
    private List<DogDataField> m_data;

    public BodyComponent(BodyPart type, GameObject component, GameObject parent, DogDataField data)
    {
        m_part = type;
        m_component = component;
        m_parent = parent;
        m_data = new List<DogDataField>();
        m_data.Add(data);
    }

    public BodyComponent(BodyPart type, GameObject component, GameObject parent, DogDataField[] dataList)
    {
        m_part = type;
        m_component = component;
        m_parent = parent;
        m_data = new List<DogDataField>();
        foreach (DogDataField field in dataList) { m_data.Add(field); };
    }

    public BodyComponent(BodyPart type, GameObject component, GameObject parent)
    {
        m_part = type;
        m_component = component;
        m_parent = parent;
        m_data = new List<DogDataField>();
    }

    public void SetData(DogDataField data) { m_data.Add(data); }
    public void SetData(DogDataField[] dataList) { foreach (DogDataField field in dataList) { m_data.Add(field); }; }

    public BodyPart GetPartType() { return m_part; }
    public GameObject GetParent() { return m_parent.gameObject; }
    public List<DogDataField> GetDataList() { return m_data; }

    public bool DefinesDataField(DogDataField field) { return m_data.Contains(field); }
    public bool HasFieldContaining(string str)
    {
        foreach (DogDataField field in m_data)
        {
            if (field.ToString().Contains(str)) { return true; }
        }
        return false;
    }
}

public class DogGeneration : MonoBehaviour
{
    private struct ModelOrientation
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

    [SerializeField] private float defaultCareDecrement;

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

    [SerializeField] private Controller controller;
    [SerializeField] private DataDisplay dogUIOutputScript;
    [SerializeField] private AStarSearch groundAStar;
    [SerializeField] private GameObject randomPointStorage;
    [SerializeField] private GameObject defaultNullObject;

    [SerializeField] private string breedDataFileDir = "/Scripts/ReferenceFiles/BreedData.txt";
    private DataTable breedData = new DataTable();
    private int numDataFields = 0;
    private int dogBreedsDefined = 0;

    private void Start()
    {
        ////////////////// Read the Breed Data from the Dog Data Text File //////////////////
        string[] data = System.IO.File.ReadAllLines(Application.dataPath + breedDataFileDir);
        for (int i = 0; i < data.Length; i++) //Removes Blank Spaces from Data
        {
            data[i] = string.Concat(data[i].Where(c => !char.IsWhiteSpace(c)));
        }

        numDataFields = DogDataField.GetNames(typeof(DogDataField)).Length;
        dogBreedsDefined = data.Length - 1;

        breedData.Clear();

        string[] headers = new string[numDataFields];
        headers = data[0].Split('?');
        foreach (string column in headers) { breedData.Columns.Add(column); } 

        for (int i = 0; i < dogBreedsDefined; i++)
        {
            string[] field = new string[numDataFields];
            field = data[i + 1].Split('?');

            DataRow newData = breedData.NewRow();
            for (int j = 0; j < numDataFields; j++)
            {
                newData[breedData.Columns[j].ToString()] = field[j];
            }
            breedData.Rows.Add(newData);
        }

        ////////////////// Model Component Orienations //////////////////
        string[] earOrientDescriptions = { "UpFront", "UpSide", "DownFront", "DownSide" };
        Vector3[] earOrientPositions = { new Vector3(0.45f, 1.15f, 0.6f), new Vector3(0.45f, 1.15f, 0.55f), new Vector3(0.45f, 1.15f, 0.55f), new Vector3(0.6f, 1.15f, 0.55f) };
        Vector3[] earOrientRotations = { new Vector3(0.0f, 3.5f, 3.5f), new Vector3(35.0f, 55.0f, 30.0f), new Vector3(50.0f, -20.0f, -55.0f), new Vector3(125.0f, 95.0f, 30.0f) };
        for (int i = 0; i < earOrientDescriptions.Length; i++) modelOrientations.Add(new ModelOrientation(DogDataField.Ear_Orientation, earOrientDescriptions[i], earOrientPositions[i], earOrientRotations[i]));

        string[] tailOrientDescriptions = { "Up", "Middle", "Down" };
        Vector3[] tailOrientRotations = { new Vector3(35.0f, 0f, 0f), new Vector3(0.0f, 0f, 0f), new Vector3(45.0f, 0f, 0f) };
        for (int i = 0; i < tailOrientDescriptions.Length; i++) modelOrientations.Add(new ModelOrientation(DogDataField.Tail_Orientation, tailOrientDescriptions[i], Vector3.zero, tailOrientRotations[i], false));

        ////////////////// Model Component Scalers //////////////////

        string[] scalingDescriptions = { "X.Short", "X.Small", "Short", "Small", "Medium", "Long", "Large", "X.Long", "X.Large" };
        float[] scalingAmounts = { -0.5f, -0.5f, -0.25f, -0.25f, -0.15f, 0.0f, 0.0f, 0.25f, 0.25f };
        for (int i = 0; i < scalingDescriptions.Length; i++) modelScalers.Add(scalingDescriptions[i], scalingAmounts[i]);

        scalingDirections.Add(DogDataField.Size, new Vector3(1, 1, 1));
        scalingDirections.Add(DogDataField.Snout_Length, new Vector3(0, 0, 1));
        scalingDirections.Add(DogDataField.Body_Length, new Vector3(0, 0, 1));
        scalingDirections.Add(DogDataField.Leg_Length, new Vector3(0, 1, 0));

        ////////////////// Add Models to the Iteratable List of all Models //////////////////
        modelList.Add(DogDataField.Snout_Kind, snoutMdls);
        modelList.Add(DogDataField.Ear_Kind, earMdls);
        modelList.Add(DogDataField.Tail_Kind, tailMdls);

        ////////////////// Initialise the States Possessed by Each Dog Property Value //////////////////
        DefineDogPropStates();
    }

    private string GetBreedValue(DogBreed breed, DogDataField dataField)
    {
        for (int i = 0; i < dogBreedsDefined; i++)
        {
            if (breed.ToString() == breedData.Rows[i]["Breed"].ToString())
            {
                return breedData.Rows[i][dataField.ToString()].ToString();
            }
        }
        Debug.LogWarning("The data field " + dataField.ToString() + " cannot be found for breed: " + breed.ToString());
        return null;
    }

    private void DefineDogPropStates()
    {
        //////////////////// Care Value States ////////////////////
        string[] hungerState = { "Starving", "Hungry", "Fed", "Overfed" };
        Vector2[] hungerStateRanges = { new Vector2(0, 15), new Vector2(0, 50), new Vector2(50, 100), new Vector2(100, 100) };

        string[] attentionState = { "Lonely", "Loved", "Overcrowded" };
        Vector2[] attentionStateRanges = { new Vector2(0, 50), new Vector2(50, 100), new Vector2(100, 100) };

        string[] restState = { "Exhausted", "Tired", "Rested", "Rejuvinated"};
        Vector2[] restStateRanges = { new Vector2(0, 20), new Vector2(20, 50), new Vector2(50, 100), new Vector2(100, 100) };

        string[] hygieneState = { "Filthy", "Dirty", "Clean" };
        Vector2[] hygieneStateRanges = { new Vector2(0, 20), new Vector2(0, 60), new Vector2(60, 100) };

        string[] healthState = { "Dying", "Sick", "Healthy" };
        Vector2[] healthStateRanges = { new Vector2(0, 10), new Vector2(0, 30), new Vector2(30, 100) };

        string[] happinessState = { "Distressed", "Upset", "Happy" };
        Vector2[] happinessStateRanges = { new Vector2(0, 20), new Vector2(0, 50), new Vector2(50, 100) };

        AddStatesToProperty(DogCareValue.Hunger, hungerState, hungerStateRanges);
        AddStatesToProperty(DogCareValue.Attention, attentionState, attentionStateRanges);
        AddStatesToProperty(DogCareValue.Rest, restState, restStateRanges);
        AddStatesToProperty(DogCareValue.Hygiene, hygieneState, hygieneStateRanges);
        AddStatesToProperty(DogCareValue.Health, healthState, healthStateRanges);
        AddStatesToProperty(DogCareValue.Happiness, happinessState, happinessStateRanges);

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

        string[] bondStates = { "Wary", "Friendly" };
        Vector2[] bondStateRanges = { new Vector2(0, 50), new Vector2(50, 100) };

        AddStatesToProperty(DogPersonalityValue.Obedience, obedienceStateNames, obedienceStateRanges);
        AddStatesToProperty(DogPersonalityValue.Affection, affectionStates, affectionStateRanges);
        AddStatesToProperty(DogPersonalityValue.Tolerance, toleranceStates, toleranceStateRanges);
        AddStatesToProperty(DogPersonalityValue.Intelligence, intelligenceStates, intelligenceStateRanges);
        AddStatesToProperty(DogPersonalityValue.Energy, energyStates, energyStateRanges);
        AddStatesToProperty(DogPersonalityValue.Bond, bondStates, bondStateRanges);
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

    //public void GenerateAllDogsLineUp()
    //{
    //    float dogSpace = 3;
    //    Vector3 posOffset = groundAStar.transform.position;
    //    posOffset.y = dogSpace;

    //    foreach (DogBreed dogBreed in (DogBreed[])DogBreed.GetValues(typeof(DogBreed)))
    //    {
    //        GameObject dog = GenerateDog(dogBreed).gameObject;        
    //        dog.transform.position = posOffset;
    //        posOffset.x += dogSpace;
    //    }
    //}

    public void GenerateRandomNewDog()
    {
        GenerateDog((DogBreed)UnityEngine.Random.Range(0, dogBreedsDefined));
    }

    private void GenerateDog(DogBreed breed)
    {
        GameObject newDog = Instantiate(dogPrefabBase, Vector3.zero, Quaternion.identity);
        newDog.transform.parent = transform.GetChild(0);
        newDog.name = "Unnamed_" + breed;

        Dog dogScript = newDog.GetComponent<Dog>();
        dogScript.m_breed = breed;
        dogScript.m_maxAge = int.Parse(GetBreedValue(breed, DogDataField.Max_Age));
        dogScript.m_age = UnityEngine.Random.Range(1, dogScript.m_maxAge);

        DefineDogProperties(dogScript);
        FinaliseDogBody(dogScript.m_breed, dogScript.m_body);
        
        Renderer[] renderers = newDog.transform.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        bounds.center = dogScript.m_body[BodyPart.Waist].m_component.transform.position;
        for (int i = 1, ni = renderers.Length; i < ni; i++) { bounds.Encapsulate(renderers[i].bounds); }
        bounds.size += new Vector3(0.5f, 0.25f, 1.0f);
        bounds.center += new Vector3(0.0f, -0.25f, 1.0f);

        dogScript.m_collider.center = bounds.center;
        dogScript.m_collider.size = bounds.size;

        controller.AddDog(dogScript);
    }

    private void DefineDogProperties(Dog dog)
    {
        foreach (DogCareValue careValue in (DogCareValue[])DogCareValue.GetValues(typeof(DogCareValue)))
        {
            if (careValue != DogCareValue.NONE) dog.m_careValues.Add(careValue, new CareProperty(careValueStates[careValue], defaultCareDecrement));
        }

        foreach (DogPersonalityValue personalityValue in (DogPersonalityValue[])DogPersonalityValue.GetValues(typeof(DogPersonalityValue)))
        {
            if ((personalityValue != DogPersonalityValue.NONE) && (personalityValue != DogPersonalityValue.Bond))
            {
                if (Enum.IsDefined(typeof(DogDataField), personalityValue.ToString()))
                {
                    float breedValueFloat;
                    string breedPersonalityValue = GetBreedValue(dog.m_breed, (DogDataField)Enum.Parse(typeof(DogDataField), personalityValue.ToString()));

                    if (float.TryParse(breedPersonalityValue, out breedValueFloat))
                    {
                        dog.m_personalityValues.Add(personalityValue, new PersonalityProperty(personalityValueStates[personalityValue], breedValueFloat));
                    }
                    else Debug.LogWarning("The dog breed data file did not contain a valid float value for the personality value: " + personalityValue);
                }
                else Debug.LogWarning(personalityValue + " is not defined as a data field in the dog data file.");
            }
            else if (personalityValue == DogPersonalityValue.Bond)
            {
                dog.m_personalityValues.Add(personalityValue, new PersonalityProperty(personalityValueStates[personalityValue], 1.0f));
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
                    }
                }
            }    
        }

        Dictionary<BodyPart, DogDataField> compsToScale = new Dictionary<BodyPart, DogDataField>();
        Dictionary<BodyPart, DogDataField> compsToRotate = new Dictionary<BodyPart, DogDataField>();

        foreach (KeyValuePair<BodyPart, BodyComponent> part in dog)
        {
            foreach (DogDataField entry in part.Value.GetDataList())
            {
                if (entry.ToString().Contains("Length")) { compsToScale.Add(part.Key, entry); }
                else if (entry.ToString().Contains("Orientation")) { compsToRotate.Add(part.Key, entry); }
            }
        }
        foreach (KeyValuePair<BodyPart, DogDataField> entry in compsToScale) { SetComponentScale(breed, dog[entry.Key], entry.Value); }
        foreach (KeyValuePair<BodyPart, DogDataField> entry in compsToRotate) { SetComponentOrientations(breed, dog[entry.Key], entry.Value); }

        dog[BodyPart.Waist].m_component.transform.localScale += scalingDirections[DogDataField.Size] * modelScalers[GetBreedValue(breed, DogDataField.Size)];
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

    private void SetComponentOrientations(DogBreed breed, BodyComponent component, DogDataField orientationDesc)
    {
        foreach (ModelOrientation orientation in modelOrientations)
        {
            if ((orientation.m_type == orientationDesc) && (orientation.m_description == GetBreedValue(breed, orientationDesc)))
            {
                bool mirrored = component.GetPartType().ToString().Contains("1");

                if (orientation.m_usePosition)
                {
                    if (!mirrored) { component.m_component.transform.localPosition = orientation.m_position; }
                    else { component.m_component.transform.localPosition = orientation.m_mirrorPos; }
                }
                if (!mirrored) { component.m_component.transform.localEulerAngles = orientation.m_rotation; }
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

    private void SetComponentScale(DogBreed breed, BodyComponent component, DogDataField scalingDesc)
    {
        if (scalingDirections.ContainsKey(scalingDesc))
        {
            foreach (KeyValuePair<string, float> scale in modelScalers)
            {
                if (scale.Key == GetBreedValue(breed, scalingDesc))
                {
                    Vector3 newScale = scalingDirections[scalingDesc] * scale.Value;

                    Dictionary<GameObject, Vector3> children = new Dictionary<GameObject, Vector3>();
                    for (int i = 0; i < component.m_component.transform.childCount; i++)
                    {
                        GameObject child = component.m_component.transform.GetChild(i).gameObject;
                        if (child.tag == "Joint")
                        {
                            children.Add(child, child.transform.localScale);
                        }
                    }

                    component.m_component.transform.localScale += newScale;

                    foreach (KeyValuePair<GameObject, Vector3> child in children)
                    {
                        child.Key.transform.localScale = child.Value; 
                    }
                    return;
                }
            }
            Debug.LogWarning(scalingDesc + " is not a component with scale data."); return;
        }
        Debug.LogWarning(breed + " has the following invalid data entry for: " + scalingDesc); return;
    }
}