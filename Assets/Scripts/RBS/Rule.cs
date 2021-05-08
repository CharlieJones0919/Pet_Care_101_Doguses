/** \file Rule.cs
*  \brief Contains the class representation of a "rule."
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/** \class Rule
*  \brief A class representation of a "rule" consisting of 2 facts or atecendents which are compared to result in a rule state depending on the rule's condition.
*/
public class Rule 
{
    public string m_atecedentA;    //!< First fact of the rule.
    public string m_atecedentB;    //!< Second fact of the rule.

    public Type m_consequentState; //!< State which the rule will be in if the rule's condition is evaluated as true.
    public Predicate m_compare;    //!< Comparison to be made between the 2 facts.
    public enum Predicate { And, Or, nAnd }; //!< Comparisons which can be made between the facts.

    /** \fn Rule
    *  \brief Constructor to allow the rule's facts, comparison and consequence state from that comparison to be set.
    *  \param atecedentA What to set this rule's first fact as.
    *  \param atecedentB What to set this rule's second fact as.
    *  \param compare What to set this rule's comparison type as.
    *  \param consequentState What to set this rule's consequence type as if its comparison is returned as true.
    */
    public Rule(string atecedentA, string atecedentB, Predicate compare, Type consequentState)  
    {
        m_atecedentA = atecedentA;
        m_atecedentB = atecedentB;
        m_compare = compare;
        m_consequentState = consequentState;
    }

    /** \fn CheckRule
    *  \brief A function to check if the rule's conditions/comparisons are true. 
    *  \param stats The current states of the facts.
    */
    public Type CheckRule(Dictionary<string, bool> stats)   
    {
        //Set specified facts to given boolean states.
        bool atecedentABool = stats[m_atecedentA];
        bool atecedentBBool = stats[m_atecedentB];

        //Defines how the specified comparison types are evaluated.
        switch (m_compare)
        {
            case (Predicate.And):
                if (atecedentABool && atecedentBBool) return m_consequentState;
                else return null;
            case (Predicate.Or):
                if (atecedentABool || atecedentBBool) return m_consequentState;
                else return null;
            case (Predicate.nAnd):
                if (!atecedentABool && !atecedentBBool) return m_consequentState;
                else return null;
            default:
                return null;
        }
    }
}