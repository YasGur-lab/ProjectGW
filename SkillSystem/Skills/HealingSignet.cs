using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using GW.Statistics;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "HealingSignet", menuName = "Skill/Warrior/HealingSignet", order = 0)]
public class HealingSignet : Skill
{
    public override void Activate(GameObject player, float amount, EffectType[] effects, List<string> mask, Vector3 pos = default,
        Health health = null)
    {
        int healing = (int)player.GetComponent<BaseStats>().GetSkillBaseDamage(m_Skill, m_Attribute, Stat.Health);
        player.GetComponent<Health>().AddHealth(healing);

        //foreach (var effectType in effects)
        //{
        //    EffectType newEffectType = effectType;
        //    player.GetComponent<EffectComponent>().ApplyEffect(newEffectType, GetActiveTime(), this);
        //}
    }
}
