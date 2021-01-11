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

    private List<Consumable> m_freeFoodBowls = new List<Consumable>();
    private Consumable m_currentFoodTarget = null;
    private bool m_eating = false;

    private void Awake()
    {
        m_controller = transform.parent.GetComponent<DogController>();
        m_controller.InitializeCareProperties(m_careValues);
        m_controller.InitializePersonalityProperties(m_personalityValues);

        navigationScript = GetComponent<Pathfinding>();
        UIOutputScript = infoPanelObject.GetComponent<DataDisplay>();

        Dictionary<Type, State> newStates = new Dictionary<Type, State>();
        newStates.Add(typeof(Hungry), new Hungry(this));
        //newStates.Add(typeof(Tired), new Tired(this));

        //newStates.Add(typeof(Idle), new Idle(this));
        //newStates.Add(typeof(Distressed), new Distressed(this));
        //newStates.Add(typeof(Happy), new Happy(this));

        //newStates.Add(typeof(AngerMania), new AngerMania(this));
        //newStates.Add(typeof(FearMania), new FearMania(this));
        //newStates.Add(typeof(ExcitementMania), new ExcitementMania(this));

        //newStates.Add(typeof(Pause), new Pause(this));
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
            if ((careProperty.GetPropertyName() == "Hunger") && m_eating)
            {
                careProperty.UpdateValue(m_currentFoodTarget.GetFufillmentValue());
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
                if (careProperty.GetValue() <= 40.0f)
                {
                    return true;
                }
                return false;
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
                return true;
            }
        }
        return false;
    }

    public bool Eating()
    {
        return m_eating;
    }

    public bool FindFood()
    {
        m_currentFoodTarget = null;
        int nodeDistanceToBowl = 1000;
        m_freeFoodBowls = m_controller.GetActiveBowlObjects();

        foreach (Consumable bowl in m_freeFoodBowls)
        {
            if ((bowl.GetUser() == null) && bowl.GetUsable())
            {
                navigationScript.FindPathTo(bowl.GetObject());
                int dist = navigationScript.GetFoundPathLength();

                if ((dist < nodeDistanceToBowl) && (dist > 0))
                {
                    m_currentFoodTarget = bowl;
                    nodeDistanceToBowl = dist;
                }
            }
        }

        if (m_currentFoodTarget != null)
        {
            return true;
        }
        return false;
    }

    public bool AttemptToEat()
    {
        if (m_currentFoodTarget == null)
        {
            FindFood();
            return false;
        }
        else if (navigationScript.FollowPathTo(m_currentFoodTarget.GetObject()))
        {
            foreach (Property careValue in m_careValues)
            {
                if (careValue.GetPropertyName() == "Hunger")
                {
                    if (((m_currentFoodTarget.GetUser() == null) || (m_currentFoodTarget.GetUser() == gameObject)) && m_currentFoodTarget.GetUsable())
                    {
                        m_currentFoodTarget.StartUse(gameObject);
                        StartCoroutine(Eat(5));
                        return true;
                    }
                }
            }

        }
        return false;
    }


    private IEnumerator Eat(float eatTime)
    {
        m_eating = true;
        yield return new WaitForSeconds(eatTime);
        m_eating = false;
        m_currentFoodTarget.EndUse();
        m_currentFoodTarget = null;
    }
}
