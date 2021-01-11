using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Consumable 
{
    private GameObject m_object;
    private Image m_icon;

    private string m_name;
    private float m_price;
    private string m_description;

    private float m_fufillmentValue;
    private float m_healthiness;

    private GameObject m_user;
    private bool m_usable;

    public Consumable(GameObject objectRef, GameObject objectParent, Vector3 position, string name, float price, string description, float fufillment, float healthModifiyer) 
    {
        m_object = MonoBehaviour.Instantiate(objectRef, position, Quaternion.identity);
        m_object.transform.parent = objectParent.transform;
        m_object.name = m_object.name + " [" + name + "]";

        m_name = name;
        m_price = price;

        m_description = description;
        m_fufillmentValue = fufillment;
        m_healthiness = healthModifiyer;

        m_user = null;
        m_usable = true;
    }

    public Vector3 GetPosition()
    {
        return m_object.transform.position;
    }

    public GameObject GetObject()
    {
        return m_object;
    }

    public void StartUse(GameObject user)
    {
        m_user = user;
    }

    public void EndUse()
    {
        m_user = null;
        m_usable = false;
    }

    public GameObject GetUser()
    {
        return m_user;
    }

    public bool GetUsable()
    {
        return m_usable;
    }

    public float GetFufillmentValue()
    {
        return m_fufillmentValue;
    }
}
