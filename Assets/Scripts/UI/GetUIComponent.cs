using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetUIComponent : MonoBehaviour
{
    [SerializeField] private DataDisplay UIOutputScript;

    private void Start()
    {
        if (GetComponent<InputField>() != null)
        {
            UIOutputScript.generalDataDisplayUI.Add(transform.tag, GetComponent<InputField>().textComponent);
        }
        else if (GetComponent<Text>() != null)
        {
            UIOutputScript.generalDataDisplayUI.Add(transform.tag, GetComponent<Text>());
        }
        else if (GetComponent<Slider>() != null)
        {
            switch (transform.parent.parent.tag)
            {
                case ("CareData"):
                    if (Enum.IsDefined(typeof(DogCareValue), transform.tag))
                    {
                        UIOutputScript.careValueDisplayUI.Add((DogCareValue)Enum.Parse(typeof(DogCareValue), transform.tag), new KeyValuePair<Slider, Text>(GetComponent<Slider>(), transform.GetChild(0).GetComponent<Text>()));
                    }
                    else { Debug.Log("Attempting add the following UI component with an invalid care value tag: " + name.ToString() + ", " + transform.tag.ToString()); }
                    break;
                case ("PersonalityData"):
                    if (Enum.IsDefined(typeof(DogPersonalityValue), transform.tag))
                    {
                        UIOutputScript.personalityValueDisplayUI.Add((DogPersonalityValue)Enum.Parse(typeof(DogPersonalityValue), transform.tag), GetComponent<Slider>());
                    }
                    else { Debug.Log("Attempting add the following UI component with an invalid personality value tag: " + name.ToString() + ", " + transform.tag.ToString()); }
                    break;
                default:
                    Debug.Log("Invalid Property Slider Parent Tag: " + transform.parent.parent.tag);
                    break;
            }
        }
        else
        {
            Debug.Log("Invalid UI Retrieval Attempted: " + transform.name);
        }
    }
}
