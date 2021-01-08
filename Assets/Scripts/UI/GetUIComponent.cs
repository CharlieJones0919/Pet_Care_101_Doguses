using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetUIComponent : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel;

    private void Start()
    {
        if (GetComponent<InputField>() != null)
        {
            infoPanel.GetComponent<InfoPanel>().generalDataDisplayUI.Add(transform.tag, GetComponent<InputField>().textComponent);
        }
        else if (GetComponent<Text>() != null)
        {
            infoPanel.GetComponent<InfoPanel>().generalDataDisplayUI.Add(transform.tag, GetComponent<Text>());
        }
        else if (GetComponent<Slider>() != null)
        {
            switch (transform.parent.parent.tag)
            {
                case ("CareData"):
                    infoPanel.GetComponent<InfoPanel>().careValueDisplayUI.Add(transform.tag, GetComponent<Slider>());
                    break;
                case ("PersonalityData"):
                    infoPanel.GetComponent<InfoPanel>().personalityValueDisplayUI.Add(transform.tag, GetComponent<Slider>());
                    break;
                default:
                    Debug.Log("Invalid Property Slider Tag: " + transform.parent.parent.tag);
                    break;
            }
        }
        else
        {
            Debug.Log("Invalid UI Retrieval Attempted.");
        }
    }
}
