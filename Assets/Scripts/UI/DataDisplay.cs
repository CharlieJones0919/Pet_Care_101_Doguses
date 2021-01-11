using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataDisplay : MonoBehaviour
{
    [SerializeField] private GameObject controller;
    [SerializeField] private Dog focusedDog;

    public Dictionary<string, Text> generalDataDisplayUI = new Dictionary<string, Text>();
    public Dictionary<string, Slider> careValueDisplayUI = new Dictionary<string, Slider>();
    public Dictionary<string, Slider> personalityValueDisplayUI = new Dictionary<string, Slider>();

    private void Start()
    {
        bool[] textSetSuccessfully = new bool[2] { false, false };

        foreach (KeyValuePair<string, Text> textElement in generalDataDisplayUI)
        {
            if (textElement.Key == "DateText")
            {
                controller.GetComponent<DogController>().gameTime.dateTextbox = textElement.Value;
                textSetSuccessfully[0] = true;
            }
            else if (textElement.Key == "TimeText")
            {
                controller.GetComponent<DogController>().gameTime.timeTextbox = textElement.Value;
                textSetSuccessfully[1] = true;
            }
        }

        if (textSetSuccessfully[0] && textSetSuccessfully[1])
        {
            generalDataDisplayUI.Remove("DateText");
            generalDataDisplayUI.Remove("TimeText");
        }
        else
        {
            Debug.Log("Date and Time Textboxes Failed to be Set");
        }

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
                foreach (Property propValue in focusedDog.m_personalityValues)
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

    private void OnDisable()
    {
        focusedDog = null;

        foreach (KeyValuePair<string, Text> textElement in generalDataDisplayUI)
        {
            textElement.Value.text = "UNKNOWN - " + textElement.Key;
        }

        foreach (KeyValuePair<string, Slider> sliderValue in personalityValueDisplayUI)
        {
            sliderValue.Value.value = 0;
        }

        foreach (KeyValuePair<string, Slider> sliderValue in careValueDisplayUI)
        {
            sliderValue.Value.value = 0;
        }
    }

    private void FixedUpdate()
    {
        if (focusedDog != null)
        {
            foreach (KeyValuePair<string, Slider> sliderValue in careValueDisplayUI)
            {
                foreach (Property propValue in focusedDog.m_careValues)
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

    public GameObject GetFocusDog()
    {
        if (focusedDog != null)
            return focusedDog.gameObject;
        else
            return null;
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

