using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Consumable
{
    [SerializeField] private GameObject m_object;
    [SerializeField] private Image m_icon;

    private string m_name;
    private float m_price;
    private string m_description;

    private float m_fufillmentValue;
    private float m_healthiness;

    public Consumable(string name, float price, string description, float fufillment, float healthModifiyer) 
    {
        m_name = name;
        m_price = price;

        m_description = description;
        m_fufillmentValue = fufillment;
        m_healthiness = healthModifiyer;
    }
}
