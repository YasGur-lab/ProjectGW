using System.Collections.Generic;
using GW.Attributes;
using GW.Statistics;
using UnityEngine;

[CreateAssetMenu(fileName = "TrollUnguent", menuName = "Skill/Archer/TrollUnguent", order = 0)]
public class TrollUnguent: Skill
{
    public override void Activate(GameObject player, float damage, EffectType[] effects, List<string> mask, Vector3 pos = default,
        Health health = null)
    {
        int regen = (int)player.GetComponent<BaseStats>().GetSkillBaseDamage(m_Skill, m_Attribute, Stat.HealthRegeneration);

        foreach (var effectType in effects)
        {
            if (effectType is RegenerationEffectType)
            {
                RegenerationEffectType newEffectType = new RegenerationEffectType();
                newEffectType.SetRegenerationTick(regen);
                player.GetComponent<EffectComponent>().ApplyEffect(newEffectType, GetActiveTime(), this);
            }
        }
    }
}
