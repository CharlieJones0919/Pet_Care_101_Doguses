using UnityEngine;

public class AnimationManagement : MonoBehaviour
{
    [SerializeField] private Dog dog;

    public void NeedsToFinishAnimation() { dog.m_needsToFinishAnim = true; Debug.Log("AAA");}
    public void FinishedAnimation() { dog.m_needsToFinishAnim = false; Debug.Log("AAA"); }
}
