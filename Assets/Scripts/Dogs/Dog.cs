using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Dog : MonoBehaviour
{
    [SerializeField] private GameObject infoPanelObject;
    public DogController m_controller;
    private DataDisplay UIOutputScript;
    private Pathfinding navigationScript;

    public string m_name;
    public string m_breed;
    public int m_age;

    public List<Property> m_careValues = new List<Property>();
    public List<Property> m_personalityValues = new List<Property>();

    public Dictionary<string, bool> m_facts = new Dictionary<string, bool>();
    public List<Rule> m_rules = new List<Rule>();

    private List<Item> m_freeItems = new List<Item>();
    private Item m_currentItemTarget = null;
   private bool m_usingItem = false;
   private bool m_waiting = true;

    private void Awake()
    {
        m_controller = transform.parent.GetComponent<DogController>();
        m_controller.InitializeCareProperties(m_careValues);
        m_controller.InitializePersonalityProperties(m_personalityValues);

        navigationScript = GetComponent<Pathfinding>();
        UIOutputScript = infoPanelObject.GetComponent<DataDisplay>();

        Dictionary<Type, State> newStates = new Dictionary<Type, State>();
        newStates.Add(typeof(Pause), new Pause(this));
        newStates.Add(typeof(Idle), new Idle(this));
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

        GetComponent<FiniteStateMachine>().SetStates(newStates);


        //////////////////// Set RBS Facts and Rules ////////////////////
        //m_facts.Add("HUNGRY", true);

        //m_facts.Add("S2S_Hungry", true);

        //m_rules.Add(new Rule("HUNGRY", "S2S_Hungry", Rule.Predicate.And, typeof(Hungry)));
    }

    // Update is called once per frame
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

#if !UNITY_EDITOR
     private bool InFocus()
    {
        if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit raycastHit;

            if (Physics.Raycast(raycast, out raycastHit))
            {
                Debug.Log("Selected: " + raycastHit.collider.tag);

                if (raycastHit.collider.tag == transform.tag) return true;
            }
        }
        return false;
    }
#else
    private bool InFocus()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;

            if (Physics.Raycast(raycast, out raycastHit))
            {
                Debug.Log("Selected: " + raycastHit.collider.tag);
                if (raycastHit.collider.tag == transform.tag) return true;
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

    public bool Hungry()
    {
        foreach (Property careProperty in m_careValues)
        {
            if (careProperty.GetPropertyName() == "Hunger")
            {
                if (careProperty.GetValue() <= 50.0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool Tired()
    {
        foreach (Property careProperty in m_careValues)
        {
            if (careProperty.GetPropertyName() == "Rest")
            {
                if (careProperty.GetValue() <= 50.0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool Full()
    {
        foreach (Property careProperty in m_careValues)
        {
            if (careProperty.GetPropertyName() == "Hunger")
            {
                if (careProperty.GetValue() < 100.0f)
                {
                    return false;
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

    public bool FindItem(ItemType type)
    {
        m_currentItemTarget = null;
        int nodeDistanceToBowl = 1000;
        m_freeItems = m_controller.GetActiveObjects(type);

        foreach (Item item in m_freeItems)
        {
            if ((item.GetUser() == null) && item.GetUsable())
            {
                navigationScript.FindPathTo(item.GetObject());

                int dist = navigationScript.GetFoundPathLength();
                if ((dist < nodeDistanceToBowl) && (dist > 0))
                {
                    m_currentItemTarget = item;
                    nodeDistanceToBowl = dist;
                }
            }
        }

        if (m_currentItemTarget != null)
        {
            return true;
        }
        return false;
    }

    public bool AttemptToUseItem(ItemType type)
    {
        if (m_currentItemTarget == null)
        {
            FindItem(type);
            return false;
        }
        else if (navigationScript.FollowPathTo(m_currentItemTarget.GetObject()))
        {
            if (((m_currentItemTarget.GetUser() == null) || (m_currentItemTarget.GetUser() == gameObject)) && m_currentItemTarget.GetUsable())
            {
                m_currentItemTarget.StartUse(gameObject);
                return true;
            }
        }
        return false;
    }

    public IEnumerator UseItem(float useTime, bool willRemainUsableAfter)
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
            m_usingItem = false;

            m_currentItemTarget.EndUse(willRemainUsableAfter);
            m_currentItemTarget = null;
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
        m_waiting = false;
    }

}
