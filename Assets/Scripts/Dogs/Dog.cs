/** \file Dog.cs */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

/** \class Dog
*  \brief This class contains the implementations of functions (or behaviour actions) that can be employed by the dog's FSM states, and the individual dog's personality and care state values.
*/
public class Dog : MonoBehaviour
{
    public DogController m_controller;     //!< Reference to the game's DogController script to retrieve data required by all dogs. 
    public GameObject infoPanelObject;     //!< Reference to the dog information UI panel to send this object's instance of this script to so if this dog is tapped on its data can be displayed. 

    private DataDisplay UIOutputScript;     //!< Script from the infoPanelObject.
    public Pathfinding navigationScript;   //!< Instance of the Pathfinding script for the dog to utalise for navigation around the map.

    public string m_name;   //!< This dog's name. 
    public DogBreed m_breed;  //!< The breed of this dog.
    public int m_age;       //!< Age of this dog - how long since it has was instantiated in game time. (Not yet implemented).
    public Dictionary<BodyPart, BodyComponent> m_body = new Dictionary<BodyPart, BodyComponent>();

    public BoxCollider m_collider;

    public List<CareProperty> m_careValues = new List<CareProperty>();          //!< A list of the dog's current care value properties so they can be easily iterated through.
    public List<PersonalityProperty> m_personalityValues = new List<PersonalityProperty>();   //!< A list of the dog's personality value properties so they can be easily iterated through. (Have not been implemented in full).

    // Will be used to store definitions for the dog's different "facts" and rules based on them.
    public Dictionary<string, bool> m_facts = new Dictionary<string, bool>();
    public List<Rule> m_rules = new List<Rule>();

    private Item m_currentItemTarget = null;  //!< An item on the map the dog is currently using or travelling towards.
    private Item m_prospectItemTarget = null; //!< An item on the map the dog could use or travel towards.
    private bool m_usingItem = false;         //!< Whether the dog is currently using an item.
    private bool m_waiting = true;            //!< Whether the dog is currently paused and waiting to stop pausing.

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////// BEHAVIOURAL TREE ////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Action nodes which are children to sequence nodes which must be successful in a sequence for the sequence node to succeed.

    //Swap to different FSM state actions.
    public BTAction swp_pause;
    public BTAction swp_idle;
    public BTAction swp_hungry;
    public BTAction swp_tired;

    public BTSequence seq_Pause;

    /** \fn Awake
    *  \brief Callled once when the scene loads to instantiate variable values and functions before the application starts. Used to define and add states to the FSM.
    */
    private void Awake()
    {
        //Define the dog's FSM states then add them to the object's FSM. (Implementation is not finished yet).
        Dictionary<Type, State> newStates = new Dictionary<Type, State>();

        newStates.Add(typeof(Pause), new Pause(this));
        newStates.Add(typeof(Idle), new Idle(this));    //When finished this will be the default starting state. (Wandering until current care values need attending to).
        newStates.Add(typeof(Hungry), new Hungry(this));
        newStates.Add(typeof(Tired), new Tired(this));

        //newStates.Add(typeof(Distressed), new Distressed(this));
        //newStates.Add(typeof(Happy), new Happy(this));

        //newStates.Add(typeof(AngerMania), new AngerMania(this));
        //newStates.Add(typeof(FearMania), new FearMania(this));
        //newStates.Add(typeof(ExcitementMania), new ExcitementMania(this));

        //newStates.Add(typeof(Play), new Play(this));
        //newStates.Add(typeof(Grabbed), new Grabbed(this));
        //newStates.Add(typeof(Interact), new Interact(this));
        //newStates.Add(typeof(Inspect), new Inspect(this));

        //newStates.Add(typeof(RunAway), new RunAway(this));
        //newStates.Add(typeof(Bite), new Bite(this));

        //newStates.Add(typeof(Die), new Die(this));

        GetComponent<FiniteStateMachine>().SetStates(newStates); //Add defined states to FSM.
        GetExistingBodyParts();
    }

    void Start()
    {
        //Get other required components from this object.
        navigationScript = GetComponent<Pathfinding>();
        UIOutputScript = infoPanelObject.GetComponent<DataDisplay>();

        Vector3 spawnPoint = Vector3.zero;
        while (spawnPoint == Vector3.zero)
        {
            spawnPoint = navigationScript.GetRandomPointInWorld();
        }
        spawnPoint.y = navigationScript.groundPlane.transform.position.y - m_body[BodyPart.Foot0].m_component.transform.position.y;
        transform.position = spawnPoint;

        ///////// Rules /////

        ////m_facts.Add("paused", true);
        ////m_facts.Add("idle", false);
        ////m_facts.Add("hungry", false);
        ////m_facts.Add("tired", false);

        ////m_facts.Add("swp2_pause", false);
        ////m_facts.Add("swp2_idle", false);
        ////m_facts.Add("swp2_hungry", false);
        ////m_facts.Add("swp2_tired", false);

        ////m_rules.Add(new Rule("paused", "swp2_idle", Rule.Predicate.And, typeof(Idle)));





        seq_Pause = new BTSequence(new List<BTNode> {  });



        ///////// Behaviour Trees /////
        //swp_pause = new BTAction(SwapToSearch);

    }


    /** \fn Update
     *  \brief Called every frame on a loop to check if the dog has been tapped on (is InFocus) and updates the dog's current care values with time progression.
     *  */
    private void Update()
    {
        UpdateCareValues();

        if (InFocus())
        {
            infoPanelObject.SetActive(false);

            if (UIOutputScript.GetFocusDog() != gameObject)
            {
                UIOutputScript.SetFocusDog(this);
            }

            infoPanelObject.SetActive(true);
        }
    }

    private void GetExistingBodyParts()
    {
        m_collider = gameObject.GetComponent<BoxCollider>();

        var allChildObjects = transform.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildObjects)
        {
            if (Enum.IsDefined(typeof(BodyPart), child.name))
            {
                BodyPart type = (BodyPart)Enum.Parse(typeof(BodyPart), child.name);
                m_body.Add(type, new BodyComponent(type, child.gameObject, child.transform.parent.gameObject));
               // Debug.Log("Found " + child.name + " and set as " + type);
            }
        }

        m_body[BodyPart.Waist].SetData(DogDataField.Size);
        m_body[BodyPart.Chest].SetData(DogDataField.Body_Length);
        m_body[BodyPart.Rear].SetData(DogDataField.Body_Length);
        foreach (BodyPart part in (BodyPart[])Enum.GetValues(typeof(BodyPart)))
        {
            if (part.ToString().Contains("Leg")) { m_body[part].SetData(DogDataField.Leg_Length); }
        }
    }

    /** \fn InFocus
     *  \brief An API agnostic function to check whether the dog has been tapped or clicked on by the player. If in the editor it's defined by the function checking for mouse input but is otherwise defined by the function checking for touch input.
     *  */
#if UNITY_EDITOR //If in the editor, check for mouse input.
    private bool InFocus() //If presumably in the editor check for left mouse button click input.
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;

            if (Physics.Raycast(raycast, out raycastHit))
            {
                Debug.Log("Selected: " + raycastHit.collider.gameObject.name);
                if (raycastHit.collider == m_collider) return true;
            }
        }
        return false;
    }
#elif UNITY_IOS || UNITY_ANDROID //If not in the editor check for touch input. 
    private bool InFocus()
    {
        if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began)) //Gets first touch input.
        {
            Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position); //A raycast between the camera and touch position to get the world position of the touch.
            RaycastHit raycastHit;

            if (Physics.Raycast(raycast, out raycastHit)) //If the raycast hits anything...
            {
                Debug.Log("Selected: " + raycastHit.collider.gameObject.name); //Output the name of the object hit by the raycast.
                if (raycastHit.collider == m_collider) return true; //If the collider hit belongs to this object, this dog is in focus.
            }
        }
        return false;
    }
#endif

    public void UpdateCareValues()
    {
        foreach (CareProperty careProperty in m_careValues)
        {
            if (UsingItemFor() == careProperty.GetPropertyName())
            {
                careProperty.UpdateValue(m_currentItemTarget.GetFufillmentValue());
            }
            else
            {
                careProperty.UpdateValue(careProperty.GetCurrenntIncrement());
            }
        }
    }

    /** \fn Hungry
*  \brief A placeholder function for a rule of if the dog value qualifies as being in this state. Is currently based on an arbitrary value but will be replaced with a definition generated by a FIS.
*  */
    public bool Hungry()
    {
        foreach (CareProperty careProperty in m_careValues)
        {
            if (careProperty.GetPropertyName() == DogCareValue.Hunger)
            {
                if (careProperty.GetValue() <= 25.0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /** \fn Full
*  \brief A placeholder function for a rule of if the dog value qualifies as being in this state. Is currently based on an arbitrary value but will be replaced with a definition generated by a FIS.
*  */
    public bool Full()
    {
        foreach (CareProperty careProperty in m_careValues)
        {
            if (careProperty.GetPropertyName() == DogCareValue.Hunger)
            {
                if (careProperty.GetValue() >= 50.0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /** \fn Tired
    *  \brief A placeholder function for a rule of if the dog value qualifies as being in this state. Is currently based on an arbitrary value but will be replaced with a definition generated by a FIS.
    *  */
    public bool Tired()
    {
        foreach (CareProperty careProperty in m_careValues)
        {
            if (careProperty.GetPropertyName() == DogCareValue.Rest)
            {
                if (careProperty.GetValue() <= 30.0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool Rested()
    {
        foreach (CareProperty careProperty in m_careValues)
        {
            if (careProperty.GetPropertyName() == DogCareValue.Rest)
            {
                if (careProperty.GetValue() >= 75.0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public DogCareValue UsingItemFor()
    {
        if (m_usingItem)
        {
            return m_currentItemTarget.GetPropertySubject();
        }

        return DogCareValue.NONE;
    }

    public bool Waiting()
    {
        return m_waiting;
    }

    public Item FindItem(ItemType type)
    {
        m_prospectItemTarget = null;
        int nodeDistanceToBowl = 1000;

        foreach (Item item in m_controller.GetActiveObjects(type))
        {
            if ((item.GetUser() == gameObject) && item.GetUsable())
            {
                m_prospectItemTarget = item;
                break;
            }
            else if ((item.GetUser() == null) && item.GetUsable())
            {
                navigationScript.FindPathTo(item.GetObject());
                int dist = navigationScript.GetFoundPathLength();
                if (dist < nodeDistanceToBowl)
                {
                    m_prospectItemTarget = item;
                    nodeDistanceToBowl = dist;
                }
            }
        }

        if (m_prospectItemTarget != null) navigationScript.FindPathTo(m_prospectItemTarget.GetObject());
        return m_prospectItemTarget;
    }

    public bool AttemptToUseItem(ItemType type)
    {
        if (m_currentItemTarget == null)
        {
            if (FindItem(type) != null)
            {
                m_currentItemTarget = m_prospectItemTarget;
            }
            else
            {
                Debug.LogWarning("Can't find required item: " + type);
                return false;
            }
        }

        if (m_currentItemTarget != null)
        {
            if (navigationScript.FollowPathTo(m_currentItemTarget.GetObject()))
            {
                m_currentItemTarget.StartUse(gameObject);
                return true;
            }
        }

        return false;
    }

    public IEnumerator UseItem(float useTime)
    {
        if (m_currentItemTarget != null)
        {
            if (m_currentItemTarget.GetCentrePreference())
            {
                Vector3 usePosition = new Vector3(m_currentItemTarget.GetPosition().x, transform.position.y, m_currentItemTarget.GetPosition().z);
                transform.position = usePosition;
            }

            m_usingItem = true;
            yield return new WaitForSeconds(useTime);
            EndItemUse();
        }
        else
        {
            Debug.LogWarning("No item to use.");
        }
    }

    public void Wander()
    {
        navigationScript.FollowPathToRandomPoint();
    }

    public IEnumerator Pause(float waitTime)
    {
        m_waiting = true;
        yield return new WaitForSeconds(waitTime);
        StopWaiting();
    }

    public void EndItemUse()
    {
        if (m_currentItemTarget != null)
        {

            m_usingItem = false;
            m_currentItemTarget.EndUse();
            m_currentItemTarget = null;
        }
        else
        {
            Debug.LogWarning("No item to end use of.");
        }
    }

    public void StopWaiting()
    {
        m_waiting = false;
    }
}