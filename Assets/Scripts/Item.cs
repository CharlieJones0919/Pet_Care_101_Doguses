using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] private List<DogCareValue> fufills = new List<DogCareValue>();
    [SerializeField] private List<float> fufillmentAmounts = new List<float>();

    [SerializeField] private GameObject m_object;
    [SerializeField] private StoreCatergory m_storeCatergory;
    [SerializeField] private Vector3 m_activeSpawnPos;

    [SerializeField] private string m_name = null;
    [SerializeField] private Sprite m_sprite;
    [SerializeField] private float m_price;
    [SerializeField] private string m_description;

    private Dictionary<DogCareValue, float> m_fufillments = new Dictionary<DogCareValue, float>();

    [SerializeField] private GameObject m_user;
    [SerializeField] private bool m_usable;
    [SerializeField] private bool m_singleUse;

    [SerializeField] private Vector3 m_relUsePos;
    [SerializeField] private Vector3 m_relUseRot;

    public Item(StoreCatergory catergory, string name, Sprite sprite, float price, string description, GameObject objectRef, GameObject objectParent, Vector3 spawnPos, Dictionary<DogCareValue, float> fufillments, Vector3 usePos, Vector3 useRot, bool isSingleUse)
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

        foreach (KeyValuePair<DogCareValue, float> val in fufillments) { m_fufillments.Add(val.Key, val.Value); }

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
    public float GetPrice() { return m_price; }
    public string GetDescription() { return m_description; }

    public Vector3 GetPosition() { return m_object.transform.position; }
    public Vector3 GetUsePosition() { return m_relUsePos; }
    public Vector3 GetUseRotation() { return m_relUseRot; }

    public List<DogCareValue> GetFufillmentList()
    {
        List<DogCareValue> values = new List<DogCareValue>();
        foreach (KeyValuePair<DogCareValue, float> value in m_fufillments) { values.Add(value.Key); }
        if (values.Count == 0) { Debug.LogWarning("The following item does not fufill any care values: " + m_name); }
        return values;
    }
    public bool FufillsCareValue(DogCareValue type)
    {
        if (m_fufillments.ContainsKey(type)) { return true; }
        return false;
    }
    public float GetFufillmentAmount(DogCareValue type)
    {
        if (m_fufillments.ContainsKey(type)) { return m_fufillments[type]; }
        Debug.LogWarning("The following item does not fufill the care value of " + type.ToString() + ": " + m_name);
        return 0.0f;
    }

    public void StartUse(GameObject user) { m_user = user; }
    public void EndUse() { m_user = null; if (m_singleUse) m_usable = false; }
    public GameObject GetUser() { return m_user; }
    public bool GetUsable() { return m_usable; }

    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize()
    {
        //if (fufills.Count != fufillmentAmounts.Count) { Debug.LogWarning("The following item has been set an inequal number of fufillments to fufillment amounts: " + m_name); }
        //else
        //{
        //    for (int i = 0; i < fufills.Count; i++)
        //    {
        //        if (!m_fufillments.ContainsKey(fufills[i])) { m_fufillments.Add(fufills[i], fufillmentAmounts[i]); }
        //        else { m_fufillments[fufills[i]] = fufillmentAmounts[i]; }
        //    }
        //}
    }
}