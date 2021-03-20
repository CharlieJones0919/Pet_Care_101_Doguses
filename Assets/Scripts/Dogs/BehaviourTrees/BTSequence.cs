/** \file BTSequence.cs */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** \class BTSequence
*  \brief A sequenced list of behaviour tree nodes of which all much be evaluated as successful for the sequence to be.
*/
public class BTSequence : BTNode
{
    //! List of this sequence's nodes.
    protected List<BTNode> btNodes = new List<BTNode>();

    //! A constructor which defines this sequence's nodes.  
    public BTSequence(List<BTNode> btNodes)
    {
        this.btNodes = btNodes;
    }

    //! An evaluation function implementation to define that if any action node fails, the sequence fails. 
    public override BTState Evaluate()
    {
        bool failed = false;
        foreach (BTNode btNode in btNodes)
        {
            if (failed == true)
            {
                break;
            }

            switch (btNode.Evaluate())
            {
                case BTState.FAILURE:
                    state = BTState.FAILURE;
                    failed = true;
                    return state;
                case BTState.SUCCESS:
                    state = BTState.SUCCESS;
                    continue;
                default:
                    state = BTState.FAILURE;
                    return state;
            }
        }
        return state;
    }
}
