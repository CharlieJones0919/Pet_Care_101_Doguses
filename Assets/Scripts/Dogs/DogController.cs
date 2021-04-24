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
        Debug.LogWarning("No " + requiredType.ToString() + " type items are available for the " + attemptingDog.m_breed + " called " + attemptingDog.m_name + "." );
        return false;
    }
}

public class CareProperty
{
    private DogCareValue m_name;
    private float m_value = 50;
    private float m_increment = 0;
    private Dictionary<string, Vector2> m_states = new Dictionary<string, Vector2>();

    public CareProperty(DogCareValue prop, Dictionary<string, Vector2> states, float defaultInc)
    {
        m_name = prop;
        m_states = states;
        m_increment = defaultInc;
    }

    public DogCareValue GetPropertyName() { return m_name; }
    public float GetValue() { return m_value; }
    public float GetCurrenntIncrement() { return m_increment; }
    public Vector2 GetStateRange(string state)
    {
        if (m_states.ContainsKey(state)) { return m_states[state]; }
        else { Debug.Log("The state " + "'" + state + "'" + " is not defined in " + m_name.ToString() + "."); return Vector2.zero; }
    }

    public void UpdateValue(float increment) { m_value = Mathf.Clamp(m_value + increment, 0.0f, 100.0f); }
}

public class PersonalityProperty
{
    private DogPersonalityValue m_name;
    private float m_value = 2.5f;
    private Dictionary<string, Vector2> m_states = new Dictionary<string, Vector2>();

    public PersonalityProperty(DogPersonalityValue prop, Dictionary<string, Vector2> states, float value)
    {
        m_name = prop;
        m_states = states;
        m_value = value;
    }

    public DogPersonalityValue GetPropertyName() { return m_name; }
    public float GetValue() { return m_value; }
    public Vector2 GetStateRange(string state)
    {
        if (m_states.ContainsKey(state)) { return m_states[state]; }
        else { Debug.Log("The state " + "'" + state + "'" + " is not defined in " + m_name.ToString() + ".");  return Vector2.zero;  }
    }

    public void SetValue(float newVal) { m_value = Mathf.Clamp(newVal, 0.0f, 5.0f); }
}