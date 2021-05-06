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
    [SerializeField] private Controller controller;
    [SerializeField] private DogGeneration dogGenerator;

    [SerializeField] private int timeAdjustment = 72 * 500; 

    [SerializeField] private static float gameTimeSeconds = 0;
    [SerializeField] private static int gameTimeMinutes = 0;
    [SerializeField] private static int gameTimeHours = 7;
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
        DailyEvents();
        WeeklyEvents();

        controller.tipPopUp.DisplayTipMessage("This is your first dog! You should buy them a bed and some food with your first daily allowance at the store. (Bottom left).");
        controller.tipPopUp.DisplayTipMessage("You're currently recieving a bonus because your dog has peak care values.");
        controller.tipPopUp.DisplayTipMessage("Use the blue buttons to control the game's speed.");
        controller.tipPopUp.DisplayTipMessage("And remember, always do your own reseach if you're going to adopt a dog or any pet. While intended to be educational, the information given in this game is gamified/anacdotal and not academically cited.");
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

    public int GetSecondMultiplier() { return timeAdjustment; }

    public static float GetGameTimeSeconds() { return gameTimeSeconds; }
    public static int GetGameTimeMinutes() { return gameTimeMinutes; }
    public static int GetGameTimeHours() { return gameTimeHours; }
    public static int GetGameTimeDays() { return gameTimeDays; }
    public static int GetGameTimeWeeks() { return gameTimeWeeks; }

    private void InstantiateFunctionLists()
    {
        dailyFunctions.Add(DailyFunction_PayPlayer);
        weeklyFunctions.Add(WeeklyFunction_OfferDog);
        annualFunctions.Add(AnnualFunction_AgeDogs);
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

    private void DailyFunction_PayPlayer() { controller.GiveAllowance(); }
    private void WeeklyFunction_OfferDog() {
        if (controller.NumberOfDogs() < controller.dogLimit) { dogGenerator.GenerateRandomNewDog(); }
        else { controller.tipPopUp.DisplayTipMessage("No new dog this week. You're already caring for the maximum number of dogs."); }
    }
    private void AnnualFunction_AgeDogs() { controller.AgeDogs(); }

}