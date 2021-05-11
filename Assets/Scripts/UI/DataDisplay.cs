/** \file DataDisplay.cs
*  \brief Contains a class for outputting data about a selected dog. This is attached to the DogInfoPanel UI object.
*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** \class DataDisplay
*  \brief Contains a reference to a selected dog and the DogInfoPanel UI components as sent in by their GetUIComponent scripts. While active will output the data specific to that dog in the panel (e.g. their name, breed, hunger level, etc...).
*/
public class DataDisplay : MonoBehaviour
{
    [SerializeField] private Controller controller;     //!< Reference to the game controller.
    [SerializeField] private Dog focusedDog;            //!< The dog to output the data of.
    [SerializeField] private GameObject newDogPanel;    //!< The adoption screen panel to activate when a new dog is added.

    public Dictionary<string, Text> generalDataDisplayUI = new Dictionary<string, Text>();                                                       //!< UI for outputting data other than Care/Personality values. (E.g. Name, Breed, age, etc...).
    public Dictionary<DogCareValue, KeyValuePair<Slider, Text>> careValueDisplayUI = new Dictionary<DogCareValue, KeyValuePair<Slider, Text>>(); //!< UI for outputting the dog's care values, paired with the percentage text and a key for the specific care value this slider/text are for.
    public Dictionary<DogPersonalityValue, Slider> personalityValueDisplayUI = new Dictionary<DogPersonalityValue, Slider>();                    //!< UI for outputting the dog's personality values, with a key for the specific personality value this slider is for.

    /** \fn OnEnable
    *  \brief When the panel is activated, its general data UI elements to the values of the current dog of focus. (This will be set via Control as called by CameraControl when a dog is tapped/clicked).
    */
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

    /** \fn OnDisable
    *  \brief When the panel is de-activated, it tells the controller to unset all the dogs' "IS_FOCUS_DOG" facts as true.
    */
    private void OnDisable()
    {
        focusedDog = null;
        controller.NotFocusedOnDog();
    }

    /** \fn FixedUpdate
    *  \brief While the InfoPanel is active, this function updates the slider and text UI element's values to those of the focused dog. 
    */
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

    /** \fn SetFocusDog
    *  \brief Called by CameraControl when a dog is tapped/clicked. Sets this script's reference to a dog to the one passed in after being hit by a camera raycast. 
    */
    public void SetFocusDog(Dog focus) { focusedDog = focus; gameObject.SetActive(true); }

    /** \fn ActivateNewDogPanel
     *  \brief Activates the adoption screen when a new dog is generated. Called by the Control class. 
     */
    public void ActivateNewDogPanel() { newDogPanel.SetActive(true); }

    /** \fn NewDogAdded
     *  \brief Unpauses the game, activates this object (the dog info panel), and deactivates the adoption screen panel. This is called by the confirmation button on the adoption panel to continue the game.
     */
    public void NewDogAdded()
    {
        controller.PAUSE(false);
        gameObject.SetActive(true);
        newDogPanel.SetActive(false);
    }

    /** \fn GetFocusDog
     *  \brief Returns the script's current focus dog data is being output for.
     */
    public GameObject GetFocusDog()
    {
        if (focusedDog != null) { return focusedDog.gameObject; }
        else { return null; }
    }

    /** \fn SetNewDogName
     *  \brief Attached to the name input textbox to set the focus dog's name on data entry.
     */
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

    /** \fn ToggleNameInputField
     *  \brief Sets the active state of the name input button. Is used to deactivate the field when a name is entered to show the name in text behind it, then reactivate it again when the edit button is pressed.
     */
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

