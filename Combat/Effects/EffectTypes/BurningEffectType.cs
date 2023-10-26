using UnityEngine;

[CreateAssetMenu(fileName = "BurningEffectType", menuName = "Effects/Effect Types/Burning Effect", order = 4)]
public class BurningEffectType : EffectType
{
    [SerializeField] private int m_BurningTick = -7;

    public int BurningTick => m_BurningTick;
    public int SetBurningTick(int burningTick) => m_BurningTick = burningTick;
}