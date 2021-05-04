using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTime : MonoBehaviour
{
    [SerializeField] private Text dateTextbox;
    [SerializeField] private Text timeTextbox;
    [SerializeField] private Text totalDaysTextbox;
    [SerializeField] private List<Text> playerMoneyTextboxes = new List<Text>();
    [SerializeField] private Controller controller;
    [SerializeField] private DogGeneration dogGenerator;

    [SerializeField] private const int timeAdjustment = 72; // 20 minutes of real time is 1 day in game time.
    private const int allowance = 80;
    [SerializeField] private static float playerMoney = 0;

    [SerializeField] private static float gameTimeSeconds = 0;
    [SerializeField] private static int gameTimeMinutes = 0;
    [SerializeField] private static int gameTimeHours = 0;
    [SerializeField] private static int gameTimeDays = 0;
    [SerializeField] private static int gameTimeDaysTotal = 0;
    [SerializeField] private static int gameTimeWeeks = 0;
    [SerializeField] private static int gameTimeYears = 0;

    private delegate void DailyFunction();
    private List<DailyFunction> dailyFunctions = new List<DailyFunction>();

    private delegate void WeeklyFunction();
    private List<WeeklyFunction> weeklyFunctions = new List<WeeklyFunction>();

    private delegate void BiannualFunction();
    private List<BiannualFunction> biannualFunctions = new List<BiannualFunction>();

    private delegate void AnnualFunction();
    private List<AnnualFunction> annualFunctions = new List<AnnualFunction>();

    private void Start()
    {
        InstantiateFunctionLists();

        WeeklyEvents(); 
    }

    private void Update()
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
                    DailyEvents();
                    gameTimeHours = 0;
                    gameTimeDays++;
                    gameTimeDaysTotal++;

                    if (gameTimeDays >= 7)
                    {
                        WeeklyEvents();
                        gameTimeDays = 0;
                        gameTimeWeeks++;

                        if (gameTimeDays >= 27)
                        {
                            BiannualEvents();

                            if (gameTimeDays >= 54)
                            {
                                AnnualEvents();
                                gameTimeWeeks = 0;
                                gameTimeYears++;
                            }
                        }
                    }
                }
            }
        }

        dateTextbox.text = String.Format("WEEK: {0:D1}     DAY: {1:D1}     YEAR: {2:D1}", gameTimeWeeks, gameTimeDays, gameTimeYears);
        timeTextbox.text = String.Format("TIME: [ {0:D2}:{1:D2} ]", gameTimeHours, gameTimeMinutes);
        totalDaysTextbox.text = ("TOTAL DAYS: " + gameTimeDaysTotal);
    }

    public static int GetSecondMultiplier() { return timeAdjustment; }
    public static float GetPlayerMoney() { return playerMoney; }

    public static float GetGameTimeSeconds() { return gameTimeSeconds; }
    public static int GetGameTimeMinutes() { return gameTimeMinutes; }
    public static int GetGameTimeHours() { return gameTimeHours; }
    public static int GetGameTimeDays() { return gameTimeDays; }
    public static int GetGameTimeWeeks() { return gameTimeWeeks; }

    private void UpdateMoneyValue(float modification = 0.0f)
    {
        if (modification > 0.0f) { playerMoney += modification; }

        if (playerMoneyTextboxes.Count > 0)
        {
            foreach (Text textbox in playerMoneyTextboxes)
            {
                textbox.text = string.Format("{0:F2}", playerMoney);
            }
        }
    }

    private void InstantiateFunctionLists()
    {
        weeklyFunctions.Add(WeeklyFunction_PayPlayer);
        weeklyFunctions.Add(WeeklyFunction_OfferDog);
    }

    private void DailyEvents()
    {
        if (dailyFunctions.Count > 0) { foreach (DailyFunction function in dailyFunctions) function(); }
    }
    private void WeeklyEvents()
    {
        if (weeklyFunctions.Count > 0) { foreach (WeeklyFunction function in weeklyFunctions) function(); }
    }
    private void BiannualEvents()
    {
        if (biannualFunctions.Count > 0) { foreach (BiannualFunction function in biannualFunctions) function(); }
    }
    private void AnnualEvents()
    {
        if (annualFunctions.Count > 0) { foreach (AnnualFunction function in annualFunctions) function(); }
    }

    private void WeeklyFunction_PayPlayer() { UpdateMoneyValue(allowance); }
    private void WeeklyFunction_OfferDog() { dogGenerator.GenerateRandomNewDog(); }


}