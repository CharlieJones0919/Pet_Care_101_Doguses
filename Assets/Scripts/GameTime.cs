using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTime : MonoBehaviour
{
    public Text dateTextbox;
    public Text timeTextbox;
    private DogGeneration dogGenerator;

    [SerializeField] private const int timeAdjustment = 72; // 20 minutes of real time is 1 day in game time.
    [SerializeField] private float gameTimeSeconds;
    [SerializeField] private int gameTimeMinutes;
    [SerializeField] private int gameTimeHours;
    [SerializeField] private int gameTimeDays;
    [SerializeField] private int gameTimeWeeks;

    private void Start()
    {
        //Read previous game time from constant external save file.
        dogGenerator = GetComponent<DogGeneration>();
        OfferDog();
    }

    public void Update()
    {
        gameTimeSeconds += Time.deltaTime * timeAdjustment;

        if (gameTimeSeconds >= 60)
        {
            gameTimeSeconds = 0.0f;
            gameTimeMinutes++;

            if (gameTimeMinutes >= 60)
            {
                gameTimeMinutes = 0;
                gameTimeHours++;

                if (gameTimeHours >= 24)
                {
                    gameTimeHours = 0;
                    gameTimeDays++;

                    if (gameTimeDays >= 7)
                    {
                        gameTimeDays = 0;
                        gameTimeWeeks++;

                        OfferDog();
                    }
                }
            }
        }

        dateTextbox.text = String.Format("Week: {0:D1}     Days: {1:D1}", gameTimeWeeks, gameTimeDays);
        timeTextbox.text = String.Format("Time: [ {0:D2}:{1:D2} ]", gameTimeHours, gameTimeMinutes);
    }

    public float GetGameTimeSeconds() { return gameTimeSeconds; }
    public int GetGameTimeMinutes() { return gameTimeMinutes; }
    public int GetGameTimeHours() { return gameTimeHours; }
    public int GetGameTimeDays() { return gameTimeDays; }
    public int GetGameTimeWeeks() { return gameTimeWeeks; }

    public float getSecondMultiplier() { return timeAdjustment; }

    private void OfferDog()
    {

    }
}