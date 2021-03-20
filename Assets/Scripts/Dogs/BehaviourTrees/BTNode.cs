/** \file BTNode.cs */

/** \class BTNode
*  \brief An abstract class for a behaviour tree "node" (step in a behaviour tree sequence). 
*  Has a state of success and a definition for evaluating this state which implementations of this class should have a defining function override for.
*/
public abstract class BTNode
{
    //! The current state of the node.
    protected BTState state;

    //! Returns the node's current state.
    public BTState BTNodeState
    {
        get { return state; }
    }

    //! Evavluate the state's set of conditions.
    public abstract BTState Evaluate();
}