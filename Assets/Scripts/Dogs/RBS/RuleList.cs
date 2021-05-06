using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** \class Rules
*  \brief Allows for the creation of new rules which are added to a subsequent list of rules for ease of checking.
*/
public class RuleList
{
    private List<Rule> rules { get; } = new List<Rule>();
    public void AddRule(Rule rule)
    {
        rules.Add(rule);
    }

    public List<Rule> GetRules() { return rules; }
}