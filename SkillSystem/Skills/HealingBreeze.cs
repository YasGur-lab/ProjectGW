using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using GW.Statistics;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "HealingBreeze", menuName = "Skill/Monk/HealingBreeze", order = 0)]
public class HealingBreeze : Skill
{
    [SerializeField] private GameObject m_HealingEffect;

    public override void Activate(GameObject player, float damage, EffectType[] effects, List<string> tags, Vector3 pos = default,
        Health health = null)
    {
        int regen = (int)player.GetComponent<BaseStats>().GetSkillBaseDamage(m_Skill, m_Attribute, Stat.HealthRegeneration);
        
        if (health && IsTargetInTagList(health.gameObject, tags))
        {
            foreach (var effectType in effects)
            {
                if (effectType is RegenerationEffectType)
                {
                    RegenerationEffectType newEffectType = new RegenerationEffectType();
                    newEffectType.SetRegenerationTick(regen);
                    health.GetComponent<EffectComponent>().ApplyEffect(newEffectType, GetActiveTime(), this);
                    GameObject go = Instantiate(m_HealingEffect, health.transform);
                }
            }
        }
        else
        {
            foreach (var effectType in effects)
            {
                if (effectType is RegenerationEffectType)
                {
                    RegenerationEffectType newEffectType = new RegenerationEffectType();
                    newEffectType.SetRegenerationTick(regen);
                    player.GetComponent<EffectComponent>().ApplyEffect(newEffectType, GetActiveTime(), this);
                    GameObject go = Instantiate(m_HealingEffect, player.transform);
                }
            }
        }
    }
}
