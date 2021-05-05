using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public GameObject defaultNULL;
    public AStarSearch groundSearch;
    public GameObject randomPointStorage;

    public TipPopUp tipPopUp;
    public DataDisplay UIOutput;     //!< Script from the infoPanelObject.
    [SerializeField] private Text ffButtonText;

    private static float playerMoney = 0;
    [SerializeField] private List<Text> playerMoneyTextboxes = new List<Text>();

    [SerializeField] private Dictionary<GameObject, Dog> allDogs = new Dictionary<GameObject, Dog>();
    [SerializeField] private int dogLimit;
    private const int maxGameSpeed = 10;

    private Dictionary<ItemType, List<Item>> itemPools = new Dictionary<ItemType, List<Item>>();
    public List<Vector3> permanentItemPositions = new List<Vector3>();
    public Dictionary<Vector3, bool> tempItemPositions = new Dictionary<Vector3, bool>();

    //[SerializeField] private bool isPetting = false;

    //public bool IsPetting() { return isPetting; }
    //public void SetPetting() { isPetting = !isPetting; }

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

    public void AddDog(Dog newDog)
    {
        allDogs.Add(newDog.gameObject, newDog);
        newDog.SetController(this);

        UIOutput.ActivateNewDogPanel();
        UIOutput.SetFocusDog(newDog);
        PAUSE(true);
    }

    public void AddToItemPools(ItemType itemGroup, Item item)
    {
        if (!itemPools.ContainsKey(itemGroup))
        {
            itemPools.Add(itemGroup, new List<Item>());
        }
        itemPools[itemGroup].Add(item);
    }

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

    public void EndItemUse(Item item, GameObject instance)
    {
        if (item.IsSingleUse())
        {
            Vector3 instanceSpawnPos = item.GetInstanceSpawnPos(instance);
            if (tempItemPositions.ContainsKey(instanceSpawnPos))
            {
                tempItemPositions[instanceSpawnPos] = false;
            }
        }
        item.StopUsingItemInstance(instance);
    }

    public void AgeDogs()
    {
        if (allDogs.Count > 0) {   foreach (Dog K9 in allDogs.Values) { K9.m_age++; }  }
    }

    public static float GetPlayerMoney() { return playerMoney; }

    public void GiveAllowance(float amount) { UpdateMoneyValue(amount); }
    public bool HasEnoughMoney(float cost) { return (cost <= playerMoney); }

    public bool MakePurchase(float cost)
    {
        if (HasEnoughMoney(cost))
        {
            UpdateMoneyValue(-cost);
            return true;
        }

        return false;
    }

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

    public void InsufficientFundsCheck()
    {
        if (playerMoney == 0) { tipPopUp.DisplayTipMessage("You don't have enough funds to buy anything right now. You'll get another donation tomorrow."); }
    }

    public void NotFocusedOnDog() { foreach (Dog dog in allDogs.Values) { dog.m_isFocusDog = false; } }
}

public class CareProperty
{
    private Dictionary<string, Vector2> m_states = new Dictionary<string, Vector2>();
    private List<string> m_currentStates = new List<string>();
    private float m_value = 50;
    private float m_decrement;

    public CareProperty(Dictionary<string, Vector2> states, float defaultDec)
    {
        m_states = states;
        m_decrement = defaultDec;
        UpdateValue(0.0f);
    }

    public float GetValue() { return m_value; }
    public float GetUsualDecrement() { return m_decrement; }
    public bool IsState(string state)
    {
        if (m_states.ContainsKey(state)) { return m_currentStates.Contains(state); }
        else { Debug.Log("No care property has a state defined as " + state); return false; }
    }

    public void UpdateValue(float increment)
    {
        m_value = Mathf.Clamp(m_value + increment, 0.0f, 100.0f);

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

public class PersonalityProperty
{
    private Dictionary<string, Vector2> m_states = new Dictionary<string, Vector2>();
    private string m_currentState;
    private float m_value;

    public PersonalityProperty(Dictionary<string, Vector2> states, float value)
    {
        m_states = states;
        UpdateValue(value);
    }

    public float GetValue() { return m_value; }
    public string GetState() { return m_currentState; }
    public bool IsState(string state)
    {
        if (m_states.ContainsKey(state)) { return (m_currentState == state); }
        else { Debug.Log("No personality property has a state defined as " + state); return false; }
    }

    public void UpdateValue(float increment)
    {
        m_value = Mathf.Clamp(m_value + increment, 0.0f, 5.0f);

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