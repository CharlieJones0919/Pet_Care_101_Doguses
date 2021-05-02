﻿/** \file Dog.cs */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/** \class Dog
*  \brief This class contains the implementations of functions (or behaviour actions) that can be employed by the dog's FSM states, and the individual dog's personality and care state values.
*/
public class Dog : MonoBehaviour
{
    public DogController controllerScript;     //!< Reference to the game's DogController script to retrieve data required by all dogs. 
    public DataDisplay UIOutputScript;     //!< Script from the infoPanelObject.
    public Pathfinding navigationScript;   //!< Instance of the Pathfinding script for the dog to utalise for navigation around the map.
    public GameObject defaultNULL;

    public string m_name;   //!< This dog's name. 
    public DogBreed m_breed;  //!< The breed of this dog.
    public int m_age;       //!< Age of this dog - how long since it has was instantiated in game time. (Not yet implemented).
    public Dictionary<BodyPart, BodyComponent> m_body = new Dictionary<BodyPart, BodyComponent>();

    public BoxCollider m_collider;
    public Animator m_animationCTRL;
    [SerializeField] private Rigidbody m_RB;

    private int lastDayUpdated = 0;

    public Dictionary<DogCareValue, CareProperty> m_careValues = new Dictionary<DogCareValue, CareProperty>(); //!< A list of the dog's current care value properties so they can be easily iterated through.
    public Dictionary<DogPersonalityValue, PersonalityProperty> m_personalityValues = new Dictionary<DogPersonalityValue, PersonalityProperty>();   //!< A list of the dog's personality value properties so they can be easily iterated through. (Have not been implemented in full).

    // Will be used to store definitions for the dog's different "facts" and rules based on them.
    public Dictionary<string, bool> m_facts = new Dictionary<string, bool>();
    public List<Rule> m_rules = new List<Rule>();

    private Item m_currentItemTarget;
    [SerializeField] private GameObject m_currentObjectTarget;  //!< An item on the map the dog is currently using or travelling towards.   

   public bool m_usingItem = false;         //!< Whether the dog is currently using an item.
public bool m_waiting = true;            //!< Whether the dog is currently paused and waiting to stop pausing.
    private Vector3 positionBeforeUsingItem;
    private Quaternion rotationBeforeUsingItem;

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

        //newStates.Add(typeof(Angery), new Angery(this));
        //newStates.Add(typeof(Scared), new Scared(this));
        //newStates.Add(typeof(Excited), new Excited(this));

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
        Vector3 spawnPoint = Vector3.zero;
        while (spawnPoint == Vector3.zero)
        {
            spawnPoint = navigationScript.GetRandomPointInWorld();
        }
        float ground2FootDiff = navigationScript.groundPlane.transform.position.y - m_body[BodyPart.Foot0].m_component.transform.position.y;
        spawnPoint.y = transform.position.y + ground2FootDiff;

        transform.position = spawnPoint;
        m_currentObjectTarget = defaultNULL;


        //  navigationScript.requiredSpace = m_dimensions.size;

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





        seq_Pause = new BTSequence(new List<BTNode> { });



        ///////// Behaviour Trees /////
        //swp_pause = new BTAction(SwapToSearch);
    }


    /** \fn Update
     *  \brief Called every frame on a loop to check if the dog has been tapped on (is InFocus) and updates the dog's current care values with time progression.
     *  */
    private void Update()
    {
        UpdateCareValues();

        int currentDay = controllerScript.localTime.GetGameTimeDays();
        if (lastDayUpdated != currentDay)
        {
            for (int day = lastDayUpdated; lastDayUpdated < currentDay; lastDayUpdated--)
            {
                UpdatePersonalityValues();
            }
        }

        if (InFocus())
        {
            UIOutputScript.gameObject.SetActive(false);

            if (UIOutputScript.GetFocusDog() != gameObject)
            {
                UIOutputScript.SetFocusDog(this);
            }

            UIOutputScript.gameObject.SetActive(true);
        }

        m_animationCTRL.SetFloat("Speed", transform.InverseTransformDirection(m_RB.velocity).z);
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
            }
        }

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

            if (Physics.Raycast(raycast, out raycastHit, Mathf.Infinity))
            {
                Debug.Log("Selected: " + raycastHit.collider.gameObject.name);
                if (raycastHit.collider == m_collider) { return true; }
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

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == 8) { StartCoroutine(Pause(5)); };
    }

    private void UpdateCareValues()
    {
        foreach (KeyValuePair<DogCareValue, CareProperty> prop in m_careValues)
        {
            if (UsingItemFor(prop.Key))
            {
                prop.Value.UpdateValue(m_currentItemTarget.GetCareFufillmentAmount(prop.Key));
            }
            else { prop.Value.UpdateValue(prop.Value.GetUsualDecrement()); }
        }
    }

    private void UpdatePersonalityValues()
    {
        foreach (KeyValuePair<DogPersonalityValue, PersonalityProperty> prop in m_personalityValues)
        {
            if (UsingItemFor(prop.Key))
            {
                prop.Value.UpdateValue(m_currentItemTarget.GetPersonalityFufillmentAmount(prop.Key));
                break;
            }
        }
    }

    /** \fn Hungry
*  \brief A placeholder function for a rule of if the dog value qualifies as being in this state. Is currently based on an arbitrary value but will be replaced with a definition generated by a FIS.
*  */
    public bool Starving() { return m_careValues[DogCareValue.Hunger].IsState("Starving"); }
    public bool Hungry() {  return m_careValues[DogCareValue.Hunger].IsState("Hungry"); }
    public bool Fed() {     return m_careValues[DogCareValue.Hunger].IsState("Fed"); }
    public bool Overfed() { return m_careValues[DogCareValue.Hunger].IsState("Overfed"); }

    public bool Lonely() { return m_careValues[DogCareValue.Attention].IsState("Lonely"); }
    public bool Loved() { return m_careValues[DogCareValue.Attention].IsState("Loved"); }
    public bool Overcrowded() { return m_careValues[DogCareValue.Attention].IsState("Overcrowded"); }

    public bool Exhausted() { return m_careValues[DogCareValue.Rest].IsState("Exhausted"); }
    public bool Tired() { return m_careValues[DogCareValue.Rest].IsState("Tired"); }
    public bool Rested() { return m_careValues[DogCareValue.Rest].IsState("Rested"); }

    public bool Filthy() { return m_careValues[DogCareValue.Hygiene].IsState("Filthy"); }
    public bool Dirty() { return m_careValues[DogCareValue.Hygiene].IsState("Dirty"); }
    public bool Clean() { return m_careValues[DogCareValue.Hygiene].IsState("Clean"); }

    public bool Dying() { return m_careValues[DogCareValue.Health].IsState("Dying"); }
    public bool Sick() { return m_careValues[DogCareValue.Health].IsState("Sick"); }
    public bool Healthy() { return m_careValues[DogCareValue.Health].IsState("Healthy"); }

    public bool Distressed() { return m_careValues[DogCareValue.Happiness].IsState("Distressed"); }
    public bool Upset() { return m_careValues[DogCareValue.Happiness].IsState("Upset"); }
    public bool Happy() { return m_careValues[DogCareValue.Happiness].IsState("Happy"); }

    public string GetPersonalityState(DogPersonalityValue forProperty)
    {
        return m_personalityValues[forProperty].GetState();
    }

    public bool UsingItemFor(DogCareValue value)
    {
        if (m_usingItem)
        {
            return m_currentItemTarget.FufillsCareValue(value);
        }
        return false;
    }

    public bool UsingItemFor(DogPersonalityValue value)
    {
        if (m_usingItem)
        {
            return m_currentItemTarget.FufillsPersonalityValue(value);
        }
        return false;
    }

    public void SetCurrentTargetItem(Item newItem, GameObject itemInstance)
    {
        m_currentItemTarget = newItem;
        m_currentObjectTarget = itemInstance;
    }

    public bool ItemOfTypeFound(ItemType type)
    {
        if (m_currentObjectTarget != defaultNULL)
        {
            if (m_currentItemTarget.GetItemType() == type) { return true; }
        }

        if (controllerScript.GetClosestActiveItemFor(type, this))
        {
            if (m_currentObjectTarget != defaultNULL)
            {
                navigationScript.FindPathTo(m_currentObjectTarget);
                return true;
            }
        }
        return false;
    }

    public bool LocateItemFor(ItemType type)
    {
        if (m_currentObjectTarget == defaultNULL)
        {
            if (!ItemOfTypeFound(type)) { return false; }
        }

        if (!m_waiting)
        {
            if (navigationScript.AttemptToReach(m_currentObjectTarget))
            {
                if (m_currentItemTarget.UseItemInstance(gameObject, m_currentObjectTarget))
                {
                    positionBeforeUsingItem = transform.position;
                    rotationBeforeUsingItem = transform.rotation;
                    return true;
                }
                else  { m_currentObjectTarget = defaultNULL;  }
            }
        }
        return false;
    }

    public IEnumerator UseItem()
    {
        if (m_currentObjectTarget != defaultNULL)
        {
            Vector3 usePosition = m_currentItemTarget.GetUsePosition();
            usePosition.y = transform.position.y;
            transform.position = usePosition;
            transform.localRotation.SetLookRotation(m_currentItemTarget.GetUseRotation());

            m_usingItem = true;
            yield return new WaitForSeconds(m_currentItemTarget.GetUseTime());
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
        m_RB.velocity = Vector3.zero;
        yield return new WaitForSeconds(waitTime);
        StopWaiting();
    }

    public void EndItemUse()
    {
        if (m_currentObjectTarget != defaultNULL)
        {
            m_usingItem = false;
            m_currentItemTarget.StopUsingItemInstance(m_currentObjectTarget);
            m_currentItemTarget = null;
            m_currentObjectTarget = defaultNULL;
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

    public void SetBackToPreviousOrientation()
    {
        if (!m_usingItem)
        {
            transform.position = positionBeforeUsingItem;
            transform.rotation = rotationBeforeUsingItem;
        }
    }

    public void Spooked() { }
    public void Attack() { }
}