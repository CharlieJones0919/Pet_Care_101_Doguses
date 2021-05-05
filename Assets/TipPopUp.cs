using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipPopUp : MonoBehaviour
{
    [SerializeField] private Text tipText;
    [SerializeField] private List<string> messageQue = new List<string>();
    [SerializeField] private float timePerCharacter;
    private float secondsToDisplay;

    private bool currentlyDisplaying = false;

    public void DisplayTipMessage(string message)
    {
        if (!messageQue.Contains(message))
        {
            messageQue.Add(message);

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                StartCoroutine(TipTimer(message));
            }
        }
    }

    private IEnumerator TipTimer(string tip = null)
    {
        tipText.text = tip;
        SetTipDisplayTime(tip);
        yield return new WaitForSeconds(secondsToDisplay);
        messageQue.Remove(tipText.text);
        if (messageQue.Count > 0) { StartCoroutine(TipTimer(messageQue[0])); }
        else { gameObject.SetActive(false); }
    }

    private void SetTipDisplayTime(string tip)
    {
        secondsToDisplay = tip.Length * timePerCharacter;
    }

    public void TipClosed()
    {
        messageQue.Remove(tipText.text);

        if (messageQue.Count > 0) { StartCoroutine(TipTimer(messageQue[0])); }
        else
        {
            StopCoroutine(TipTimer());
            gameObject.SetActive(false);
        }
    }
}
