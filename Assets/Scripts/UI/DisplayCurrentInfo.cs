using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCurrentInfo : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel;

    private void Start()
    {
        if (GetComponent<InputField>() != null)
        {
            infoPanel.GetComponent<InfoPanel>().textDisplayUI.Add(transform.tag, GetComponent<InputField>().textComponent);
        }
        else if (GetComponent<Text>() != null)
        {
            infoPanel.GetComponent<InfoPanel>().textDisplayUI.Add(transform.tag, GetComponent<Text>());
        }
        else if (GetComponent<Slider>() != null)
        {
            if (transform.tag.Substring(0, 2) == "S_")
            infoPanel.GetComponent<InfoPanel>().dailyValueDisplayUI.Add(transform.tag, GetComponent<Slider>());
            else
            infoPanel.GetComponent<InfoPanel>().dynamicValueDisplayUI.Add(transform.tag, GetComponent<Slider>());
        }
        else
        {
            Debug.Log("Invalid UI Retrieval Attempted.");
        }
    }
}
