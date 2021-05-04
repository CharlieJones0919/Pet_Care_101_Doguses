using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] private uint defaultNumToInstantiate = 0;
    [SerializeField] private float useTime = 0;

    [SerializeField] private List<DogCareValue> careFufills = new List<DogCareValue>();
    [SerializeField] private List<float> careFufillmentAmounts = new List<float>();
    [SerializeField] private List<DogPersonalityValue> personalityFufills = new List<DogPersonalityValue>();
    [SerializeField] private List<float> personalityFufillmentAmounts = new List<float>();

    private Dictionary<DogCareValue, float> m_careFufillments = new Dictionary<DogCareValue, float>();
    private Dictionary<DogPersonalityValue, float> m_personalityFufillments = new Dictionary<DogPersonalityValue, float>();

    private GameObject m_defaultNullObject;
    [SerializeField] private GameObject m_objectPrefab;
    private GameObject m_instanceParent;
    [SerializeField] private Vector3 m_instancesInactivePos;

    private List<ItemInstance> m_instancePool = new List<ItemInstance>();
    private List<ItemInstance> m_availablePoolInstances = new List<ItemInstance>();
    private int numberOfPoolInstances = 0;
    private int numberOfAvailableInstances = 0;

    [SerializeField] private ItemType m_itemType;
    [SerializeField] private string m_name = null;
    [SerializeField] private Sprite m_sprite;
    [SerializeField] private double m_price;
    [SerializeField] private string m_description;

    [SerializeField] private bool m_singleUse;
    [SerializeField] private Vector2 m_relUsePos;
    [SerializeField] private bool m_needsUseOffset;

    private class ItemInstance
    {
        private ItemType m_type;
        private GameObject m_instance;
        private Vector3 m_lastSpawnedPos = Vector3.zero;
        private GameObject m_defaultParent;

        private Vector3 m_inactivePos;
        private float m_activePosY;
        private GameObject m_nullUser;
        private GameObject m_user;

        public ItemInstance(ItemType itemType, GameObject objectBase, GameObject parentTransform, Vector3 inactivePos, GameObject nullObj)
        {
            m_type = itemType;
            m_instance = Instantiate(objectBase, inactivePos, objectBase.transform.rotation);
            m_instance.transform.SetParent(parentTransform.transform);
            m_defaultParent = parentTransform;

            m_inactivePos = inactivePos;
            m_activePosY = objectBase.transform.position.y;
            m_nullUser = nullObj;
            m_user = m_nullUser;
        }

        public void Activate(Vector3 activePos)
        {
            m_lastSpawnedPos = activePos;
            m_instance.SetActive(true);
            m_instance.transform.localPosition = activePos;
            m_instance.transform.localPosition += new Vector3(0, m_activePosY, 0);
        }

        public void Deactivate()
        {
            if (CurrentlyActive())
            {
                m_instance.SetActive(false);
                m_user = m_nullUser;
                m_instance.transform.position = m_inactivePos;
            }
        }

        public void EndUse() { if (CurrentlyActive()) { m_user = m_nullUser; } }

        public GameObject GetObject() { return m_instance; }
        public bool IsObject(GameObject thisObject) { return (m_instance == thisObject); }
        public Vector3 GetPosition() { return m_instance.transform.position; }
        public Vector3 GetLastSpawnPos() { return m_lastSpawnedPos; }
        public GameObject GetParent() { return m_defaultParent; }
        public bool CurrentlyActive() { return m_instance.activeSelf; }

        public void SetUser(GameObject user) { m_user = user; }
        public bool UserIs(GameObject thisUser) { return (m_user == thisUser); }
        public GameObject GetUser() { return m_user; }
        public bool UsableFor(GameObject user) { return (((m_user == m_nullUser) || (m_user == user)) && m_instance.activeSelf); }
    }

    public void InstantiatePool(uint numInitialInstances = 0)
    {
        uint numToCreate;
        if (numInitialInstances == 0) { numToCreate = defaultNumToInstantiate; }
        else { numToCreate = numInitialInstances; }

        for (uint i = 0; i < numToCreate; i++)
        {
            m_instancePool.Add(new ItemInstance(m_itemType, m_objectPrefab, m_instanceParent, m_instancesInactivePos, m_defaultNullObject));
        }
        numberOfPoolInstances = m_instancePool.Count;
    }

    public void SetInstanceParent(GameObject parent) { m_instanceParent = parent; }
    public void SetNULLObjectRef(GameObject nullObj) { m_defaultNullObject = nullObj; }

    private ItemInstance AddNewToPool()
    {
        ItemInstance newInstance = new ItemInstance(m_itemType, m_objectPrefab, m_instanceParent, m_instancesInactivePos, m_defaultNullObject);
        m_instancePool.Add(newInstance);
        numberOfPoolInstances = m_instancePool.Count;
        return m_instancePool[m_instancePool.Count - 1];
    }

    public void ActivateAvailableInstanceTo(Vector3 spawnPos)
    {
        foreach (ItemInstance instance in m_instancePool)
        {
            if (!instance.CurrentlyActive())
            {
                instance.Activate(spawnPos);
                m_availablePoolInstances.Add(instance); numberOfAvailableInstances++;
                return;
            }
        }

        AddNewToPool().Activate(spawnPos);
        m_availablePoolInstances.Add(m_instancePool[m_instancePool.Count - 1]); numberOfAvailableInstances++;
    }

    public void DeactivateInstance(GameObject instanceToDeactivate)
    {
        foreach (ItemInstance instance in m_instancePool)
        {
            if (instance.IsObject(instanceToDeactivate) && instance.CurrentlyActive())
            {
                instance.Deactivate();

                m_availablePoolInstances.Remove(instance); numberOfAvailableInstances--;
                return;
            }
        }
        Debug.LogWarning("This object could not be found in this item's instance pool to deactivate: " + instanceToDeactivate);
    }

    public bool TryGetAvailableInstance(Dog attemptingUser)
    {
        if (numberOfAvailableInstances == 0) { return false; }

        foreach (ItemInstance instance in m_availablePoolInstances)
        {
            if (instance.UsableFor(attemptingUser.gameObject))
            {
                attemptingUser.SetCurrentTargetItem(this, instance.GetObject());
                return true;
            }
        }
        return false;
    }

    public bool TryGetClosestAvailableInstance(Dog attemptingUser)
    {
        if (numberOfAvailableInstances != 0)
        {
            Vector3 userPosition = attemptingUser.gameObject.transform.position;
            ItemInstance closestInstanceSoFar = null;
            float closestDistanceSoFar = 0.0f;

            foreach (ItemInstance instance in m_availablePoolInstances)
            {
                if (instance.UsableFor(attemptingUser.gameObject) || instance.UserIs(attemptingUser.gameObject))
                {
                    float thisDist = Vector3.Distance(instance.GetPosition(), userPosition);
                    if ((thisDist < closestDistanceSoFar) || (instance == m_availablePoolInstances[0]))
                    {
                        closestInstanceSoFar = instance;
                        closestDistanceSoFar = thisDist;
                    }
                }
            }

            if (closestInstanceSoFar != null)
            {
                attemptingUser.SetCurrentTargetItem(this, closestInstanceSoFar.GetObject());  return true;
            }
        }
        return false;
    }

    public bool UseItemInstance(GameObject attemptingUser, GameObject requestedInstance)
    {
        foreach (ItemInstance instance in m_availablePoolInstances)
        {
            if (instance.IsObject(requestedInstance))
            {
                if (instance.UsableFor(attemptingUser))
                {
                    instance.SetUser(attemptingUser);
                    m_availablePoolInstances.Remove(instance); numberOfAvailableInstances--;
                    return true;
                }
            }
        }
        Debug.LogWarning("This object could not be found in this item's instance pool.");
        return false;
    }

    public bool IsUsable(GameObject attemptingUser, GameObject requestedInstance)
    {
        foreach (ItemInstance instance in m_availablePoolInstances)
        {
            if (instance.UsableFor(attemptingUser) && instance.IsObject(requestedInstance))
            {
                return true;
            }
        }
        return false;
    }

    public Vector3 GetInstanceSpawnPos(GameObject requestedInstance)
    {
        foreach (ItemInstance instance in m_instancePool)
        {
            if (instance.IsObject(requestedInstance))
            {
                return instance.GetLastSpawnPos();
            }
        }
        Debug.Log("Instance not found in the pool: " + requestedInstance.name);
        return Vector3.zero;
    }

    public GameObject GetInstanceParent(GameObject requestedInstance)
    {
        foreach (ItemInstance instance in m_instancePool)
        {
            if (instance.IsObject(requestedInstance))
            {
                return instance.GetParent();
            }
        }
        Debug.Log("Instance not found in the pool: " + requestedInstance.name);
        return m_defaultNullObject;
    }

    public void StopUsingItemInstance(GameObject requestedInstance)
    {
        foreach (ItemInstance instance in m_instancePool)
        {
            if (instance.IsObject(requestedInstance))
            {
                switch (IsSingleUse())
                {
                    case (false):
                        instance.EndUse();
                        m_availablePoolInstances.Add(instance); numberOfAvailableInstances++;
                        break;
                    case (true):
                        instance.Deactivate();
                        break;
                }
            }
        }
    }

    public ItemType GetItemType() { return m_itemType; }
    public string GetName() { return m_name; }
    public Sprite GetSprite() { return m_sprite; }
    public void SetSprite(Sprite sprite) { m_sprite = sprite; }
    public double GetPrice() { return m_price; }
    public string GetDescription() { return m_description; }

    public float GetUseTime() { return useTime; }
    public bool IsSingleUse() { return m_singleUse; }
    public Vector2 GetUsePosOffset() { return m_relUsePos; }
    public bool NeedsUseOffset() { return m_needsUseOffset; }

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