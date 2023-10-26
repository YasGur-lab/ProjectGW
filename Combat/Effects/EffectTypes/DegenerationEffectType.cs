using UnityEngine;

[CreateAssetMenu(fileName = "DegenerationEffectType", menuName = "Effects/Effect Types/Degeneration Effect", order = 2)]
public class DegenerationEffectType : EffectType
{
    [SerializeField] private int m_DegenerationTick = -3;

    public int DenegerationTick => m_DegenerationTick;
    public int SetDegenerationTick(int degenerationTick) => m_DegenerationTick = degenerationTick;
}