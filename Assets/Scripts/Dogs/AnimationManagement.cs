using UnityEngine;

public class AnimationManagement : MonoBehaviour
{
    [SerializeField] private Dog dog;

    public void NeedsToFinishAnimation() { dog.m_needsToFinishAnim = true; }
    public void FinishedAnimation() { dog.m_needsToFinishAnim = false; }
}
