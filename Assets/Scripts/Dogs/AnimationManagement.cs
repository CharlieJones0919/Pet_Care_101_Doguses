using UnityEngine;

public class AnimationManagement : MonoBehaviour
{
    [SerializeField] private Dog dog;

    public void NeedsToFinishAnimation() { dog.m_facts["NEEDS_2_FINISH_ANIM"] = true; }
    public void FinishedAnimation() { dog.m_facts["NEEDS_2_FINISH_ANIM"] = false; }
}
