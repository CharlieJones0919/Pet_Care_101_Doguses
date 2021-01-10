using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Dog : MonoBehaviour
{
    public DogController m_controller;

    [SerializeField] private GameObject infoPanelObject;
    private DataDisplay UIOutputScript;
    private Pathfinding navigationScript;

    public string m_name;
    public string m_breed;
    public int m_age;

    public Dictionary<Property, float> m_careValues = new Dictionary<Property, float>();
    public List<Property> m_personalityValues = new List<Property>();

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
    }

    // Update is called once per frame
    void Update()
    {
        if (!InFocus())
        {
            //navigationScript.FollowPathTo(testTarget);
        }
        else if (!infoPanelObject.active)
        {
            if (UIOutputScript.GetFocusDog() != gameObject)
            {
                UIOutputScript.SetFocusDog(this);
            }

            infoPanelObject.SetActive(true);
        }

        UpdateCareValues();
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
        foreach (KeyValuePair<Property, float> careProperty in m_careValues)
        {
            careProperty.Key.UpdateValue(careProperty.Value);
            //Debug.Log(careProperty.Key.GetPropertyName() + ": " + careProperty.Key.GetValue());
        }
    }
}
