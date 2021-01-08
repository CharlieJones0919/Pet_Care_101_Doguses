using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Property
{
    private string m_property;
    private float m_value;
    private Dictionary<string, bool> m_states = new Dictionary<string, bool>();

    public Property(string property, List<string> states)
    {
        m_property = property;

        foreach (string newState in states)
        {
            m_states.Add(newState, false);
        }
    }

    public string GetPropertyName() { return m_property; }
    public float GetValue() { return m_value; }
    public void SetValue(float value) { m_value = value; }
}

public class Dog : MonoBehaviour
{
    public GameObject testTarget;  //!< Test target object to find paths to.

    public GameObject infoPanelObject;
    private InfoPanel infoPanelScript;
    private Pathfinding navigationScript;

    public string m_name;
    public string m_breed;
    public int m_age;

    public List<Property> careValues = new List<Property>();
    public List<Property> personalityValues = new List<Property>();

    // Start is called before the first frame update
    void Start()
    {
        navigationScript = GetComponent<Pathfinding>();
        infoPanelScript = infoPanelObject.GetComponent<InfoPanel>();

        //////////////////// SETTING CARE VALUE PROPERTIES ////////////////////

        List<string> hungerStates = new List<string>();
        hungerStates.Add("Starving");
        hungerStates.Add("Fed");
        hungerStates.Add("Overfed");
        careValues.Add(new Property("Hunger", hungerStates));

        List<string> attentionStates = new List<string>();
        attentionStates.Add("Lonely");
        attentionStates.Add("Loved");
        attentionStates.Add("Overcrowded");
        careValues.Add(new Property("Attention", attentionStates));

        List<string> restStates = new List<string>();
        restStates.Add("Exhausted");
        restStates.Add("Tired");
        restStates.Add("Rested");
        careValues.Add(new Property("Rest", restStates));

        List<string> hygieneStates = new List<string>();
        hygieneStates.Add("Filthy");
        hygieneStates.Add("Dirty");
        hygieneStates.Add("Clean");
        careValues.Add(new Property("Hygiene", hygieneStates));

        List<string> healthStates = new List<string>();
        healthStates.Add("Dying");
        healthStates.Add("Sick");
        healthStates.Add("Good");
        careValues.Add(new Property("Health", healthStates));

        List<string> happinessStates = new List<string>();
        happinessStates.Add("Distressed");
        happinessStates.Add("Upset");
        happinessStates.Add("Happy");
        careValues.Add(new Property("Happiness", happinessStates));

        List<string> bondStates = new List<string>();
        bondStates.Add("Wary");
        bondStates.Add("Friendly");
        careValues.Add(new Property("Bond", bondStates));

        //////////////////// SETTING PERSONALITY VALUE PROPERTIES ////////////////////
        
        List<string> toleranceStates = new List<string>();
        toleranceStates.Add("Nervous");
        toleranceStates.Add("Neutral");
        toleranceStates.Add("Calm");
        personalityValues.Add(new Property("Tolerance", toleranceStates));
 
        List<string> affectionStates = new List<string>();
        affectionStates.Add("Aggressive");
        affectionStates.Add("Grouchy");
        affectionStates.Add("Apathetic");
        affectionStates.Add("Friendly");
        affectionStates.Add("Loving");
        personalityValues.Add(new Property("Affection", affectionStates));
         
        List<string> intelligenceStates = new List<string>();
        intelligenceStates.Add("Dumb");
        intelligenceStates.Add("Average");
        intelligenceStates.Add("Smart");
        personalityValues.Add(new Property("Intelligence", intelligenceStates));
         
        List<string> energyStates = new List<string>();
        energyStates.Add("Sleepy");
        energyStates.Add("Normal");
        energyStates.Add("Hyper");
        personalityValues.Add(new Property("Energy", energyStates));
         
        List<string> obedienceStates = new List<string>();
        obedienceStates.Add("Bad");
        obedienceStates.Add("Good");
        personalityValues.Add(new Property("Obedience", obedienceStates));
    }

    // Update is called once per frame
    void Update()
    {
        if (!InFocus())
        {
            //navigationScript.FollowPathTo(testTarget);
        }
        else if (!infoPanelObject.active || (infoPanelScript != transform.gameObject))
        {
            infoPanelScript.SetFocusDog(this);
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
}
