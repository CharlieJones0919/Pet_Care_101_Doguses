using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BodyPart
{
    Neck, Head, Eye0, Eye1,
    Snout,
    Ear0, Ear1,
    Tail,

    Shoulder0, Shoulder1, Shoulder2, Shoulder3,
    UpperLeg0, UpperLeg1, UpperLeg2, UpperLeg3,

    Knee0, Knee1, Knee2, Knee3,

    LowerLeg0, LowerLeg1, LowerLeg2, LowerLeg3,
    Ankle0, Ankle1, Ankle2, Ankle3,
    Foot0, Foot1, Foot2, Foot3,

    Chest, Rear, Waist
}

public struct BodyComponent
{
    private BodyPart m_part;
    public GameObject m_component;
    private GameObject m_parent;
    private List<DogDataField> m_data;

    public BodyComponent(BodyPart type, GameObject component, GameObject parent, DogDataField data)
    {
        m_part = type;
        m_component = component;
        m_parent = parent;
        m_data = new List<DogDataField>();
        m_data.Add(data);
    }

    public BodyComponent(BodyPart type, GameObject component, GameObject parent, DogDataField[] dataList)
    {
        m_part = type;
        m_component = component;
        m_parent = parent;
        m_data = new List<DogDataField>();
        foreach (DogDataField field in dataList) { m_data.Add(field); };
    }

    public BodyComponent(BodyPart type, GameObject component, GameObject parent)
    {
        m_part = type;
        m_component = component;
        m_parent = parent;
        m_data = new List<DogDataField>();
    }

    public void SetData(DogDataField data) { m_data.Add(data); }
    public void SetData(DogDataField[] dataList) { foreach (DogDataField field in dataList) { m_data.Add(field); }; }

    public BodyPart GetPartType() { return m_part; }
    public GameObject GetParent() { return m_parent.gameObject; }
    public List<DogDataField> GetDataList() { return m_data; }

    public bool DefinesDataField(DogDataField field) { return m_data.Contains(field); }
    public bool HasFieldContaining(string str)
    {
        foreach (DogDataField field in m_data)
        {
            if (field.ToString().Contains(str)) { return true; }
        }
        return false;
    }
}

public class DogController : MonoBehaviour
{
    public GameTime localTime;
    [SerializeField] private GameObject bowlPrefab;
    [SerializeField] private GameObject bedPrefab;
    private List<GameObject> objectsForDeletion = new List<GameObject>();

    private List<ItemPool> itemPool = new List<ItemPool>();

    // Start is called before the first frame update
    //public void Start()
    //{
    //        ////////////////////////////// Object Pool Testing //////////////////////////////
    //        float yHeight = bowlPrefab.transform.localScale.y / 2.0f;

    //        List<Vector3> tempBowlPositions = new List<Vector3>();
    //        tempBowlPositions.Add(new Vector3(-20.0f, yHeight, 20.0f));
    //        tempBowlPositions.Add(new Vector3(-5.0f, yHeight, -10.0f));

    //        yHeight = bedPrefab.transform.localScale.y / 2.0f;
    //        List<Vector3> tempBedPositions = new List<Vector3>();
    //        tempBedPositions.Add(new Vector3(20.0f, yHeight, 10.0f));
    //        tempBedPositions.Add(new Vector3(-20.0f, yHeight, 5.0f));

    //        itemPool.Add(new ItemPool(ItemType.BOWL, bowlPrefab, gameObject, tempBowlPositions));
    //        itemPool.Add(new ItemPool(ItemType.BED, bedPrefab, gameObject, tempBedPositions));

    //        foreach (ItemPool pool in itemPool)
    //        {
    //            pool.InstantiateNewToList();
    //            pool.InstantiateNewToList();
    //        }
    //    }

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
}