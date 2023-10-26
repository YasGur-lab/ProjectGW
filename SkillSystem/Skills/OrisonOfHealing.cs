using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using GW.Statistics;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "OrisonOfHealing", menuName = "Skill/Monk/OrisonOfHealing", order = 2)]
public class OrisonOfHealing : Skill
{
    [SerializeField] private GameObject m_HealingEffect;

    public override void Activate(GameObject player, float amount, EffectType[] effects, List<string> tags, Vector3 pos = default,
        Health health = null)
    {
        int healing = (int)player.GetComponent<BaseStats>().GetSkillBaseDamage(m_Skill, m_Attribute, Stat.Health);
        if (health && IsTargetInTagList(health.gameObject, tags))
        {
            health.GetComponent<Health>().AddHealth(healing);
            GameObject go = Instantiate(m_HealingEffect, health.transform);
            //DestroyParticleSystem destroy = go.AddComponent<DestroyParticleSystem>();
            //destroy.SetTimer(1.0f);
        }

        else
        {
            player.GetComponent<Health>().AddHealth(healing);
            GameObject go = Instantiate(m_HealingEffect, player.transform);
            //DestroyParticleSystem destroy = go.AddComponent<DestroyParticleSystem>();
            //destroy.SetTimer(1.0f);
        }

        
        //foreach (var effectType in effects)
        //{
        //    EffectType newEffectType = effectType;
        //    player.GetComponent<EffectComponent>().ApplyEffect(newEffectType, GetActiveTime(), this);
        //}
    }
}
