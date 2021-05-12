/** \file TipPopUp.cs
*   \brief Contains a simple class for outputting a message log to the player during runtime.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** \class TipPopUp
*   \brief A class for outputting messages to the player during runtime for a number of seconds before deactivating using a couroutine.
*/
public class TipPopUp : MonoBehaviour
{   
    [SerializeField] private Text tipText = null;           //!< The text UI to output the message[s] to.
    [SerializeField] private List<string> messageQueue = new List<string>(); //!< A list of string messages waiting to be displayed.
    [SerializeField] private float timePerCharacter = 0;    //!< Scaler of time per character to calculate the total amount of time the message should be displayed for.
    [SerializeField] private float secondsToDisplay;        //!< How much time the current message should be displayed for based on the number of characters in it.

    /** \fn DisplayTipMessage
    *   \brief Takes a string message and adds it to the message queue if it isn't already in there. If the tip pop-up box isn't currently active it'll immediately activate it then immediately output this message as the first/only.
    */
    public void DisplayTipMessage(string message)
    {
        if (!messageQueue.Contains(message))
        {
            messageQueue.Add(message);

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
               // StartCoroutine(TipTimer(message));
            }
        }
    }

    public void Update()
    {
        TipTimer(messageQueue[0]);
    }


    /** \fn TipTimer
    *   \brief A couroutine to output the given message to the textbox, wait for the appropriate time for the length of that message, then remove it from the message queue. 
    *   If there are more messages in the queue, it'll display the next one, otherwise it'll just deactivate the pop-up box.
    */
    private void TipTimer(string tip = null)
    {
        if (tipText.text != tip) { tipText.text = tip; SetTipDisplayTime(tip); }

        if (secondsToDisplay <= 0)
        {
            messageQueue.Remove(tipText.text);
            if (messageQueue.Count == 0) { gameObject.SetActive(false); }
        }
        else { secondsToDisplay -= Time.deltaTime; };
    }

    /** \fn SetTipDisplayTime
    *   \brief Sets the amount of time the next message should be displayed for to the number of characters in the given tip string multiplied by timePerCharacter. 
    */
    private void SetTipDisplayTime(string tip)
    {
        secondsToDisplay = tip.Length * timePerCharacter;
    }

    /** \fn TipClosed
    *   \brief Called by the close button of the tip message box when pressed. Removes the current message from the que prematurely then displays the next message if there is one, and if not, closes the message box.
    */
    public void TipClosed()
    {
        if (messageQueue.Count > 0) { secondsToDisplay = 0; }
        else
        {
            gameObject.SetActive(false);
        }
    }
}