using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public TipPopUp tipPopUp;
    public DataDisplay UIOutput;     //!< Script from the infoPanelObject.
    [SerializeField] private Text ffButtonText;

    [SerializeField] private List<Dog> allDogs = new List<Dog>();
    [SerializeField] private static int dogLimit = 8;

    private Dictionary<ItemType, List<Item>> itemPools = new Dictionary<ItemType, List<Item>>();
    public List<Vector3> permanentItemPositions = new List<Vector3>();
    public Dictionary<Vector3, bool> tempItemPositions = new Dictionary<Vector3, bool>();

    public void PAUSE(bool state)
    {
        switch (state)
        {
            case (true):
                Time.timeScale = 0; Debug.Log("GAME PAUSED");
                break;
            case (false):
                Time.timeScale = 1; Debug.Log("GAME PLAYING");
                break;
        }
    }
    public void FASTFORWARD()
    {
        switch (Time.timeScale)
        {
            case (0):
            case (10):
                PAUSE(false);
                break;
            default:
                if (Time.timeScale < 10)
                {
                    Time.timeScale++;
                }
                break;
        }

        //defaultCol.a = 1;
        ffButtonText.text = "x" + Time.timeScale.ToString();
    }

    public void AddDog(Dog newDog)
    {
        allDogs.Add(newDog);
        UIOutput.ActivateNewDogPanel();
        PAUSE(true);
    }

    public void AddToItemPools(ItemType itemGroup, Item item)
    {
        if (!itemPools.ContainsKey(itemGroup))
        {
            itemPools.Add(itemGroup, new List<Item>());
        }
        itemPools[itemGroup].Add(item);
    }

    public bool GetClosestActiveItemFor(ItemType requiredType, Dog attemptingDog)
    {
        if (itemPools[requiredType].Count > 0)
        {
            foreach (Item item in itemPools[requiredType])
            {
                if (item.TryGetClosestAvailableInstance(attemptingDog)) { return true; }
            }
        } 
        Debug.LogWarning("No " + requiredType.ToString() + " type items are available for the " + attemptingDog.m_breed + " called " + attemptingDog.m_name.ToString() + "." );
        return false;
    }

    public void EndItemUse(Item item, GameObject instance)
    {
        if (item.IsSingleUse())
        {
            Vector3 instanceSpawnPos = item.GetInstanceSpawnPos(instance);
            if (tempItemPositions.ContainsKey(instanceSpawnPos))
            {
                tempItemPositions[instanceSpawnPos] = false;
            }
        }
        item.StopUsingItemInstance(instance);
    }

    public Dog GetNewestDog()
    {
        if (allDogs.Count > 0) { return allDogs[allDogs.Count - 1]; }
        else return null;  
    }
}

public class CareProperty
{
    private Dictionary<string, Vector2> m_states = new Dictionary<string, Vector2>();
    private List<string> m_currentStates = new List<string>();
    private float m_value = 100;
    private float m_decrement;

    public CareProperty(Dictionary<string, Vector2> states, float defaultDec)
    {
        m_states = states;
        m_decrement = defaultDec;
        UpdateValue(0.0f);
    }

    public float GetValue() { return m_value; }
    public float GetUsualDecrement() { return m_decrement; }
    public bool IsState(string state)
    {
        if (m_states.ContainsKey(state)) { return m_currentStates.Contains(state); }
        else { Debug.Log("No care property has a state defined as " + state); return false; }
    }

    public void UpdateValue(float increment)
    {
        m_value = Mathf.Clamp(m_value + (increment * Time.timeScale), 0.0f, 100.0f);

        m_currentStates.Clear();
        foreach (KeyValuePair<string, Vector2> state in m_states)
        {
            if ((m_value > state.Value.x) && (m_value < state.Value.y))
            {
                m_currentStates.Add(state.Key);           
            }
        }
    }
}

public class PersonalityProperty
{
    private Dictionary<string, Vector2> m_states = new Dictionary<string, Vector2>();
    private string m_currentState;
    private float m_value;

    public PersonalityProperty(Dictionary<string, Vector2> states, float value)
    {
        m_states = states;
        UpdateValue(value);
    }

    public float GetValue() { return m_value; }
    public string GetState() { return m_currentState; }
    public bool IsState(string state)
    {
        if (m_states.ContainsKey(state)) { return (m_currentState == state); }
        else { Debug.Log("No personality property has a state defined as " + state); return false; }
    }

    public void UpdateValue(float increment)
    {
        m_value = Mathf.Clamp(m_value + (increment * Time.timeScale), 0.0f, 100.0f);

        foreach (KeyValuePair<string, Vector2> state in m_states)
        {
            if ((m_value > state.Value.x) && (m_value < state.Value.y))
            {
                m_currentState = state.Key;
                return;
            }
        }
    }
}