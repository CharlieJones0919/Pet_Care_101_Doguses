/** \file AnimationManagement.cs */
using UnityEngine;

/** \class AnimationManagement
*  \brief A class for the dogs' animation controller's events. (Only currently has 2 functions). These are functions called at specified keyframes in some of the dog's animations.
*/
public class AnimationManagement : MonoBehaviour
{
    [SerializeField] private Dog dog; //!< A reference to the dog this animator is attached to. Required to allow the animations to influence the dog's behaviour.

    /** \fn NeedsToFinishAnimation
    *  \brief Sets the dog's "NEEDS_2_FINISH_ANIM" fact as true. The dog can't exit the pause state until this is unset back to false. Used so the dog stops all behaviours until finishing specific animations. (E.g. Pauses while finishing the waking up animation).
    */
    public void NeedsToFinishAnimation() { dog.m_facts["NEEDS_2_FINISH_ANIM"] = true; }
    /** \fn FinishedAnimation
    *  \brief Resets dog's "NEEDS_2_FINISH_ANIM" fact back to false after NeedsToFinishAnimation() is called. Allows the dog to unpause after the animation is done.
    */
    public void FinishedAnimation() { dog.m_facts["NEEDS_2_FINISH_ANIM"] = false; }
}
