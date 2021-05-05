using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataDisplay : MonoBehaviour
{
    [SerializeField] private Controller controller;
    [SerializeField] private Dog focusedDog;
    [SerializeField] private GameObject newDogPanel;

    public Dictionary<string, Text> generalDataDisplayUI = new Dictionary<string, Text>();
    public Dictionary<DogCareValue, KeyValuePair<Slider, Text>> careValueDisplayUI = new Dictionary<DogCareValue, KeyValuePair<Slider, Text>>();
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
        }
    }

    private void OnDisable()
    {
        focusedDog = null;
        controller.NotFocusedOnDog();
    }

    private void FixedUpdate()
    {
        if (focusedDog != null)
        {
            foreach (KeyValuePair<DogCareValue, KeyValuePair<Slider, Text>> sliderValue in careValueDisplayUI)
            {
                sliderValue.Value.Key.value = focusedDog.m_careValues[sliderValue.Key].GetValue();
                sliderValue.Value.Value.text = string.Format("{0:F2}%", sliderValue.Value.Key.value);
            }

            foreach (KeyValuePair<DogPersonalityValue, Slider> sliderValue in personalityValueDisplayUI)
            {
                sliderValue.Value.value = focusedDog.m_personalityValues[sliderValue.Key].GetValue();
            }
        }
    }

    

    public void SetFocusDog(Dog focus) { focusedDog = focus; gameObject.SetActive(true); }

    public void ActivateNewDogPanel() { newDogPanel.SetActive(true); }
    public void NewDogAdded()
    {
        controller.PAUSE(false);
        gameObject.SetActive(true);
        newDogPanel.SetActive(false);
    }

    public GameObject GetFocusDog()
    {
        if (focusedDog != null) { return focusedDog.gameObject; }
        else { return null; }
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

