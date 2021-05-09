/** \file GameTime.cs
*   \brief Controls time keeping and game-time based events (like dog generation).
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** \class GameTime
*   \brief Keeps game-time (system time modified by an speed adjustment for gameification) and calls time triggered events like daily payment and weekly dog offerings.
*/
public class GameTime : MonoBehaviour
{
    [SerializeField] private Controller controller;      //!< Reference to the game controller.
    [SerializeField] private DogGeneration dogGenerator; //!< Reference to DogGeneration so a new dog can be generated every in-game week.
    [SerializeField] private int timeAdjustment;         //!< Multiplier from regular seconds to game time.
                                                         
    [SerializeField] private Text dateTextbox;           //!< UI to output the current in-game date to.
    [SerializeField] private Text timeTextbox;           //!< UI to output the current in-game time to.
    [SerializeField] private Text totalDaysTextbox;      //!< UI to output the total number of in-game days passed since the game started.

    private static float gameTimeSeconds = 0;            //!< Game-time Seconds. 
    private static int   gameTimeMinutes = 0;            //!< Game-time Minutes. 
    private static int   gameTimeHours = 20;              //!< Game-time Hours. (Game starts at 8AM).
    private static int   gameTimeDays = 0;               //!< Game-time Days.
    private static int   gameTimeDaysTotal = 0;          //!< Game-time Days Total.
    private static int   gameTimeWeeks = 0;              //!< Game-time Weeks. 
    private static int   gameTimeYears = 0;              //!< Game-time Years. 

    private delegate void TimeFunction();                //!< Delegate function for time triggered functions to define. (So they can be added to lists as generic types for iterating through to call at the appropritate time stamps).
    private List<TimeFunction> dailyFunctions = new List<TimeFunction>();       //!< List of daily functions. 
    private List<TimeFunction> weeklyFunctions = new List<TimeFunction>();      //!< List of weekly functions. 
    private List<TimeFunction> biannualFunctions = new List<TimeFunction>();    //!< List of biannual functions.
    private List<TimeFunction> annualFunctions = new List<TimeFunction>();      //!< List of annual functions. 

    /** \fn Start
    *   \brief Adds the time functions to their lists, calls them on first game start, then displays the initial game tip messages.
    */
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

    /** \fn Update
    *   \brief Updates the in-game time and required date/time variables using the time adjustment value, then outputs the new values to the UI. Also calls the time based functions at their required intervals.
    */
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

    /** \fn GetSecondMultiplier
     *   \brief Returns the time adjustment value. Not currentyly used anywhere but could be used if a value in another class needed scaling in time with the adjustment.
     */
    public int GetSecondMultiplier() { return timeAdjustment; }

    /** \fn GetGameTimeSeconds
     *   \brief Returns the seconds passed in game time.
     */
    public static float GetGameTimeSeconds() { return gameTimeSeconds; }
    /** \fn GetGameTimeMinutes
     *   \brief Returns the minutes passed in game time.
     */
    public static int GetGameTimeMinutes() { return gameTimeMinutes; }
    /** \fn GetGameTimeHours
    *   \brief Returns the hours passed in game time.
    */
    public static int GetGameTimeHours() { return gameTimeHours; }
    /** \fn GetGameTimeDays
    *   \brief Returns the days passed in game time.
    */
    public static int GetGameTimeDays() { return gameTimeDays; }
    /** \fn GetGameTimeWeeks
    *   \brief Returns the weeks passed in game time.
    */
    public static int GetGameTimeWeeks() { return gameTimeWeeks; }

    /** \fn InstantiateFunctionLists
     *   \brief Adds the instantiations of TimeFunctions to their appropriate lists.
     */
    private void InstantiateFunctionLists()
    {
        dailyFunctions.Add(DailyFunction_PayPlayer);
        weeklyFunctions.Add(WeeklyFunction_OfferDog);
        annualFunctions.Add(AnnualFunction_AgeDogs);
    }

    /** \fn DailyEvents
    *   \brief Calls all the functions in the dailyFunctions list. Called when a day passes in game time.
    */
    private void DailyEvents() { if (dailyFunctions.Count > 0) { foreach (TimeFunction function in dailyFunctions) function(); } }
    /** \fn WeeklyEvents
    *   \brief Calls all the functions in the weeklyFunctions list. Called when a week passes in game time.
    */
    private void WeeklyEvents() { if (weeklyFunctions.Count > 0) { foreach (TimeFunction function in weeklyFunctions) function(); } }
    /** \fn BiannualEvents
     *   \brief Calls all the functions in the biannualFunctions list. Called when half a year passes in game time.
     */
    private void BiannualEvents() { if (biannualFunctions.Count > 0) { foreach (TimeFunction function in biannualFunctions) function(); } }
    /** \fn AnnualEvents
     *   \brief Calls all the functions in the annualFunctions list. Called when a year passes in game time.
     */
    private void AnnualEvents() { if (annualFunctions.Count > 0) { foreach (TimeFunction function in annualFunctions) function(); } }

    /** \fn DailyFunction_PayPlayer
    *   \brief A daily function which pays the player their dailt allowance.
    */
    private void DailyFunction_PayPlayer() { controller.GiveAllowance(); }
    /** \fn WeeklyFunction_OfferDog
    *   \brief A weekly function which generates a new dog.
    */
    private void WeeklyFunction_OfferDog()
    {
        if (controller.NumberOfDogs() < controller.dogLimit) { dogGenerator.GenerateRandomNewDog(); }
        else { controller.tipPopUp.DisplayTipMessage("No new dog this week. You're already caring for the maximum number of dogs."); }
    }
    /** \fn AnnualFunction_AgeDogs
    *   \brief A annual function which ages all the dogs by 1 year.
    */
    private void AnnualFunction_AgeDogs() { controller.AgeDogs(); }
}