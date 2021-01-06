using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Dog : MonoBehaviour
{
    private Pathfinding navigation;
    public GameObject testTarget;  //!< Test target object to find paths to.

    public GameObject infoPanelObject;
    private InfoPanel infoPanelScript;

    private Dictionary<string, float> m_personalityValues;
    private Dictionary<string, int> m_careValues;

    public string m_name;
    public string m_breed;
    public int m_age;

    public int m_hunger;
    public int m_attention;
    public int m_rest;
    public int m_hygiene;
    public int m_health;
    public int m_happiness;
    public int m_bond;

    public float m_tolerance;
    public float m_affection;
    public float m_intelligence;
    public float m_energy;
    public float m_obedience;

    // Start is called before the first frame update
    void Start()
    {
       // m_name = null;

        //m_careValues.Add("hunger", 100);
        //m_careValues.Add("attention", 100);
        //m_careValues.Add("rest", 100);
        //m_careValues.Add("hygiene", 100);
        //m_careValues.Add("bond", 100);
        //m_careValues.Add("happiness", 100);
        //m_careValues.Add("health", 100);

        navigation = GetComponent<Pathfinding>();
        infoPanelScript = infoPanelObject.GetComponent<InfoPanel>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!InFocus())
        {
            navigation.FollowPathTo(testTarget);
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

    public void setName(string name)
    {
        m_name = name;
    }

    public string getName()
    {
        return m_name;
    }
}
