/** \file GetUIComponent.cs
*  \brief Added to the UI elements used to display dog care/personality values and other dog data in the DogInfoPanel to send references to themselves to the DataDisplay class' UI Dictionaries.
*  Dictionaries can't be populated in the inspector, and this is faster than the DataDisplay class seaching for all the components manually by name or tag at runtime.
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** \class GetUIComponent
*  \brief Just contains a reference to the DataDisplay script in the DogInfoPanel, and a Start() function to add its component[s] to the appropriate dictionary in said script depending on which components this object can find in itself.
*/
public class GetUIComponent : MonoBehaviour
{
    [SerializeField] private DataDisplay UIOutputScript; //!< Reference to the DataDisplay script in the DogInfoPanel. Set as a serialized field in the inspector.

    /** \fn GetUIComponent
    *  \brief Add its component[s] to the appropriate dictionary in DataDisplay depending on which components can be found in this object and what this object's tag is. 
    *  If the object's tag is set to be the same as a DogCareValue/DogPersonalityValue it'll make the dictionary key one of those enums by parsing its tag as such.
    */
    private void Start()
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////// NAME INPUT TEXT INPUT BOX ////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        if (GetComponent<InputField>() != null)
        {
            UIOutputScript.generalDataDisplayUI.Add(transform.tag, GetComponent<InputField>().textComponent);
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////// GENERAL DOG INFORMATION TEXTBOXES ////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        else if (GetComponent<Text>() != null)
        {
            UIOutputScript.generalDataDisplayUI.Add(transform.tag, GetComponent<Text>());
        }
        else if (GetComponent<Slider>() != null)
        {
            switch (transform.parent.parent.tag)
            {
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////// DOG CARE VALUE TEXTBOXES & SLIDERS ////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                case ("CareData"):
                    if (Enum.IsDefined(typeof(DogCareValue), transform.tag))
                    {
                        UIOutputScript.careValueDisplayUI.Add((DogCareValue)Enum.Parse(typeof(DogCareValue), transform.tag), new KeyValuePair<Slider, Text>(GetComponent<Slider>(), transform.GetChild(0).GetComponent<Text>()));
                    }
                    else { Debug.Log("Attempting add the following UI component with an invalid care value tag: " + name.ToString() + ", " + transform.tag.ToString()); }
                    break;
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////// DOG PERSONALITY VALUE TEXTBOXES & SLIDERS ////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
