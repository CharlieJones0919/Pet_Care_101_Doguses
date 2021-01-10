using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetUIComponent : MonoBehaviour
{
    [SerializeField] private GameObject UIOutputScript;

    private void Start()
    {
        if (GetComponent<InputField>() != null)
        {
            UIOutputScript.GetComponent<DataDisplay>().generalDataDisplayUI.Add(transform.tag, GetComponent<InputField>().textComponent);
        }
        else if (GetComponent<Text>() != null)
        {
            UIOutputScript.GetComponent<DataDisplay>().generalDataDisplayUI.Add(transform.tag, GetComponent<Text>());
        }
        else if (GetComponent<Slider>() != null)
        {
            switch (transform.parent.parent.tag)
            {
                case ("CareData"):
                    UIOutputScript.GetComponent<DataDisplay>().careValueDisplayUI.Add(transform.tag, GetComponent<Slider>());
                    break;
                case ("PersonalityData"):
                    UIOutputScript.GetComponent<DataDisplay>().personalityValueDisplayUI.Add(transform.tag, GetComponent<Slider>());
                    break;
                default:
                    Debug.Log("Invalid Property Slider Tag: " + transform.parent.parent.tag);
                    break;
            }
        }
        else
        {
            Debug.Log("Invalid UI Retrieval Attempted: " + transform.name);
        }
    }
}
