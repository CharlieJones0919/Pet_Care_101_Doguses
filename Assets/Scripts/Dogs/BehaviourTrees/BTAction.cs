/** \file BTAction.cs */

/** \class BTAction
*  \brief This is a class which is designated a function which can be called as an actionable step in a behaviour tree sequence.
*/
public class BTAction : BTNode
{
    //! Stores the function signature for the action.
    public delegate BTState ActionNodeFunction();

    //! Stores the action node function for evaluation.
    private ActionNodeFunction btAction;

    //! Class constructor which sets the behaviour tree action to a function.
    public BTAction(ActionNodeFunction btAction)
    {
        this.btAction = btAction;
    }

    //! Defines if the action node has failed or not.
    public override BTState Evaluate()
    {
        switch (btAction())
        {
            case BTState.SUCCESS:
                state = BTState.SUCCESS;
                return state;
            case BTState.FAILURE:
                state = BTState.FAILURE;
                return state;
            default:
                state = BTState.FAILURE;
                return state;
        }
    }
}