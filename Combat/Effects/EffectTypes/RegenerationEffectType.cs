using UnityEngine;

[CreateAssetMenu(fileName = "RegenerationEffectType", menuName = "Effects/Effect Types/Regeneration Effect", order = 1)]
public class RegenerationEffectType : EffectType
{
    [SerializeField] private int m_RenegerationTick = 3;

    public int RenegerationTick => m_RenegerationTick;
    public int SetRegenerationTick(int regenerationTick) => m_RenegerationTick = regenerationTick;
}