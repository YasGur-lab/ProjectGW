using GW.Attributes;
using System.Collections;
using System.Collections.Generic;
using GW.Statistics;
using Unity.VisualScripting;
using UnityEngine;
using System.Text.RegularExpressions;

[CreateAssetMenu(fileName = "SeverArtery", menuName = "Skill/Warrior/SeverArtery", order = 2)]
public class SeverArtery : Skill
{
    public override void Activate(GameObject player, float damage, EffectType[] effects, List<string> mask, Vector3 pos = default,
        Health health = null)
    {
        if (health == null) return;
        int duration = (int)player.GetComponent<BaseStats>().GetSkillBaseDamage(m_Skill, m_Attribute, Stat.Duration);

        foreach (var effectType in effects)
        {
            if (effectType is BleedingEffectType)
            {
                BleedingEffectType newEffectType = new BleedingEffectType();
                health.GetComponent<EffectComponent>().ApplyEffect(newEffectType, duration, this);
            }
        }
    }
}