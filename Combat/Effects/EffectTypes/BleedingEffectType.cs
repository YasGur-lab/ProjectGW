using UnityEngine;

[CreateAssetMenu(fileName = "BleedingEffectType", menuName = "Effects/Effect Types/Bleeding Effect", order = 3)]
public class BleedingEffectType : EffectType
{
    [SerializeField] private int m_BleedingTick = -3;

    public int BleedingTick => m_BleedingTick;
    public int SetBleedingTick(int BleedingTick) => m_BleedingTick = BleedingTick;
}