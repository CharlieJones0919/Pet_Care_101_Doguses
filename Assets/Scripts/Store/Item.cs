﻿/** \file Item.cs
*   \brief Contains the class definition for a game "Item" which contains a list of "ItemInstances." The concept of the game's items have been divided into these 2 classes to reduce overhead by requiring many GameObjects all the data of the Item class. 
*   Instead, there is only 1 actual instance of each item, with multple GameObjects represented by these Items which are almost purely just model prefabs of which their use can be defined from the Item they were generated by and their state of activation and use.
*/
using System.Collections.Generic;
using UnityEngine;

/** \class Item
*   \brief A class with almost all of its pertinant values serialized so prefabs of instances can easily/dynamically be made by changing these values in Unity's inspector GUI.
*  
*   It inherits from ISerializationCallbackReceiver so a message can be output in the debug log if an item has been set an inequal number of DogCareValue/DogPersonality value fufillments to float values for each.
*   The care/personality values the item restores or modifies when in yse by a dog are specified in the inspector as a list, then an equal sized list of floats is used to specify the amount each of these values are modified by its use.
*   Upon finishing entering this data in the inspector, the ISerializationCallbackReceiver adds the lists Care/Personality fufillments and float amounts to a Dictonary which pairs these values. 
*   This has been done because Dictionaries aren't serializable in Unity, so the data for the Dictionaries are set to serializable lists first then added to a Dictionary.
*/
public class Item : MonoBehaviour, ISerializationCallbackReceiver
{
    public bool doNotDeploy = false;                          //!< Whether the item should be displayed in the store. (Added last minute to de-include items I ran out of time to implement).
    [SerializeField] private int defaultNumToInstantiate = 0; //!< How many object prefabs for this model should be instantiated by default. (This isn't a cap on the number that can be activated/purchased, but if it's known that there will be many instances it could save on overhead to create them ahead of time instead of during play-time).

    [SerializeField] private List<DogCareValue> careFufills = new List<DogCareValue>();                      //!< Serializable list of the DogCareValues this item modifies when used by a dog. Added to the m_careFufillments Dictionary as a key when entry is submitted in the inspector.
    [SerializeField] private List<float> careFufillmentAmounts = new List<float>();                          //!< Serializable float amount the modified DogCareValues are modified by when this item is used. Added to the m_careFufillments Dictionary as a value when entry is submitted in the inspector.
    [SerializeField] private List<DogPersonalityValue> personalityFufills = new List<DogPersonalityValue>(); //!< Serializable list of the DogPersonalityValues this item modifies when used by a dog. Added to the m_personalityFufillments Dictionary as a key when entry is submitted in the inspector.
    [SerializeField] private List<float> personalityFufillmentAmounts = new List<float>();                   //!< Serializable float amount the modified DogPersonalityValues are modified by when this item is used. Added to the m_personalityFufillments Dictionary as a value when entry is submitted in the inspector.

    private Dictionary<DogCareValue, float> m_careFufillments = new Dictionary<DogCareValue, float>();                      //!< A Dictionary list of the care values in a dog modified when using this item, paired with the amount the care value is modified by.
    private Dictionary<DogPersonalityValue, float> m_personalityFufillments = new Dictionary<DogPersonalityValue, float>(); //!< A Dictionary list of the personality values in a dog modified when using this item, paired with the amount the personality value is modified by.

    private GameObject m_defaultNullObject;                                 //!< An empty game object for when game objects shouldn't be set to any actual reference anymore, but unity throws errors if game objects are set to null during runtime.
    private GameObject m_instanceParent;                                    //!< The empty GameObject the Item prefab instances should be parented to. Just specified for organisation and ease of finding instances, and so held items (which are made a child of a dog's snout object) can be placed back from where they came from. 
    [SerializeField] private ItemInstance m_instancePrefab = null;          //!< The GameObject representation of this Item. An instance of this will be activated to the game world when the Item is purchased from the store.
    [SerializeField] private Vector3 m_instancesInactivePos = Vector3.zero; //!< A position to place the GameObject instances of this Item when they're deactivated and out of use. (Under the floor).

    private List<ItemInstance> m_instancePool = new List<ItemInstance>();   //!< A list of the m_instancePrefab objects instantiated to be activated representations of this item. The object includes a simple ItemInstance class which contains additional data like if that instance is currently being used or is active.

    [SerializeField] private ItemType m_itemType = 0;            //!< The ItemType of this Item. (E.g. FOOD, BED, TOYS, etc...).
    [SerializeField] private string m_name = null;               //!< Name of this Item to be displayed in the store. (E.g. Dry Kibble, Plush Duck, etc...).
    [SerializeField] private Sprite m_sprite = null;             //!< The image to be displayed in the store for this Item.
    [SerializeField] private float m_price = 0;                  //!< Price of this Item for the store.
    [SerializeField] private string m_description = null;        //!< Description of this Item for the store.

    [SerializeField] private bool m_singleUse = false;           //!< Whether or not this item can be used multiple times or just once. (E.g. Food items are single use and will be deactivated after use, but beds will remain after use).
    [SerializeField] private Vector3 m_relUsePos = Vector2.zero; //!< Where the dog should be positioned on the object instance when using it. (E.g. Placed in the centre of a bed). 
    [SerializeField] private bool m_needsUseOffset = false;      //!< Whether or not to actually set the dog's position to the specified m_relUsePos. Some Items don't require the dog to be positioned anywhere in partiular around the item instance to use them.

    /** \fn InstantiatePool
    *   \brief Called in StoreController for each Item to generate its ItemInstance objects. 
    */
    public void InstantiatePool(int numInitialInstances = 0)
    {
        int numToCreate;
        if (numInitialInstances == 0) { numToCreate = defaultNumToInstantiate; }
        else { numToCreate = numInitialInstances; }

        for (int i = 0; i < numToCreate; i++)
        {
            ItemInstance newInstance = Instantiate(m_instancePrefab, m_instancesInactivePos, m_instancePrefab.transform.rotation);
            newInstance.Initialise(m_instanceParent, m_instancesInactivePos, m_defaultNullObject);
            m_instancePool.Add(newInstance);
            newInstance.gameObject.SetActive(false);
        }
    }

    /** \fn SetInstanceParent
    *   \brief Called by StoreController to specify where this Item's object instances should be send to in the object hierarchy. (Currently specified as the ground object).
    *   Has to be set by StoreController when the Items are initialised as prefabs can't get objects from the hierarchy so can't just be a serializable field.
    */
    public void SetInstanceParent(GameObject parent) { m_instanceParent = parent; }
    /** \fn SetNULLObjectRef
     *   \brief Called by StoreController to set the null object. This object is the same in all script references, it's just an empty GameObject.
     *   Has to be set by StoreController when the Items are initialised as prefabs can't get objects from the hierarchy so can't just be a serializable field.
     */
    public void SetNULLObjectRef(GameObject nullObj) { m_defaultNullObject = nullObj; }

    /** \fn ActivateAvailableInstanceTo
    *   \brief Called by StoreController when the Item is successfully purchased, and if a pre-instantiated ItemInstance has already been made and hasn't been activated yet, this function will just call it's activation function to activate that instance to the specified world position.
    *   If there isn't already an inactive instance in the pool though, it'll just instatiate a new one and activate that.
    */
    public void ActivateAvailableInstanceTo(Vector3 spawnPos)
    {
        foreach (ItemInstance instance in m_instancePool)
        {
            if (instance != null)
            {
                if (!instance.CurrentlyActive())
                {
                    instance.Activate(spawnPos);
                    return;
                }
            }
        }
        //If none were available for activation in the pool already.
        AddNewToPool().Activate(spawnPos);
    }

    /** \fn AddNewToPool
    *   \brief Called by ActivateAvailableInstanceTo() if there wasn't already an inactive instance to activate. Creates a new instance, adds it to the pool, and returns that newest instance for ActivateAvailableInstanceTo() to activate.
    */
    private ItemInstance AddNewToPool()
    {
        ItemInstance newInstance = Instantiate(m_instancePrefab, m_instancesInactivePos, m_instancePrefab.transform.rotation);
        newInstance.Initialise(m_instanceParent, m_instancesInactivePos, m_defaultNullObject);
        m_instancePool.Add(newInstance);
        return newInstance;
    }

    /** \fn TryGetAvailableInstance
    *   \brief Returns whether or not this Item has any active instances that can be used by the specified dog in its List object pool of instances. If there is, it'll use the direct Dog script reference passed in as a parameter to set the dog's current target to that instance's object.
    */
    public bool TryGetAvailableInstance(Dog attemptingUser)
    {
        if (m_instancePool.Count > 0)
        {
            foreach (ItemInstance instance in m_instancePool)
            {
                if (instance != null)
                {
                    if (instance.UsableFor(attemptingUser.gameObject))
                    {
                        attemptingUser.SetCurrentTargetItem(this, instance.gameObject);
                        return true;
                    }
                }
                else { return false; }
            }
        }
        return false;
    }

    /** \fn TryGetClosestAvailableInstance
    *   \brief The same as TryGetAvailableInstance(), but looks through the instances to identify which instance object is the closest to the specifed dog.
    */
    public bool TryGetClosestAvailableInstance(Dog attemptingUser)
    {
        if (m_instancePool.Count > 0)
        {
            Vector3 userPosition = attemptingUser.gameObject.transform.position;
            ItemInstance closestInstanceSoFar = null;
            float closestDistanceSoFar = 0.0f;

            foreach (ItemInstance instance in m_instancePool)
            {
                if (instance != null)
                {
                    if (instance.UsableFor(attemptingUser.gameObject))
                    {
                        float thisDist = Vector3.Distance(instance.GetPosition(), userPosition);
                        {
                            closestInstanceSoFar = instance;
                            closestDistanceSoFar = thisDist;
                        }
                    }
                }
                else { return false; }
            }

            if (closestInstanceSoFar != null)
            {
                attemptingUser.SetCurrentTargetItem(this, closestInstanceSoFar.gameObject); return true;
            }
        }
        else { Debug.Log("None"); }
        return false;
    }

    /** \fn UseItemInstance
    *   \brief Called by a Dog if it collides with (or reaches) its target Item's object instance to see if the dog can use the instance now. If the instance is still usable for the user, the instance's user will be set to the requesting dog.
    */
    public bool UseItemInstance(GameObject attemptingUser, GameObject requestedInstance)
    {
        if (m_instancePool.Count != 0)
        {
            foreach (ItemInstance instance in m_instancePool)
            {
                if (instance != null)
                {
                    if (instance.IsObject(requestedInstance))
                    {
                        if (instance.UsableFor(attemptingUser))
                        {
                            instance.StartUse(attemptingUser);
                            return true;
                        }
                    }
                }
                else { return false; }
            }
        }
        return false;
    }

    /** \fn StopUsingItemInstance
    *   \brief Called by Controller when called by a dog that wants to stop using its ItemInstance. If the Item is single-use an ending of use will deactivate the object, but otherwise the instance's user will just be set back to null so it can be used again.
    *   Called by Controller first instead of directly by the dog so if the item is temporary its used position can be made free again.
    */
    public void StopUsingItemInstance(GameObject requestedInstance)
    {
        if (m_instancePool.Count != 0)
        {
            foreach (ItemInstance instance in m_instancePool)
            {
                if (instance != null)
                {
                    if (instance.IsObject(requestedInstance))
                    {
                        switch (IsSingleUse())
                        {
                            case (false):
                                instance.EndUse();
                                break;
                            case (true):
                                instance.Deactivate();
                                break;
                        }
                    }
                }
            }
        }
    }

    /** \fn GetInstanceSpawnPos
    *   \brief Returns the requested instance's activation position. Used by Controller to free up the instance's used position again if the instance was single-use and going to be deactivated.
    */
    public Vector3 GetInstanceSpawnPos(GameObject requestedInstance)
    {
        if (m_instancePool.Count != 0)
        {
            foreach (ItemInstance instance in m_instancePool)
            {
                if (instance != null)
                {
                    if (instance.IsObject(requestedInstance))
                    {
                        return instance.GetLastSpawnPos();
                    }
                }
            }
        }
        Debug.Log("Instance not found in the pool: " + requestedInstance.name);
        return Vector3.zero;
    }

    /** \fn GetInstanceParent
    *   \brief Returns the object the Item's instances are children of.
    */
    public GameObject GetInstanceParent(GameObject requestedInstance)
    {
        if (m_instancePool.Count != 0)
        {
            foreach (ItemInstance instance in m_instancePool)
            {
                if (instance.IsObject(requestedInstance))
                {
                    return m_instanceParent;
                }
            }
        }
        Debug.Log("Instance not found in the pool: " + requestedInstance.name);
        return m_defaultNullObject;
    }

    /** \fn GetItemType
    *   \brief Returns the Item's type;
    */
    public ItemType GetItemType() { return m_itemType; }
    /** \fn GetName
    *   \brief Returns the Item's name.
    */
    public string GetName() { return m_name; }
    /** \fn GetSprite
    *   \brief Returns the Item's sprite.
    */
    public Sprite GetSprite() { return m_sprite; }
    /** \fn GetPrice
    *   \brief Returns the Item's price.
    */
    public float GetPrice() { return m_price; }
    /** \fn GetDescription
    *   \brief Returns the Item's description.
    */
    public string GetDescription() { return m_description; }

    /** \fn IsSingleUse
    *   \brief Returns whether or not the Item is single-use.
    */
    public bool IsSingleUse() { return m_singleUse; }
    /** \fn GetUsePosOffset
    *   \brief Returns the Item's position offset for use.
    */
    public Vector3 GetUsePosOffset() { return m_relUsePos; }
    /** \fn GetUsePosOffset
    *   \brief Returns whether the Item's requires a position offset to use.
    */
    public bool NeedsUseOffset() { return m_needsUseOffset; }

    /** \fn FufillsCareValue
    *   \brief Returns whether or not this item modifies the specified care value of a dog when used.
    */
    public bool FufillsCareValue(DogCareValue type)
    {
        if (m_careFufillments.ContainsKey(type)) { return true; }
        return false;
    }
    /** \fn GetCareFufillmentAmount
    *   \brief If the specified care value is modified by using this item, this function returns the amount that value is modified by.
    */
    public float GetCareFufillmentAmount(DogCareValue type)
    {
        if (m_careFufillments.ContainsKey(type)) { return m_careFufillments[type]; }
        Debug.LogWarning("The following item does not fufill the care value of " + type.ToString() + ": " + m_name);
        return 0.0f;
    }

    /** \fn FufillsPersonalityValue
    *   \brief Returns whether or not this item modifies the specified personality value of a dog when used.
    */
    public bool FufillsPersonalityValue(DogPersonalityValue type)
    {
        if (m_personalityFufillments.ContainsKey(type)) { return true; }
        return false;
    }
    /** \fn GetPersonalityFufillmentAmount
    *   \brief If the specified personality value is modified by using this item, this function returns the amount that value is modified by.
    */
    public float GetPersonalityFufillmentAmount(DogPersonalityValue type)
    {
        if (m_personalityFufillments.ContainsKey(type)) { return m_personalityFufillments[type]; }
        Debug.LogWarning("The following item does not fufill the personality value of " + type.ToString() + ": " + m_name);
        return 0.0f;
    }

    /** \fn OnBeforeSerialize
    *   \brief Required for use of ISerializationCallbackReceiver.
    */
    public void OnBeforeSerialize() { }
    /** \fn OnAfterDeserialize
    *   \brief Outputs an warning message if the care/personality modifiyer specifications have an inequal number of fufillment amounts listed once entered. Otherwise, the list of enums and float modification amounts are added to their respective dictionaries.
    */
    public void OnAfterDeserialize()
    {
        if (careFufills.Count != careFufillmentAmounts.Count) { Debug.LogWarning("The following item has been set an inequal number of care fufillments to fufillment amounts: " + m_name); }
        else
        {
            m_careFufillments = new Dictionary<DogCareValue, float>(); 

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
            m_personalityFufillments = new Dictionary<DogPersonalityValue, float>(); 

            for (int i = 0; i < personalityFufills.Count; i++)
            {
                if (!m_personalityFufillments.ContainsKey(personalityFufills[i]))
                { m_personalityFufillments.Add(personalityFufills[i], personalityFufillmentAmounts[i]); }
                else { m_personalityFufillments[personalityFufills[i]] = personalityFufillmentAmounts[i]; }
            }
        }
    }
}