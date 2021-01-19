/** \file Dog.cs */
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
    [SerializeField] private GameObject infoPanelObject; //!< Reference to the dog information UI panel to send this object's instance of this script to so if this dog is tapped on its data can be displayed. 
    public DogController m_controller;      //!< Reference to the game's DogController script to retrieve data required by all dogs. 
    private DataDisplay UIOutputScript;     //!< Script from the infoPanelObject.
    private Pathfinding navigationScript;   //!< Instance of the Pathfinding script for the dog to utalise for navigation around the map.
    private Collider m_collider;


    public string m_name;   //!< This dog's name. 
    public string m_breed;  //!< The breed of this dog.
    public int m_age;       //!< Age of this dog - how long since it has was instantiated in game time. (Not yet implemented).

    public List<Property> m_careValues = new List<Property>();          //!< A list of the dog's current care value properties so they can be easily iterated through.
    public List<Property> m_personalityValues = new List<Property>();   //!< A list of the dog's personality value properties so they can be easily iterated through. (Have not been implemented in full).

    // WIll be used to store definitions for the dog's different "facts" and rules based on them.
    //public Dictionary<string, bool> m_facts = new Dictionary<string, bool>(); 
    //public List<Rule> m_rules = new List<Rule>();

    private Item m_currentItemTarget = null; //!< An item on the map the dog is currently using or travelling towards.
    private Item m_prospectItemTarget = null;//!< An item on the map the dog could use or travel towards.
    private bool m_usingItem = false;        //!< Whether the dog is currently using an item.
    private bool m_waiting = true;           //!< Whether the dog is currently paused and waiting to stop pausing.

    /** \fn Awake
    *  \brief Callled once when the scene loads to instantiate variable values and functions before the application starts. Used to define and add states to the FSM.
    */
    private void Awake()
    {
        //Get the controller script from this object's parent (the controller's object is the parent of all dog objects). Then initialise the dog's starting care and personality values from the script's given defaults.
        m_controller = transform.parent.GetComponent<DogController>();
        m_controller.InitializeCareProperties(m_careValues);
        m_controller.InitializePersonalityProperties(m_personalityValues);

        //Get other required components from this object.
        navigationScript = GetComponent<Pathfinding>();
        UIOutputScript = infoPanelObject.GetComponent<DataDisplay>();
        m_collider = gameObject.GetComponent<Collider>();

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
    }

    /** \fn Update
     *  \brief Called every frame on a loop to check if the dog has been tapped on (is InFocus) and updates the dog's current care values with time progression.
     *  */
    void Update()
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

    /** \fn InFocus
     *  \brief An API agnostic function to check whether the dog has been tapped or clicked on by the player. If in the editor it's defined by the function checking for mouse input but is otherwise defined by the function checking for touch input.
     *  */
#if UNITY_IOS || UNITY_ANDROID //If not in the editor check for touch input. 
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
#elif UNITY_EDITOR //If in the editor, check for mouse input.
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
#endif

    public void UpdateCareValues()
    {
        foreach (Property careProperty in m_careValues)
        {
            if (UsingItemFor() == careProperty.GetPropertyName())
            {
                careProperty.UpdateValue(m_currentItemTarget.GetFufillmentValue());
            }
            else
            {
                careProperty.UpdateValue(careProperty.GetIncrement());
            }
        }
    }

    /** \fn Hungry
*  \brief A placeholder function for a rule of if the dog value qualifies as being in this state. Is currently based on an arbitrary value but will be replaced with a definition generated by a FIS.
*  */
    public bool Hungry()
    {
        foreach (Property careProperty in m_careValues)
        {
            if (careProperty.GetPropertyName() == "Hunger")
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
        foreach (Property careProperty in m_careValues)
        {
            if (careProperty.GetPropertyName() == "Hunger")
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
        foreach (Property careProperty in m_careValues)
        {
            if (careProperty.GetPropertyName() == "Rest")
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
        foreach (Property careProperty in m_careValues)
        {
            if (careProperty.GetPropertyName() == "Rest")
            {
                if (careProperty.GetValue() >= 75.0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public string UsingItemFor()
    {
        if (m_usingItem)
        {
            return m_currentItemTarget.GetPropertySubject();
        }

        return null;
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
                if ((dist < nodeDistanceToBowl) && (dist > 0))
                {
                    m_prospectItemTarget = item;
                    nodeDistanceToBowl = dist;
                }
            }
        }

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
                Debug.Log("Can't find required item: " + type);
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
            Debug.Log("No item to use.");
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
            Debug.Log("No item to end use of.");
        }
    }

    public void StopWaiting()
    {
        m_waiting = false;
    }
}