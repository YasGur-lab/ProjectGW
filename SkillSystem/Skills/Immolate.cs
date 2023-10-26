using System.Collections.Generic;
using System.Text.RegularExpressions;
using GW.Attributes;
using GW.Combat;
using GW.Statistics;
using UnityEngine;

[CreateAssetMenu(fileName = "Immolate", menuName = "Skill/Mage/Immolate", order = 5)]
public class Immolate : Skill
{
    public override void Activate(GameObject player, float damage, EffectType[] effects, List<string> tags, Vector3 pos = default,
        Health health = null)
    {
        if (health == null) return;
        int duration = (int)player.GetComponent<BaseStats>().GetSkillBaseDamage(m_Skill, m_Attribute, Stat.Duration);
        float dmg = player.GetComponent<BaseStats>().GetSkillBaseDamage(m_Skill, m_Attribute, Stat.Damage);
        
        health.TakeDamage(player, dmg);

        foreach (var effectType in effects)
        {
            if (effectType is BurningEffectType)
            {
                BurningEffectType newEffectType = new BurningEffectType();
                health.GetComponent<EffectComponent>().ApplyEffect(newEffectType, duration, this);
            }
        }
    }
}
