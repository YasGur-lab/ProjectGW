using UnityEngine;

[CreateAssetMenu(fileName = "SpeedEffectType", menuName = "Effects/Effect Types/Speed Effect", order = 1)]
public class SpeedEffectType : EffectType
{
    [Tooltip("Multiplier for the movement speed")]
    [SerializeField] private float speedMultiplier = 1.5f;

    public float SpeedMultiplier => speedMultiplier;
}