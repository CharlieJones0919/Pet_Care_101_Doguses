using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] private List<DogCareValue> careFufills = new List<DogCareValue>();
    [SerializeField] private List<float> careFufillmentAmounts = new List<float>();
    [SerializeField] private List<DogPersonalityValue> personalityFufills = new List<DogPersonalityValue>();
    [SerializeField] private List<float> personalityFufillmentAmounts = new List<float>();

    [SerializeField] private GameObject m_object;
    [SerializeField] private StoreCatergory m_storeCatergory;
    [SerializeField] private Vector3 m_activeSpawnPos;

    [SerializeField] private string m_name = null;
    [SerializeField] private Sprite m_sprite;
    [SerializeField] private double m_price;
    [SerializeField] private string m_description;

    private Dictionary<DogCareValue, float> m_careFufillments = new Dictionary<DogCareValue, float>();
    private Dictionary<DogPersonalityValue, float> m_personalityFufillments = new Dictionary<DogPersonalityValue, float>();

    [SerializeField] private GameObject m_user;
    [SerializeField] private bool m_usable;
    [SerializeField] private bool m_singleUse;

    [SerializeField] private Vector3 m_relUsePos;
    [SerializeField] private Vector3 m_relUseRot;

    public Item(StoreCatergory catergory, string name, Sprite sprite, double price, string description, GameObject objectRef, GameObject objectParent, Vector3 spawnPos, Dictionary<DogCareValue, float> cFufillments, Dictionary<DogPersonalityValue, float> pFufillments, Vector3 usePos, Vector3 useRot, bool isSingleUse)
    {
        m_storeCatergory = catergory;

        m_name = name;
        m_sprite = sprite;
        m_price = price;
        m_description = description;

        m_object = Instantiate(objectRef, new Vector3(0, -50, 0), Quaternion.identity);
        m_object.SetActive(false);
        m_object.transform.parent = objectParent.transform;
        m_object.transform.name = "[ " + name + " ]";
        m_activeSpawnPos = spawnPos;

        foreach (KeyValuePair<DogCareValue, float> val in cFufillments) { m_careFufillments.Add(val.Key, val.Value); }
        foreach (KeyValuePair<DogPersonalityValue, float> val in pFufillments) { m_personalityFufillments.Add(val.Key, val.Value); }

        m_relUsePos = usePos;
        m_relUseRot = useRot;

        m_user = null;
        m_usable = false;
        m_singleUse = isSingleUse;
    }

    public StoreCatergory GetCatergory() { return m_storeCatergory; }
    public GameObject GetObject() { return m_object; }

    public string GetName() { return m_name; }
    public Sprite GetSprite() { return m_sprite; }
    public void SetSprite(Sprite sprite) { m_sprite = sprite; }
    public double GetPrice() { return m_price; }
    public string GetDescription() { return m_description; }

    public Vector3 GetPosition() { return m_object.transform.position; }
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

    public void StartUse(GameObject user) { m_user = user; }
    public void EndUse() { m_user = null; if (m_singleUse) m_usable = false; }
    public GameObject GetUser() { return m_user; }
    public bool GetUsable() { return m_usable; }

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