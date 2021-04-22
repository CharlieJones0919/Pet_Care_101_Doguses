using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInstance : MonoBehaviour
{
    private ItemType m_type;
    private GameObject m_instance;
    private bool m_singleUse;
    private Vector3 m_inactivePos;
    private GameObject m_user;

    public ItemInstance(ItemType itemType, GameObject objectBase, bool isSingleUse, GameObject parentTransform, Vector3 inactivePos)
    {
        m_type = itemType;
        m_instance = Instantiate(objectBase, inactivePos, Quaternion.identity);
        m_instance.transform.SetParent(parentTransform.transform);

        m_singleUse = isSingleUse;
        m_inactivePos = inactivePos;
        m_user = null;
    }

    public void Activate(Vector3 activePos)
    {
        m_instance.SetActive(true);
        m_instance.transform.position = activePos;
    }

    public void Deactivate()
    {
        m_instance.SetActive(false);
        m_instance.transform.position = m_inactivePos;
    }

    public GameObject GetObject() { return m_instance; }
    public Vector3 GetPosition() { return m_instance.transform.position; }
    public void StartUse(GameObject user) { m_user = user; }
    public void EndUse() { m_user = null; if (m_singleUse) { Deactivate(); } }
    public GameObject GetUser() { return m_user; }
    public bool Usable() { return ((m_user == null) && m_instance.activeSelf); }
}

public class Item : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] private List<DogCareValue> careFufills = new List<DogCareValue>();
    [SerializeField] private List<float> careFufillmentAmounts = new List<float>();
    [SerializeField] private List<DogPersonalityValue> personalityFufills = new List<DogPersonalityValue>();
    [SerializeField] private List<float> personalityFufillmentAmounts = new List<float>();

    [SerializeField] private ItemType m_objectType;
    [SerializeField] private GameObject m_baseObject;
    [SerializeField] private GameObject m_instanceParent;
    [SerializeField] private Vector3 m_instancesInactivePos;
    private List<ItemInstance> m_instancePool = new List<ItemInstance>();

    [SerializeField] private StoreCatergory m_storeCatergory;
    [SerializeField] private Vector3 m_activeSpawnPos;

    [SerializeField] private string m_name = null;
    [SerializeField] private Sprite m_sprite;
    [SerializeField] private double m_price;
    [SerializeField] private string m_description;

    private Dictionary<DogCareValue, float> m_careFufillments = new Dictionary<DogCareValue, float>();
    private Dictionary<DogPersonalityValue, float> m_personalityFufillments = new Dictionary<DogPersonalityValue, float>();

    [SerializeField] private bool m_singleUse;
    [SerializeField] private Vector3 m_relUsePos;
    [SerializeField] private Vector3 m_relUseRot;


    public void InstantiatePool(uint numInitialInstances)
    {
        for (uint i = 0; i < numInitialInstances; i++)
        {
            m_instancePool.Add(new ItemInstance(m_objectType, m_baseObject, m_singleUse, m_instanceParent, m_instancesInactivePos));
        }
    }

    public List<ItemInstance> GetInstances()
    {
        return m_instancePool;
    }

    public List<ItemInstance> GetAvailableInstances()
    {
        List<ItemInstance> availableInstances = new List<ItemInstance>();

        foreach (ItemInstance instance in m_instancePool)
        {
            if (instance.Usable()) { availableInstances.Add(instance); }
        }

        if (availableInstances.Count > 0) { return availableInstances; }
        else
        {
            InstantiatePool(1);
            return GetAvailableInstances(); // Incorrect if called by dog not store.
        }
    }

    public ItemInstance GetClosestActiveInstanceTo(Vector3 point)
    {
        List<ItemInstance> availableInstances = GetAvailableInstances();
        ItemInstance closestInstanceSoFar = null;
        float closestDistanceSoFar = 0.0f;

        foreach (ItemInstance instance in availableInstances)
        {
            float thisDist = Vector3.Distance(instance.GetPosition(), point);

            if (instance == availableInstances[0])
            {
                closestInstanceSoFar = instance;
                closestDistanceSoFar = thisDist;
            }
            else if (thisDist < closestDistanceSoFar)
            {
                closestInstanceSoFar = instance;
                closestDistanceSoFar = thisDist;
            }
        }

        return closestInstanceSoFar;
    }

    public ItemType GetObjectType() { return m_objectType; }
    public bool IsTangible() { return (m_baseObject != null); }
    public StoreCatergory GetCatergory() { return m_storeCatergory; }

    public string GetName() { return m_name; }
    public Sprite GetSprite() { return m_sprite; }
    public void SetSprite(Sprite sprite) { m_sprite = sprite; }
    public double GetPrice() { return m_price; }
    public string GetDescription() { return m_description; }

    public bool IsSingleUse() { return m_singleUse; }
    public Vector3 GetUsePosition() { return m_relUsePos; }
    public Vector3 GetUseRotation() { return m_relUseRot; }

    public List<DogCareValue> GetCareFufillmentList()
    {
        List<DogCareValue> values = new List<DogCareValue>();
        foreach (KeyValuePair<DogCareValue, float> value in m_careFufillments) { values.Add(value.Key); }
        if (values.Count == 0) { Debug.LogWarning("The following item does not fufill any care values: " + m_name); }
        return values;
    }
    public bool FufillsCareValue(DogCareValue type)
    {
        if (m_careFufillments.ContainsKey(type)) { return true; }
        return false;
    }
    public float GetCareFufillmentAmount(DogCareValue type)
    {
        if (m_careFufillments.ContainsKey(type)) { return m_careFufillments[type]; }
        Debug.LogWarning("The following item does not fufill the care value of " + type.ToString() + ": " + m_name);
        return 0.0f;
    }

    public List<DogPersonalityValue> GetPersonalityFufillmentList()
    {
        List<DogPersonalityValue> values = new List<DogPersonalityValue>();
        foreach (KeyValuePair<DogPersonalityValue, float> value in m_personalityFufillments) { values.Add(value.Key); }
        if (values.Count == 0) { Debug.LogWarning("The following item does not fufill any personality values: " + m_name); }
        return values;
    }
    public bool FufillsPersonalityValue(DogPersonalityValue type)
    {
        if (m_personalityFufillments.ContainsKey(type)) { return true; }
        return false;
    }
    public float GetPersonalityFufillmentAmount(DogPersonalityValue type)
    {
        if (m_personalityFufillments.ContainsKey(type)) { return m_personalityFufillments[type]; }
        Debug.LogWarning("The following item does not fufill the personality value of " + type.ToString() + ": " + m_name);
        return 0.0f;
    }

    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize()
    {
        if (careFufills.Count != careFufillmentAmounts.Count) { Debug.LogWarning("The following item has been set an inequal number of care fufillments to fufillment amounts: " + m_name); }
        else
        {
            if (m_careFufillments == null) { m_careFufillments = new Dictionary<DogCareValue, float>(); }

            for (int i = 0; i < careFufills.Count; i++)
            {
                if (!m_careFufillments.ContainsKey(careFufills[i]))
                { m_careFufillments.Add(careFufills[i], careFufillmentAmounts[i]); }
                else { m_careFufillments[careFufills[i]] = careFufillmentAmounts[i]; }
            }
        }

        if (personalityFufills.Count != personalityFufillmentAmounts.Count) { Debug.LogWarning("The following item has been set an inequal number of personality fufillments to fufillment amounts: " + m_name); }
        else
        {
            if (m_personalityFufillments == null) { m_personalityFufillments = new Dictionary<DogPersonalityValue, float>(); }

            for (int i = 0; i < personalityFufills.Count; i++)
            {
                if (!m_personalityFufillments.ContainsKey(personalityFufills[i]))
                { m_personalityFufillments.Add(personalityFufills[i], personalityFufillmentAmounts[i]); }
                else { m_personalityFufillments[personalityFufills[i]] = personalityFufillmentAmounts[i]; }
            }
        }
    }
}