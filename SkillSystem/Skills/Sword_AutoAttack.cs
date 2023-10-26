using GW.Attributes;
using System.Collections;
using System.Collections.Generic;
using GW.Statistics;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Sword_AutoAttack", menuName = "Skill/Warrior/Sword_AutoAttack", order = 3)]
public class Sword_AutoAttack : Skill
{
    public override void Activate(GameObject player, float damage, EffectType[] effects, List<string> mask, Vector3 pos = default,
        Health health = null)
    {
        if (health == null) return;
        float dmg = player.GetComponent<BaseStats>().GetSkillBaseDamage(m_Skill, m_Attribute,Stat.Damage);

        health.TakeDamage(player, dmg);

        //foreach (var effectType in effects)
        //{
        //    EffectType newEffectType = effectType;
        //    if (newEffectType is BleedingEffectType bleedingEffectType)
        //    {
        //        health.GetComponent<EffectComponent>().ApplyEffect(bleedingEffectType, duration, this);
        //        //GameObject go = Instantiate(m_HealingEffect, player.transform);
        //    }
        //}
    }
}