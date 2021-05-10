/** \file DogGeneration.cs
*  \brief This file reads in all the breeds and their specific physical and initial personality values/types from an external text file in Assets/Scripts/BreedData.txt.
*  It's contains most of the public dog related enums that comprise their details, appearances and behaviours.
*  The main DogGeneration class is mainly concerned with instantiating dog prefabs then adding their breed varying missing model parts (e.g. ears, snouts, tails, etc...) then scaling their body components to the values also specified in the BreedData text file.
*  Afformentioned class is also responcible for defining the states of each Care/Personality value, and the values which would be in the bounds of each state. 
*  (E.g. The DogCareValue of Hunger has a state of "Fed" which has upper and lower bounds that if a dog's Hunger value was in those bounds, would set that CareValue to be in the "Fed" state). 
*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

/** \enum DogCareValue
*  \brief A public enumerated list of "Care" values each dog will possess; these are a percentage set to 100% for a dog when first created. These are the values the player will need to maintain as they fall in value with time continuously. 
*  The main difference between these and DogPersonalityValue is that while these change drastically by the hour, the personality values change very slowly.
*/
public enum DogCareValue
{
   NONE, Hunger, Attention, Rest, Hygiene, Health, Happiness
}

/** \enum DogPersonalityValue
*  \brief A public enumerated list of "Personality" values each dog will possess; these are a value out of 5 stars set in DogGeneration based on the dog's breed. These values don't change without user-interaction/item use.
*  Unfortunately the functionality of these weren't implemented in full and don't influence the dogs' behaviours in any way as was originally intended, meaning dogs of a different breed don't act any differently but are purely aesthetically different.
*/
public enum DogPersonalityValue
{
   NONE, Obedience, Tolerance, Affection, Intelligence, Energy, Bond
}

/** \enum DogBreed
*  \brief The different possible dogs breeds to generate. While these will be read from the BreedData.txt file as well, and having a duplication of the data as enums increases chances of inconsistencies between them, having them as enums reduces the chance of typo errors from referring to this data via strings.
*/
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
    Shiba_Inu
}

/** \enum DogDataField
*  \brief All the different data fields that can be collected from the BreedData.txt file. Kept as enums for the same reason as the DogBreed enums.
*/
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

/** \enum BodyPart
*  \brief All the game object parts that compose a Dog object's "body" - these are defines to make referencing of each part of the dog easy if required like for scaling the components based on breed in the DogGeneration class.
*/
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

/** \struct BodyComponent
*  \brief Each "BodyPart" enum is paired with a BodyComponent structure in the Dog class as a dictionary of pairs. 
*  This struct stores the BodyPart enum, the actual GameObject being referred to, the object's parent in the hierarchy, and the DogDataField data from the BreedData.txt file which should be applied to that component.
*  In retrospect, this would probably be better achieved by making this a class and attaching it to each body component in the inspector with a serializable list to set the data for which would send itself to controller or the Dog script's list of parts.
*/
public struct BodyComponent
{
    private BodyPart m_part;            //!< Which BodyPart this BodyComponent is paired to.
    public GameObject m_component;      //!< The GameObject/model of this body part.
    private GameObject m_parent;        //!< Parent of this GameObject in the hierarchy.
    private List<DogDataField> m_data;  //!< The DogDataField data from the BreedData.txt file which should be applied to that component. (E.g. If this is a BodyPart, the Leg_Length data should be applied to it for scaling in DogGeneration).

    /** \fn BodyComponent
    *  \brief A constructor to set all of the struct's values with just 1 DogDataField entry.
    */
    public BodyComponent(BodyPart type, GameObject component, GameObject parent, DogDataField data)
    {
        m_part = type;
        m_component = component;
        m_parent = parent;
        m_data = new List<DogDataField>();
        m_data.Add(data);
    }

    /** \fn BodyComponent
    *  \brief A constructor to set all of the struct's values with multiple DogDataField entries passed in as an array.
    */
    public BodyComponent(BodyPart type, GameObject component, GameObject parent, DogDataField[] dataList)
    {
        m_part = type;
        m_component = component;
        m_parent = parent;
        m_data = new List<DogDataField>();
        foreach (DogDataField field in dataList) { m_data.Add(field); };
    }

    /** \fn BodyComponent
    *  \brief A constructor to set the struct's BodyPart type, object, and parent, but no DogDataField entries.
    */
    public BodyComponent(BodyPart type, GameObject component, GameObject parent)
    {
        m_part = type;
        m_component = component;
        m_parent = parent;
        m_data = new List<DogDataField>();
    }

    /** \fn SetData
    *  \brief Add a single DogDataField entry to this component's list. */
    public void SetData(DogDataField data) { m_data.Add(data); }
    /** \fn SetData
    *  \brief Add a multiple DogDataField entries to this component's list as an array. */
    public void SetData(DogDataField[] dataList) { foreach (DogDataField field in dataList) { m_data.Add(field); }; }

    /** \fn GetPartType
    *  \brief Returns this component's BodyPart enum. */
    public BodyPart GetPartType() { return m_part; }
    /** \fn GetParent
    *  \brief Returns this component's parent object. */
    public GameObject GetParent() { return m_parent.gameObject; }
    /** \fn GetDataList
    *  \brief Returns this component's applicable DogDataField enum entries. */
    public List<DogDataField> GetDataList() { return m_data; }
}

/** \class DogGeneration
*  \brief This class is responcible for creating the Dog instances when triggered to by GameTime, and modifying the basic Dog prefab with additional breed specific physical attributes as defined in BreedData.txt.
*  Also initialises dogs' other base data attributes like their Care/Personality values + their states/bounds, along with their ages and initial personality values.
*/
public class DogGeneration : MonoBehaviour
{
    /** \struct ModelOrientation
    *  \brief A short struct for storing different body part/model positioning/orientation descriptions from BreedData with the real data of what that should make the model's rotation, what that would be mirrored (for if the body component's position isn't centred on the dog), etc.
    */
    private struct ModelOrientation
    {
        public DogDataField m_type;  //!< The component orientation data field from the BreedData.txt file. (E.g. Ear_Orientation, Tail_Orientation, etc...).
        public string m_description; //!< The description of the m_type field for this ModelOrientation. For instance, multiple ModelOrientations with the m_descriptions of "Up," "Down," and "Middle" would be required for Tail_Oritentation. 
        public Vector3 m_position;   //!< Where the component should be positioned locally for this orientation.
        public Vector3 m_rotation;   //!< The rotation this orientation description should be paired with.
        public bool m_usePosition;   //!< Whether or not to actually use the position given in the constructor.

        public Vector3 m_mirrorPos;  //!< This component's position locally mirrored. (The ear parts use this as they aren't centered on the dog object so one of them must be reflected).
        public Vector3 m_mirrorRot;  //!< This component's rotation locally mirrored. (The ear parts use this as they aren't centered on the dog object so one of them must be reflected).

        /** \struct ModelOrientation
        *  \brief A constructor to instatiate a ModelOrientation structure with all its required variables.
        */
        public ModelOrientation(DogDataField type, string description, Vector3 position, Vector3 rotation, bool usePosition = true)
        {
            m_type = type; m_description = description; m_position = position; m_rotation = rotation; m_usePosition = usePosition;

            m_mirrorPos = position;
            m_mirrorPos.x = -m_mirrorPos.x;
            m_mirrorRot = -rotation;
            m_mirrorRot.x = rotation.x;
        }
    }

    [SerializeField] private Controller controller;      //!< Reference to the game controller.
    [SerializeField] private float defaultCareDecrement; //!< A serializable float value to set the amount the dogs' care values are decreased by each update by default.

    private Dictionary<DogCareValue, Dictionary<string, Vector2>> careValueStates = new Dictionary<DogCareValue, Dictionary<string, Vector2>>();    //!< A Dictionary list of all the care values a dog should have, paired with the states each of those care values can be in depending on its float value at run-time (check CareProperties in Controller for more info), and the lower/upper bounds of said state.
    private Dictionary<DogPersonalityValue, Dictionary<string, Vector2>> personalityValueStates = new Dictionary<DogPersonalityValue, Dictionary<string, Vector2>>(); //!< The same as careValueStates but for the dogs' personality values.

    public Dog dogPrefabBase;                            //!< The unmodified dog object prefab. Won't have its breed specific model components yet, care/personality values, or individual details like an age and breed set yet - those are created/set in this class.
    private Dictionary<DogDataField, List<GameObject>> modelList = new Dictionary<DogDataField, List<GameObject>>(); //!< A list of all the additional dog model components such as alternative ears, tails, and snouts.
    [SerializeField] private string dogModelPartsPath;   //!< Asset folder base directory path as a string to find the additional dog model parts in.

    private List<ModelOrientation> modelOrientations = new List<ModelOrientation>();                        //!< A list of all the defined ModelOrientations. Used to set the component's rotations/positions based on the description for such in their breed's BreedData field.
    private Dictionary<string, float> modelScalers = new Dictionary<string, float>();                       //!< A dictionary of paired scaling descriptions as defined in the BreedData.txt file with an actual value for how much the dog's GameObject should be scaled based on its key description.
    private Dictionary<DogDataField, Vector3> scalingDirections = new Dictionary<DogDataField, Vector3>();  //!< Which directions the modelScalers values should be applied in depending on the data field. (E.g. "Size" should be applied on all axies, whereas "Leg_Length" should only be applied on the Y-axis).

    [SerializeField] private string breedDataFileDir;    //!< A string file path to the directory BreedData.txt is being kept in.
    private DataTable breedData = new DataTable();       //!< A data table which will be populated by the data in BreedData.txt for easier/faster access to its data.
    private int dogBreedsDefined = 0;                    //!< How many dog breeds were defined in BreedData.txt - used to iterate through the list of breeds to find their data values. Assigned after the file has been read.


    /** \fn Start
    *  \brief Reads the BreedData.txt file and adds the data to the breedData DataTable, retrieves the dog model parts from the assets folder, defines the orientation + scaling descriptions as values, then finally defines the dog's care and personality values/states/bounds.
    */
    private void Start()
    {
        ////////////////// Read the Breed Data from the Dog Data Text File //////////////////
        string[] data = System.IO.File.ReadAllLines(Application.dataPath + breedDataFileDir);
        for (int i = 0; i < data.Length; i++) //Removes Blank Spaces from Data
        {
            data[i] = string.Concat(data[i].Where(c => !char.IsWhiteSpace(c)));
        }

        int numDataFields = DogDataField.GetNames(typeof(DogDataField)).Length;
        dogBreedsDefined = data.Length - 1;

        ////////////////// Convert the Data String Read from BreedData.txt into a DataTable //////////////////
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

        ////////////////// Model Component Orientations //////////////////
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
        modelList.Add(DogDataField.Snout_Kind, Resources.LoadAll<GameObject>(dogModelPartsPath + "/Snouts").ToList<GameObject>());
        modelList.Add(DogDataField.Ear_Kind, Resources.LoadAll<GameObject>(dogModelPartsPath + "/Ears").ToList<GameObject>());
        modelList.Add(DogDataField.Tail_Kind, Resources.LoadAll<GameObject>(dogModelPartsPath + "/Tails").ToList<GameObject>());

        ////////////////// Initialise the States Possessed by Each Dog Property Value //////////////////
        DefineDogPropStates();
    }

    /** \fn DefineDogPropStates
    *  \brief Listing the states of each DogCareValue and DogPersonality value then adding them to the appropriate states dictionary.
    */
    private void DefineDogPropStates()
    {
        //////////////////// Care Value States ////////////////////
        string[] hungerState = { "Starving", "Hungry", "Fed", "Overfed" };
        Vector2[] hungerStateRanges = { new Vector2(0, 15), new Vector2(0, 50), new Vector2(50, 100), new Vector2(100, 100) };

        string[] attentionState = { "Lonely", "Loved", "Overcrowded" };
        Vector2[] attentionStateRanges = { new Vector2(0, 50), new Vector2(50, 100), new Vector2(100, 100) };

        string[] restState = { "Exhausted", "Tired", "Rested", "Rejuvinated" };
        Vector2[] restStateRanges = { new Vector2(0, 20), new Vector2(0, 50), new Vector2(50, 100), new Vector2(100, 100) };

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

    /** \fn AddStatesToProperty
     *  \brief Used by DefineDogPropStates() to add the listed string and Vector2 bounds arrays to the careValueStates dictionary after checking an equal number of state names to ranges have been created.
     */
    private void AddStatesToProperty(DogCareValue property, string[] names, Vector2[] ranges)
    {
        if (names.Length != ranges.Length) { Debug.LogWarning("Inequal number of state names to ranges attempting to be added to: " + property); return; }
        Dictionary<string, Vector2> states = new Dictionary<string, Vector2>();
        for (int i = 0; i < names.Length; i++) states.Add(names[i], ranges[i]);
        careValueStates.Add(property, states);
    }

    /** \fn AddStatesToProperty
     *  \brief Used by DefineDogPropStates() to add the listed string and Vector2 bounds arrays to the personalityValueStates dictionary after checking an equal number of state names to ranges have been created.
     */
    private void AddStatesToProperty(DogPersonalityValue property, string[] names, Vector2[] ranges)
    {
        if (names.Length != ranges.Length) { Debug.LogWarning("Inequal number of state names to ranges attempting to be added to: " + property); return; }
        Dictionary<string, Vector2> states = new Dictionary<string, Vector2>();
        for (int i = 0; i < names.Length; i++) states.Add(names[i], ranges[i]);
        personalityValueStates.Add(property, states);
    }

    /** \fn GetBreedValue
     *  \brief Returns the given data field for the given breed from the data table as a string. (E.g. Calling with the parameters as French_Bulldog & Size will return "Small").
     */
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

    /** \fn DEBUG_GenerateAllDogs
     *  \brief Not for actual use. Just an informal function for debugging purposes to check all dogs are generated correctly.
     */
    public void DEBUG_GenerateAllDogs()
    {
        float dogSpace = 3;
        Vector3 posOffset = Vector3.zero;
        posOffset.y = dogSpace;

        foreach (DogBreed dogBreed in (DogBreed[])DogBreed.GetValues(typeof(DogBreed)))
        {
            GameObject dog = GenerateDog(dogBreed).gameObject;
            dog.transform.position = posOffset;
            posOffset.x += dogSpace;
        }
    }

    /** \fn GenerateRandomNewDog
     *  \brief Called by GameTime to generate a new dog of a random breed from the DogBreed enum list.
     */
    public void GenerateRandomNewDog()
    {
        GenerateDog((DogBreed)UnityEngine.Random.Range(0, dogBreedsDefined));
    }

    /** \fn GenerateDog
     *  \brief Calls all the required functions to instantiate a working new Dog object with data values and model components/modifications set specific to the chosen parameter breed. 
     */
    private Dog GenerateDog(DogBreed breed)
    {
        ///// Instantiate the Prefab Dog Object /////
        Dog newDog = Instantiate(dogPrefabBase, Vector3.zero, Quaternion.identity);
        newDog.transform.parent = transform.GetChild(0);
        newDog.name = "Unnamed_" + breed;

        ///// Set General Breed Specific Data /////
        newDog.m_breed = breed;
        newDog.m_maxAge = int.Parse(GetBreedValue(breed, DogDataField.Max_Age));
        newDog.m_age = UnityEngine.Random.Range(1, newDog.m_maxAge);
        DefineDogProperties(newDog); // Set dog's care/personality values/states.

        ///// Modify Dog's Physical Apperance/GameObjects to Correlate with Breed Data /////
        FinaliseDogBody(newDog.m_breed, newDog.m_body);

        ///// Set Dog's Collider to Match New Dimensions in Render Space /////
        Renderer[] renderers = newDog.transform.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        bounds.center = newDog.m_body[BodyPart.Waist].m_component.transform.position;
        for (int i = 1, ni = renderers.Length; i < ni; i++) { bounds.Encapsulate(renderers[i].bounds); }
        /// Add some extra scale to collider for Item collision detection:
        bounds.size += new Vector3(0.5f, 0.25f, 1.0f);
        bounds.center += new Vector3(0.0f, -0.25f, 1.0f);
        newDog.m_collider.center = bounds.center;
        newDog.m_collider.size = bounds.size;

        ///// Add New Dog to Controller List /////
        controller.AddDog(newDog);
        return newDog;
    }

    /** \fn DefineDogProperties
     *  \brief Adds all the previously defined Care and Personality states/bounds to the new dog's list of these values as CareProperties and PersonalityProperties. For the Personality properties their values are also assigned here based on their breed's personality value rankings from BreedData.txt.
     */
    private void DefineDogProperties(Dog dog)
    {
        ///// Instantiate Dog's Care Properties & Decrement /////
        foreach (DogCareValue careValue in (DogCareValue[])DogCareValue.GetValues(typeof(DogCareValue)))
        {
            if (careValue != DogCareValue.NONE) dog.m_careValues.Add(careValue, new CareProperty(careValueStates[careValue], defaultCareDecrement));
        }

        ///// Instantiate Dog's Personality Properties based on Breed /////
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
            else if (personalityValue == DogPersonalityValue.Bond) // Bond was moved from previously being a care value to being a personality value, as I thought this should be slower changing but still begining as a standard value irregardless of breed. 
            {
                dog.m_personalityValues.Add(personalityValue, new PersonalityProperty(personalityValueStates[personalityValue], 1.0f));
            }
        }
    }

    /** \fn FinaliseDogBody
     *  \brief Instantiates the remaining dog body parts not included in the base prefab as they vary by breed. Adds them to the dog's body list then modifies the components (previously existing AND new) to match their descriptions set per breed in BreedData.txt.
     */
    private void FinaliseDogBody(DogBreed breed, Dictionary<BodyPart, BodyComponent> dog)
    {
        ///// Set Non-Existing Body Part Hierarchy Parents /////
        Dictionary<BodyPart, GameObject> parentAssignments = new Dictionary<BodyPart, GameObject>()
        {
            { BodyPart.Ear0, dog[BodyPart.Head].m_component  },
            { BodyPart.Ear1, dog[BodyPart.Head].m_component  },
            { BodyPart.Snout, dog[BodyPart.Head].m_component },
            { BodyPart.Tail, dog[BodyPart.Rear].m_component  }
        };

        ///// Set Non-Existing Body Part's Applicable Data Fields /////
        Dictionary<BodyPart, DogDataField[]> dataAssignments = new Dictionary<BodyPart, DogDataField[]>()
        {
            { BodyPart.Ear0, new DogDataField[]  { DogDataField.Ear_Kind, DogDataField.Ear_Orientation   }},
            { BodyPart.Ear1, new DogDataField[]  { DogDataField.Ear_Kind, DogDataField.Ear_Orientation   }},
            { BodyPart.Snout, new DogDataField[] { DogDataField.Snout_Kind, DogDataField.Snout_Length    }},
            { BodyPart.Tail, new DogDataField[]  { DogDataField.Tail_Kind, DogDataField.Tail_Orientation }}
        };

        ///// Create Non-Existing Body Part Objects /////
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

        ///// Lists of Components to Scale and Rotate /////
        /// (These are added to lists first so all scaling can be done first, THEN rotations). ///
        Dictionary<BodyPart, DogDataField> compsToScale = new Dictionary<BodyPart, DogDataField>();
        Dictionary<BodyPart, DogDataField> compsToRotate = new Dictionary<BodyPart, DogDataField>();

        ///// Identify Which Dog Body Parts Need Scaling or Orienting /////
        foreach (KeyValuePair<BodyPart, BodyComponent> part in dog)
        {
            foreach (DogDataField entry in part.Value.GetDataList())
            {
                if (entry.ToString().Contains("Length")) { compsToScale.Add(part.Key, entry); }
                if (entry.ToString().Contains("Orientation")) { compsToRotate.Add(part.Key, entry); }
            }
        }
        ///// Scale then Orient Required Body Parts to Breed Specifications /////
        foreach (KeyValuePair<BodyPart, DogDataField> entry in compsToScale) { SetComponentScale(breed, dog[entry.Key], entry.Value); }
        foreach (KeyValuePair<BodyPart, DogDataField> entry in compsToRotate) { SetComponentOrientations(breed, dog[entry.Key], entry.Value); }

        ///// Scale Entire Dog to Breed Size Specification /////
        dog[BodyPart.Waist].m_component.transform.localScale += scalingDirections[DogDataField.Size] * modelScalers[GetBreedValue(breed, DogDataField.Size)];
    }

    /** \fn CreateComponentModel
     *  \brief Used by FinaliseDogBody() to create the missing dog components. This function gets which component model should be instantiated based on the given parameter breed then sets the new object's hierarchy parent as the object specified in FinaliseDogBody().
     */
    private GameObject CreateComponentModel(DogBreed breed, GameObject parent, DogDataField modelKind)
    {
        List<GameObject> modelOptions = modelList[modelKind]; // Shorten the list of model options down from the full model list to just the ones of the body part kind. (E.g. Tails, Snouts, etc...).
        string breedModel = GetBreedValue(breed, modelKind);  // Get this breed's specified model for this body part.

        // Find the correct model for the breed:
        foreach (GameObject model in modelOptions)
        {
            if (model.name == breedModel) 
            {
                // Instantiate then Parent New Body Part:
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

    /** \fn SetComponentScale
     *  \brief Sets the given body part component to have the scale that it should for its breed (e.g. X-Short legs for Corgis and X-Long legs for Dachshunds).
     */
    private void SetComponentScale(DogBreed breed, BodyComponent component, DogDataField scalingDesc)
    {
        string breedScale = GetBreedValue(breed, scalingDesc);

        if (scalingDirections.ContainsKey(scalingDesc)) // Check this is actually a valid scaling description that can be found in BreedData.txt.
        {
            foreach (KeyValuePair<string, float> scale in modelScalers)
            {
                // Find matching scale to this breed's given description:
                if (scale.Key == breedScale)
                {
                    Vector3 newScale = scalingDirections[scalingDesc] * scale.Value; // Multiply scaling amount specification by the data field's specified scaling directions.

                    // Get all Joint child objects of this body part and store their current scales so they can be reverted back to their former scales. (Don't scale children).
                    Dictionary<GameObject, Vector3> children = new Dictionary<GameObject, Vector3>();
                    for (int i = 0; i < component.m_component.transform.childCount; i++)
                    {
                        GameObject child = component.m_component.transform.GetChild(i).gameObject;
                        if (child.tag == "Joint")
                        {
                            children.Add(child, child.transform.localScale);
                        }
                    }

                    // Scale the body part:
                    component.m_component.transform.localScale += newScale;

                    // Revert scaling on children.
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

    /** \fn SetComponentOrientations
     *  \brief Sets the given body part component to have the rotation/position (orientation) that it should for its breed (e.g. Downwards side ears for Shih-Tzus and Upwards top ears for French Bulldogs).
     */
    private void SetComponentOrientations(DogBreed breed, BodyComponent component, DogDataField orientationDesc)
    {
        string breedOrientation = GetBreedValue(breed, orientationDesc);
        foreach (ModelOrientation orientation in modelOrientations)
        {
            // Find matching orientation to this breed's given description:
            if ((orientation.m_type == orientationDesc) && (orientation.m_description == breedOrientation)) 
            {
                bool mirrored = component.GetPartType().ToString().Contains("1"); // Determine if this component should use the mirrored orientation values.

                // Set component position if specified to for this orientation:
                if (orientation.m_usePosition)
                {
                    // Use mirrored or regular position:
                    if (!mirrored) { component.m_component.transform.localPosition = orientation.m_position; }
                    else { component.m_component.transform.localPosition = orientation.m_mirrorPos; }
                }

                // Set rotation as regular or as mirror orientation values:
                if (!mirrored) { component.m_component.transform.localEulerAngles = orientation.m_rotation; }
                else
                {
                    component.m_component.transform.localEulerAngles = orientation.m_mirrorRot;

                    // If it's mirrored, flip the component's scale on the X-axis:
                    Vector3 mirroredScale = component.m_component.transform.localScale;
                    mirroredScale.x = -mirroredScale.x;
                    component.m_component.transform.localScale = mirroredScale;
                }
                return;
            }
        }
        Debug.LogWarning(breed + " has the following invalid data entry for: " + orientationDesc); return;
    }
}