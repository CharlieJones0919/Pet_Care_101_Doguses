using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InfoPanel : MonoBehaviour
{
    public Dog focusedDog;

    public Dictionary<string, Text> generalDataDisplayUI = new Dictionary<string, Text>();
    public Dictionary<string, Slider> careValueDisplayUI = new Dictionary<string, Slider>();
    public Dictionary<string, Slider> personalityValueDisplayUI = new Dictionary<string, Slider>();

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (focusedDog != null)
        {
            foreach (KeyValuePair<string, Text> textElement in generalDataDisplayUI)
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
                        {
                            Debug.Log("Attempting to set incorrectly tagged Text UI element: " + textElement.Key);
                        }
                        break;
                }
            }

            foreach (KeyValuePair<string, Slider> sliderValue in personalityValueDisplayUI)
            {
                foreach (Property propValue in focusedDog.personalityValues)
                {
                    if (sliderValue.Key == propValue.GetPropertyName())
                    {
                        sliderValue.Value.value = propValue.GetValue();
                        break;
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (focusedDog != null)
        {
            foreach (KeyValuePair<string, Slider> sliderValue in careValueDisplayUI)
            {
                foreach (Property propValue in focusedDog.careValues)
                {
                    if (sliderValue.Key == propValue.GetPropertyName())
                    {
                        sliderValue.Value.value = propValue.GetValue();
                        break;
                    }
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

            foreach (KeyValuePair<string, Text> textElement in generalDataDisplayUI)
            {
                if (textElement.Key == "NameText")
                {
                    textElement.Value.text = focusedDog.m_name;
                    break;
                }
            }

            ToggleNameInputField(false);
        }
    }

    public void ToggleNameInputField(bool activeState)
    {
        foreach (KeyValuePair<string, Text> textElement in generalDataDisplayUI)
        {
            if (textElement.Key == "NameTextbox")
            {
                textElement.Value.transform.parent.gameObject.SetActive(activeState);
                break;
            }
        }
    }
}

