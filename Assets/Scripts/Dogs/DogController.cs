using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { BOWL, BED };

public class DogController : MonoBehaviour
{
    [SerializeField] private GameObject bowlPrefab;
    [SerializeField] private GameObject bedPrefab;
    private List<GameObject> objectsForDeletion = new List<GameObject>();

    private class ItemPool
    {
        public ItemType type;
        private GameObject prefabRef;
        private GameObject destinationRef;
        private List<Vector3> allowedPositions;
        public List<Item> itemList;

        public ItemPool(ItemType iType, GameObject prefab, GameObject parentObj, List<Vector3> spawnPositions)
        {
            type = iType;
            prefabRef = prefab;
            destinationRef = new GameObject(type.ToString() + " OBJECTS");
            destinationRef.transform.parent = parentObj.transform;
            allowedPositions = spawnPositions;
            itemList = new List<Item>();
        }

        public void InstantiateNewToList()
        {
            DogCareValue propertySubject = DogCareValue.NONE;
            Vector3 foundFreePos = Vector3.zero;
            bool singleUse = false;
            bool centrePref = false;

            foreach (Vector3 position in allowedPositions)
            {
                foundFreePos = position;

                switch (type)
                {
                    case (ItemType.BOWL):
                        propertySubject = DogCareValue.Hunger;
                        singleUse = true;
                        centrePref = false;
                        break;
                    case (ItemType.BED):
                        propertySubject = DogCareValue.Rest;
                        centrePref = true;
                        break; 
                }

                itemList.Add(new Item(prefabRef, destinationRef, foundFreePos, "GENERIC " + type.ToString(), 1.00f, "Desc.", 0.05f, 0.015f, propertySubject, singleUse, centrePref));
                break;
            }

            if ((propertySubject != null))
            {
                allowedPositions.Remove(foundFreePos);
                return;
            }
            Debug.Log("Maximum Number of " + type.ToString() + "'s Already Placed");
        }
    };

    List<ItemPool> itemPool = new List<ItemPool>();

    // Start is called before the first frame update
    void Start()
    {
        ////////////////////////////// Object Pool Testing //////////////////////////////
        float yHeight = bowlPrefab.transform.localScale.y / 2.0f;

        List<Vector3> tempBowlPositions = new List<Vector3>();
        tempBowlPositions.Add(new Vector3(-20.0f, yHeight, 20.0f));
        tempBowlPositions.Add(new Vector3(-5.0f, yHeight, -10.0f));

        yHeight = bedPrefab.transform.localScale.y / 2.0f;
        List<Vector3> tempBedPositions = new List<Vector3>();
        tempBedPositions.Add(new Vector3(20.0f, yHeight, 10.0f));
        tempBedPositions.Add(new Vector3(-20.0f, yHeight, 5.0f));

        itemPool.Add(new ItemPool(ItemType.BOWL, bowlPrefab, gameObject, tempBowlPositions));
        itemPool.Add(new ItemPool(ItemType.BED, bedPrefab, gameObject, tempBedPositions));

        foreach (ItemPool pool in itemPool)
        {
            pool.InstantiateNewToList();
            pool.InstantiateNewToList();
        }
    }

    public List<Item> GetActiveObjects(ItemType type)
    {
        foreach (ItemPool pool in itemPool)
        {
            if (pool.type == type)
            {
                return pool.itemList;
            }
        }
        Debug.Log("No Objects of that Type Available");
        return null;
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
};