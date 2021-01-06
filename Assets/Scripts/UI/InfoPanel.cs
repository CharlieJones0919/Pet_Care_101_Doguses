using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InfoPanel : MonoBehaviour
{
    public Dog focusedDog;

    public Dictionary<string, Text> textDisplayUI = new Dictionary<string, Text>();
    public Dictionary<string, Slider> dynamicValueDisplayUI = new Dictionary<string, Slider>();
    public Dictionary<string, Slider> dailyValueDisplayUI = new Dictionary<string, Slider>();

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (focusedDog != null)
        {
            foreach (KeyValuePair<string, Text> textElement in textDisplayUI)
            {
                switch (textElement.Key)
                {
                    case ("NameText"):
                        if (focusedDog.m_name != "")
                        {
                            ToggleNameInputField(false);
                            textElement.Value.text = focusedDog.m_name;
                        }
                        else
                            ToggleNameInputField(true);
                        break;
                    case ("AgeText"):
                        textElement.Value.text = focusedDog.m_age.ToString() + " yrs";
                        break;
                    case ("BreedText"):
                        textElement.Value.text = focusedDog.m_breed;
                        break;
                    default:
                        if (textElement.Key != "NameTextbox")
                            Debug.Log("Attempting to set an incorrectly tagged Text UI element.");
                        break;
                }
            }

            foreach (KeyValuePair<string, Slider> valueElement in dailyValueDisplayUI)
            {
                switch (valueElement.Key)
                {
                    case ("S_ToleranceValue"):
                        valueElement.Value.value = focusedDog.m_tolerance;
                        break;
                    case ("S_AffectionValue"):
                        valueElement.Value.value = focusedDog.m_affection;
                        break;
                    case ("S_IntelligenceValue"):
                        valueElement.Value.value = focusedDog.m_intelligence;
                        break;
                    case ("S_EnergyValue"):
                        valueElement.Value.value = focusedDog.m_energy;
                        break;
                    case ("S_ObedienceValue"):
                        valueElement.Value.value = focusedDog.m_obedience;
                        break;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (focusedDog != null)
        {
            foreach (KeyValuePair<string, Slider> valueElement in dynamicValueDisplayUI)
            {
                switch (valueElement.Key)
                {
                    case ("HungerValue"):
                        valueElement.Value.value = focusedDog.m_hunger;
                        break;
                    case ("AttentionValue"):
                        valueElement.Value.value = focusedDog.m_attention;
                        break;
                    case ("RestValue"):
                        valueElement.Value.value = focusedDog.m_rest;
                        break;
                    case ("HygieneValue"):
                        valueElement.Value.value = focusedDog.m_hygiene;
                        break;
                    case ("HealthValue"):
                        valueElement.Value.value = focusedDog.m_health;
                        break;
                    case ("HappinessValue"):
                        valueElement.Value.value = focusedDog.m_happiness;
                        break;
                    case ("BondValue"):
                        valueElement.Value.value = focusedDog.m_bond;
                        break;
                    default:
                        Debug.Log("Attempting to set an incorrectly tagged Slider UI element.");
                        break;
                }
            }
        }
    }

    public void SetFocusDog(Dog focus)
    {
        focusedDog = focus;
    }

    public void ClearFocusDog()
    {
        focusedDog = null;
    }

    public void SetNewDogName(string name)
    {
        if ((focusedDog != null) && (name != ""))
        {
            focusedDog.m_name = name;

            foreach (KeyValuePair<string, Text> textElement in textDisplayUI)
            {
                switch (textElement.Key)
                {
                    case ("NameText"):
                        textElement.Value.text = focusedDog.m_name;
                        break;
                }
            }

            ToggleNameInputField(false);
        }
    }

    public void ToggleNameInputField(bool activeState)
    {
        foreach (KeyValuePair<string, Text> textElement in textDisplayUI)
        {
            if (textElement.Key == "NameTextbox")
            {
                textElement.Value.transform.parent.gameObject.SetActive(activeState);
                break;
            }
        }
    }
}

