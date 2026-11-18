using UnityEngine;
[CreateAssetMenu(menuName = "AI/Enemy Animation Set")]
public class EnemyAnimationSet : ScriptableObject
{
    public AnimationClip attackerHold;
    public AnimationClip flankerHold;
    public AnimationClip attackLight;
    public AnimationClip attackHeavy;
}
