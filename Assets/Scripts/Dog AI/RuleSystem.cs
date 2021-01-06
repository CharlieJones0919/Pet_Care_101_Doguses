using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogFacts : MonoBehaviour
{


    //Facts about the dog and their current states.
    public Dictionary<string, float> m_currentPropertyStates = new Dictionary<string, float>(); 
    //public List<Rule> m_rules = new List<Rule>();

    public DogFacts(Dictionary<string, float> factList)
    {
        //foreach (KeyValuePair<string, float> fact in factList)
        //{
        //    switch (fact.Key)
        //    { 
        //        case ("Attention (%)"):
        //            m_hunger = fact.Value;
        //            break;
        //        case ("Hunger (%)"):
        //            m_attention = fact.Value;
        //            break;
        //        case ("Rest (%)"):
        //            m_attention = fact.Value;
        //            break;
        //        case ("Hygiene (%)"):
        //            m_attention = fact.Value;
        //            break;
        //    }
        //}

    }

    // Start is called before the first frame update
    void Start()
    {
        m_currentPropertyStates.Add("hunger", 500.0f);
        m_currentPropertyStates.Add("hunger", 500.0f);


        ///These are set to true on entry to their respective states and set to false on exit. They allow for the rules to determine which state is currently active so they can determine which state to swap to when needed.
        //Hunger States [Input]
        //m_currentPropertyStates.Add("starving", 500.0f);
        //m_currentPropertyStates.Add("fed", 500.0f);
        //m_currentPropertyStates.Add("overfed", 500.0f);
        ////Attention States [Input]
        //m_currentPropertyStates.Add("lonely", 500.0f);
        //m_currentPropertyStates.Add("loved", 500.0f);
        //m_currentPropertyStates.Add("crowded", 500.0f);
        ////Rest States [Input]
        //m_currentPropertyStates.Add("exhausted", 500.0f);
        //m_currentPropertyStates.Add("tired", 500.0f);
        //m_currentPropertyStates.Add("rested", 500.0f);
        ////Hygiene States [Input]
        //m_currentPropertyStates.Add("filthy", 500.0f);
        //m_currentPropertyStates.Add("dirty", 500.0f);
        //m_currentPropertyStates.Add("clean", 500.0f);

        ////Bond States [Output]
        //m_currentPropertyStates.Add("wary", 500.0f);
        //m_currentPropertyStates.Add("friendly", 500.0f);
        ////Happiness States [Output]
        //m_currentPropertyStates.Add("distressed", 500.0f);
        //m_currentPropertyStates.Add("upset", 500.0f);
        //m_currentPropertyStates.Add("happy", 500.0f);
        ////Health States [Output]
        //m_currentPropertyStates.Add("dying", 500.0f);
        //m_currentPropertyStates.Add("sick", 500.0f);
        //m_currentPropertyStates.Add("good", 500.0f);

        ////Tolerance States [Input]
        //m_obedienceStates.Add("nervous", 500.0f);
        //m_obedienceStates.Add("neutral", 500.0f);
        //m_obedienceStates.Add("calm", 500.0f);
        ////Affection States [Input]
        //m_obedienceStates.Add("aggressive", 500.0f);
        //m_obedienceStates.Add("grouchy", 500.0f);
        //m_obedienceStates.Add("apathetic", 500.0f);
        //m_obedienceStates.Add("friendly", 500.0f);
        //m_obedienceStates.Add("loving", 500.0f);
        ////Intelligence States [Input]
        //m_obedienceStates.Add("dumb", 500.0f);
        //m_obedienceStates.Add("average", 500.0f);
        //m_obedienceStates.Add("smart", 500.0f);
        ////Energy States [Input]
        //m_obedienceStates.Add("sleepy", 500.0f);
        //m_obedienceStates.Add("normal", 500.0f);
        //m_obedienceStates.Add("hyper", 500.0f);

        ////Obedience States [Output]
        //m_obedienceStates.Add("bad", 500.0f);
        //m_obedienceStates.Add("good", 500.0f);

        // m_rules.Add(new Rule(""));
    }
}
