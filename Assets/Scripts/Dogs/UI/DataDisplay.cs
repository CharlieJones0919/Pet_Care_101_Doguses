using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataDisplay : MonoBehaviour
{
    [SerializeField] private GameObject controller;
    [SerializeField] private Dog focusedDog;

    public Dictionary<string, Text> generalDataDisplayUI = new Dictionary<string, Text>();
    public Dictionary<DogCareValue, Slider> careValueDisplayUI = new Dictionary<DogCareValue, Slider>();
    public Dictionary<DogPersonalityValue, Slider> personalityValueDisplayUI = new Dictionary<DogPersonalityValue, Slider>();

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
                            textElement.Value.text = focusedDog.m_name.Replace("_", " ");
                        }
                        else
                            ToggleNameInputField(true);
                        break;
                    case ("AgeText"):
                        textElement.Value.text = focusedDog.m_age.ToString() + " yrs";
                        break;
                    case ("BreedText"):
                        textElement.Value.text = focusedDog.m_breed.ToString().Replace("_", " ");
                        break;
                    default:
                        if (textElement.Key != "NameTextbox")
                        {
                            Debug.Log("Attempting to set incorrectly tagged Text UI element: " + textElement.Key);
                        }
                        break;
                }
            }

            foreach (KeyValuePair<DogPersonalityValue, Slider> sliderValue in personalityValueDisplayUI)
            {
                sliderValue.Value.value = focusedDog.m_personalityValues[sliderValue.Key].GetValue();
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

        foreach (KeyValuePair<DogCareValue, Slider> sliderValue in careValueDisplayUI)
        {
            sliderValue.Value.value = 0;
        }

        foreach (KeyValuePair<DogPersonalityValue, Slider> sliderValue in personalityValueDisplayUI)
        {
            sliderValue.Value.value = 0;
        }
    }

    private void FixedUpdate()
    {
        if (focusedDog != null)
        {
            foreach (KeyValuePair<DogCareValue, Slider> sliderValue in careValueDisplayUI)
            {
                sliderValue.Value.value = focusedDog.m_careValues[sliderValue.Key].GetValue();
            }

            foreach (KeyValuePair<DogPersonalityValue, Slider> sliderValue in personalityValueDisplayUI)
            {
                sliderValue.Value.value = focusedDog.m_personalityValues[sliderValue.Key].GetValue();
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
                    focusedDog.gameObject.name = name;
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

