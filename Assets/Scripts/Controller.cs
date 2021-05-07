﻿/** \file Controller.cs
*  \brief This is a script every dog instance has a reference to and acts as a hub point for core game values and misc functions.
*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** \class Controller
*  \brief Keeps references to widely used objects and scripts for other classes to use. Allows for game time controls.
*/
public class Controller : MonoBehaviour
{
    public GameObject defaultNULL;              //!< An empty game object for when game objects shouldn't be set to any actual reference anymore, but unity throws errors if game objects are set to null during runtime.
    public AStarSearch groundSearch;            //!< The ground's A-Star search script for the dog's to use for navigation and for any other class to use for point generation/finding on the map's floor.
    public GameObject randomPointStorage;       //!< A game object for the dog's pathfinding random points to be made children of. (Just for hierarchy organisation).

    public TipPopUp tipPopUp;                   //!< Reference to the TipPopUp class for classes to output messages to the player via. 
    public DataDisplay UIOutput;                //!< Script from the infoPanelObject.
    [SerializeField] private Text ffButtonText; //!< Reference to the number text on the fast forward button to display the current time multiplier to.

    [SerializeField] private float playerMoney = 0; //!< The player's in-game currency value.
    private const float allowance = 50;             //!< How much money the player is given daily. (In-game days).
    private const float bonusPay = 0.05f;           //!< How much money the player is given for as long as a dog has peak care conditions.
    [SerializeField] private List<Text> playerMoneyTextboxes = new List<Text>(); //!< Textboxes the player's money value is outputted to.

    [SerializeField] private Dictionary<GameObject, Dog> allDogs = new Dictionary<GameObject, Dog>(); //!< A dictionary of all the player's dogs with their GameObjects as their key for ease of reference.
    public int dogLimit;                        //!< Maximum number of allowed dogs.
    private const int maxGameSpeed = 10;        //!< Maximum fast forward speed.

    private Dictionary<ItemType, List<Item>> itemPools = new Dictionary<ItemType, List<Item>>(); //!< All the items (active or not) generated by the store controller.
    public List<Vector3> bedItemPositions = new List<Vector3>(); //!< Positions beds can be placed to.
    public Dictionary<Vector3, bool> foodItemPositions = new Dictionary<Vector3, bool>(); //!< Positions temporary food items can be placed to.

    /** \fn PAUSE
    *  \brief Sets the game's time scale to pause or play the game based on its bool parameter.
    *  \param state Whether to pause or play the game's time.
    */
    public void PAUSE(bool state)
    {
        switch (state)
        {
            case (true):
                Time.timeScale = 0; 
                break;
            case (false):
                Time.timeScale = 1; 
                break;
        }
        ffButtonText.text = "x" + Time.timeScale.ToString();
    }

    /** \fn FASTFORWARD
    *  \brief Increases the game's time scale to speed up the game to it's maximum value. Called by the fast forward button.
    */
    public void FASTFORWARD()
    {
        switch (Time.timeScale)
        {
            case (0):
            case (maxGameSpeed):
                PAUSE(false);
                break;
            default:
                if (Time.timeScale < maxGameSpeed)
                {
                    Time.timeScale++;
                }
                break;
        }
        ffButtonText.text = "x" + Time.timeScale.ToString();
    }

    /** \fn AddDog
    *  \brief Called by DogGeneration when a new dog is made daily to add the new instance to the controller's list of dogs, and to activate the new dog screen panel.
    *  \param newDog The new dog instance created by DogGeneration.
    */
    public void AddDog(Dog newDog)
    {
        allDogs.Add(newDog.gameObject, newDog);
        newDog.SetController(this);

        UIOutput.ActivateNewDogPanel();
        UIOutput.SetFocusDog(newDog);
        PAUSE(true);
    }

    /** \fn NumberOfDogs
    *  \brief Returns the number of active dogs.
    */
    public int NumberOfDogs() { return allDogs.Count; }

    /** \fn GetAllDogs
    *  \brief Returns the Dictionary of all world dogs.
    */
    public Dictionary<GameObject, Dog> GetAllDogs() { return allDogs; }

    /** \fn AddToItemPools
    *  \brief Used by the StoreController to add a new item to the Controller's complete list of items possible to purchase.
    */
    public void AddToItemPools(ItemType itemGroup, Item item)
    {
        if (!itemPools.ContainsKey(itemGroup))
        {
            itemPools.Add(itemGroup, new List<Item>());
        }
        itemPools[itemGroup].Add(item);
    }

    /** \fn GetActiveItemFor
    *  \brief Returns whether or not there is an item of the requested type available for use of the given dog in the item pool. If it is, the item itself will set the dog's current target to the available instance via the reference passed in as a parameter.
    */
    public bool GetActiveItemFor(ItemType requiredType, Dog attemptingDog)
    {
        if (itemPools[requiredType].Count > 0)
        {
            foreach (Item item in itemPools[requiredType])
            {
                if (item.TryGetAvailableInstance(attemptingDog)) { return true; }
            }
        }
        return false;
    }

    /** \fn GetClosestActiveItemFor
    *  \brief The same as GetActiveItemFor(), but tries to identify the closest item instance to the given dog.
    */
    public bool GetClosestActiveItemFor(ItemType requiredType, Dog attemptingDog)
    {
        if (itemPools[requiredType].Count > 0)
        {
            foreach (Item item in itemPools[requiredType])
            {
                if (item.TryGetClosestAvailableInstance(attemptingDog)) { return true; }
            }
        } 
        return false;
    }

    /** \fn EndItemUse
    *  \brief Calls the item's end use script and if it was a temporary item which will be deactivated from the world, the position it was using will be made free again by setting its dictionary value back to false.
    */
    public void EndItemUse(Item item, GameObject instance)
    {
        if (item.IsSingleUse())
        {
            Vector3 instanceSpawnPos = item.GetInstanceSpawnPos(instance);
            if (foodItemPositions.ContainsKey(instanceSpawnPos))
            {
                foodItemPositions[instanceSpawnPos] = false;
            }
        }
        item.StopUsingItemInstance(instance);
    }

    /** \fn AgeDogs
    *  \brief Called every year in game time to increase the age of all the dogs. (Doesn't really do anything at current as dog death didn't get implemented due to time constraints).
    */
    public void AgeDogs()
    {
        if (allDogs.Count > 0) {   foreach (Dog K9 in allDogs.Values) { K9.m_age++; }  }
    }

    /** \fn GetPlayerMoney
    *  \brief Returns the player's money value indirectly via a float. (To prevent other classes from changing the real variable's value).
    */
    public float GetPlayerMoney() { float moneyVal = playerMoney; return moneyVal; }

    /** \fn GiveAllowance
    *  \brief Increases the player's money by the set daily allowance value.
    */
    public void GiveAllowance() { UpdateMoneyValue(allowance); }
    /** \fn GiveGoodCareBonus
    *  \brief Bonus pay given by a Dog class if all of their care values are in good states.
    */
    public void GiveGoodCareBonus() { UpdateMoneyValue(bonusPay * Time.deltaTime); }
    /** \fn HasEnoughMoney
    *  \brief For the store controller to check if the player has enough money to pay for an item.
    */
    public bool HasEnoughMoney(float cost) { return (cost <= playerMoney); }

    /** \fn HasEnoughMoney
    *  \brief For the store controller to pay for an item. (Subtracts the item's price from the player's money).
    */
    public bool MakePurchase(float cost)
    {
        if (HasEnoughMoney(cost))
        {
            UpdateMoneyValue(-cost);
            return true;
        }

        return false;
    }

    /** \fn UpdateMoneyValue
    *  \brief Updates the player's money value and the related UI textboxes.
    */
    private void UpdateMoneyValue(float modification)
    {
        playerMoney += modification; 

        if (playerMoneyTextboxes.Count > 0)
        {
            foreach (Text textbox in playerMoneyTextboxes)
            {
                textbox.text = string.Format("{0:F2}", playerMoney);
            }
        }
    }

    /** \fn InsufficientFundsCheck
    *  \brief Displays a tip message if the player is out of money.
    */
    public void InsufficientFundsCheck()
    {
        if (playerMoney == 0) { tipPopUp.DisplayTipMessage("You don't have enough funds to buy anything right now. You'll get another donation tomorrow."); }
    }

    /** \fn NotFocusedOnDog
    *  \brief Sets all dogs to not be in focus if the DogInfoPanel is closed.
    */
    public void NotFocusedOnDog() { foreach (Dog dog in allDogs.Values) { dog.m_facts["IS_FOCUS"] = false; } }
}

/** \class CareProperty
 *  \brief Stores all the dog's "CareProperties" as defined in DogGeneration (e.g. Hunger, Rest, etc...), the care value's associated "states" (e.g. Exhausted, Tired, Rested, etc...), 
 *  and its current actual numerical value. Also contains a function for updating the care value and sets the care property's subsequent current state from that new value based on the state's lower and upper qualification bounds. 
 */
public class CareProperty
{
    private Dictionary<string, Vector2> m_states = new Dictionary<string, Vector2>(); //!< This care property's possible states (e.g. Hunger has the states of Starving, Hungry, Fed and Overfed), and the Vector2 is each state's lower and upper bounds.
    private List<string> m_currentStates = new List<string>(); //!< Current states based on the current numerical value of the property. Care properties have a list of multiple current states instead of just one, because the bounds of some overlap (e.g. if the dog is "Exhausted" it'll also still be "Tired").
    private float m_value = 100; //!< The care property's current value. Set to 100 by default so the dog is in good health when first instantiated.
    private float m_decrement;   //!< Care properties are always going down and need replenishing. This is the default decrement to take from the value on each update.

    /** \fn CareProperty
    *  \brief Constructor to instantiate the care property's states, their bounds, and default decrement.
    */
    public CareProperty(Dictionary<string, Vector2> states, float defaultDec)
    {
        m_states = states;
        m_decrement = defaultDec;
        UpdateValue(0.0f);
    }

    /** \fn IsState
    *  \brief Returns whether or not the property is currently in the given parameter state.
    */
    public bool IsState(string state)
    {
        if (m_states.ContainsKey(state)) { return m_currentStates.Contains(state); }
        else { Debug.Log("No care property has a state defined as " + state); return false; }
    }

    /** \fn GetUsualDecrement
    *  \brief Returns the property's usual decrement.
    */
    public float GetUsualDecrement() { return m_decrement; }

    /** \fn GetValue
    *  \brief Returns the property's current value.
    */
    public float GetValue() { return m_value; }

    /** \fn UpdateValue
    *  \brief Updates the property's value and checks which state[s] it's in now from that change.
    */
    public void UpdateValue(float modifiyer)
    {
        m_value = Mathf.Clamp(m_value + modifiyer, 0.0f, 100.0f);

        m_currentStates.Clear();
        foreach (KeyValuePair<string, Vector2> state in m_states)
        {
            if ((m_value >= state.Value.x) && (m_value <= state.Value.y))
            {
                m_currentStates.Add(state.Key);           
            }
        }
    }
}

/** \fn PersonalityProperty
*  \brief The same as CareProperty except for the personality properties which can only be one state at a time and aren't decreased every update by default.
*/
public class PersonalityProperty
{
    private Dictionary<string, Vector2> m_states = new Dictionary<string, Vector2>(); //!< This personality property's possible states, and the Vector2 is each state's lower and upper bounds.
    private string m_currentState; //!< Current state based on the current numerical value of the property. Personality properties can only be one state at a time, because their state bounds don't overlap.
    private float m_value;         //!< The personality property's current value. This isn't given a default starting value like in CareProperty as the values are determined by the breed.

    /** \fn PersonalityProperty
    *  \brief Constructor to instantiate the personality property's states, their bounds, and default values based on breed.
    */
    public PersonalityProperty(Dictionary<string, Vector2> states, float value)
    {
        m_states = states;
        UpdateValue(value);
    }

    /** \fn IsState
    *  \brief Returns whether or not the property is currently in the given parameter state.
    */
    public bool IsState(string state)
    {
        if (m_states.ContainsKey(state)) { return (m_currentState == state); }
        else { Debug.Log("No personality property has a state defined as " + state); return false; }
    }

    /** \fn GetState
    *  \brief Returns the property's current state.
    */
    public string GetState() { return m_currentState; }

    /** \fn GetValue
    *  \brief Returns the property's current value.
    */
    public float GetValue() { return m_value; }

    /** \fn UpdateValue
    *  \brief Updates the property's value and checks which state it's in now from that change.
    */
    public void UpdateValue(float modifiyer)
    {
        m_value = Mathf.Clamp(m_value + modifiyer, 0.0f, 5.0f);

        foreach (KeyValuePair<string, Vector2> state in m_states)
        {
            if ((m_value >= state.Value.x) && (m_value <= state.Value.y))
            {
                m_currentState = state.Key;
                break;
            }
        }
    }
}