using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DogController : MonoBehaviour
{
    public GameTime localTime;
    private List<GameObject> objectsForDeletion = new List<GameObject>();

    private Dictionary<ItemType, List<Item>> itemPools = new Dictionary<ItemType, List<Item>>();

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
        foreach (Item item in itemPools[requiredType])
        {
            if (item.TryGetClosestAvailableInstance(attemptingDog)) { return true; }
        }
        //Debug.LogWarning("No " + requiredType.ToString() + " type items are available for the " + attemptingDog.m_breed + " called " + attemptingDog.m_name.ToString() + "." );
        return false;
    }
}

public class CareProperty
{
    private Dictionary<string, Vector2> m_states = new Dictionary<string, Vector2>();
    private string m_currentState;
    private float m_value = 50;
    private float m_decrement;

    public CareProperty(Dictionary<string, Vector2> states, float defaultDec)
    {
        m_states = states;
        m_decrement = defaultDec;
        UpdateValue(0.0f);
    }

    public float GetValue() { return m_value; }
    public float GetUsualDecrement() { return m_decrement; }
    public string GetState() { return m_currentState; }
    public bool IsState(string state) { return (m_currentState == state); }

    public void UpdateValue(float increment)
    {
        m_value = Mathf.Clamp(m_value + increment, 0.0f, 100.0f);

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
    public bool IsState(string state) { return (m_currentState == state); }

    public void UpdateValue(float increment)
    {
        m_value = Mathf.Clamp(m_value + increment, 0.0f, 100.0f);

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