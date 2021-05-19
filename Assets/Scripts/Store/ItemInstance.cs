/** \file ItemInstance.cs
*   \brief Contains the class definition for an an "ItemInstance" - an active object instance of the item derived from and instantiated by and Item class. 
*/
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////// ITEM INSTANCE CLASS ///////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/** \class ItemInstance
*   \brief This class encapsulates an object instance of an Item. Combines the object with some other data about that particular instance like if it's being used, where it was activated to in the map.
*/
public class ItemInstance : MonoBehaviour
{
    [SerializeField] private Vector3 m_lastSpawnedPos = Vector3.zero;    //!< The position this object instance was placed to when last activated onto the map.
    [SerializeField] private Vector3 m_inactivePos;                      //!< Position to place the instance when it's not active in the world.
    [SerializeField] private float m_activePosY = 0;                     //!< The default Y-axis position of the prefab. Used so when the object is placed in the world, irregardless of where it's centre pivot point is, it can still be placed ON the ground.
    [SerializeField] private GameObject m_nullUser;                       //!< This is m_defaultNullObject from the Item class. Is what the instance's user is set to when it's not in use instead of setting a GameObject to system NULL which will throw errors.
    [SerializeField] private GameObject m_user;                          //!< The dog object using this instance at current, and if not in use will be set to m_nullUser.

    /** \fn Initialise
    *   \brief An initialising function to instantiate the ItemInstance which takes its required initial data then sets its user to null. Is always set an inactive by default.
    */
    public void Initialise(GameObject parentTransform, Vector3 inactivePos, GameObject nullObj)
    {
        transform.SetParent(parentTransform.transform);
        m_inactivePos = inactivePos;
        m_nullUser = nullObj;
        Deactivate();
    }

    /** \fn StartUse
    *   \brief Sets the instance's user to a specifed dog object.
    */
    public void StartUse(GameObject user) { m_user = user; }

    /** \fn EndUse
    *   \brief If the Item this InstanceItem derived from WASN'T single-use, after being used the object will be remain active by its user will just be reset to null so it can be used again.
    */
    public void EndUse() { m_user = m_nullUser; }

    /** \fn UsableFor
    *   \brief Returns whether or not this instance can be used by the specified object (should be a dog). It's usable if the instance doesn't currently have a user (it's set to the null object), or the user IS the object specified already, and this instance is [still] active.
    */
    public bool UsableFor(GameObject user) { return (((m_user == m_nullUser) || (m_user == user)) && CurrentlyActive()); }

    /** \fn Activate
    *   \brief Called when the instance's parent Item has been purchased in the store to activate this instance object to the map to the specified position. Also saves the instance's last activated position to this specifed destination.
    */
    public void Activate(Vector3 activePos)
    {
        m_lastSpawnedPos = activePos;
        gameObject.SetActive(true);
        transform.localPosition = activePos;
        transform.localPosition += new Vector3(0, m_activePosY, 0);
    }

    /** \fn Deactivate
    *   \brief If the Item this InstanceItem derived from is single-use, after being used the instance will be deactivated and moved back to its inactive position with its user reset to null. Not destroyed.
    */
    public void Deactivate()
    {
        if (CurrentlyActive())
        {
            m_user = m_nullUser;
            transform.position = m_inactivePos;
            gameObject.SetActive(false);
        }
    }

    /** \fn GetObject
    *   \brief Returns the GameObject of this ItemInstance.
    */
   // public GameObject GetObject() { return gameObject; }
    /** \fn IsObject
    *   \brief Returns a boolean of whether or not this ItemInstance is the object passed in as a parameter.
    */
    public bool IsObject(GameObject thisObject) { return (gameObject == thisObject); }
    /** \fn GetPosition
    *   \brief Returns the instance's current position. (Used by Item's TryGetClosestAvailableInstance() to compare instance's distances from the attempting dog user).
    */
    public Vector3 GetPosition() { return transform.position; }
    /** \fn GetLastSpawnPos
    *   \brief Returns the position this object was last activated to. 
    */
    public Vector3 GetLastSpawnPos() { return m_lastSpawnedPos; }
    /** \fn CurrentlyActive
    *   \brief Returns whether or not this instance object is currently active. (Used by Item when looking through its instance list to find pre-generated inactive copies to activate upon purchase).
    */
    public bool CurrentlyActive() { return gameObject.activeSelf; }
}