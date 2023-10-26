using System.Collections.Generic;
using GW.Attributes;
using GW.Statistics;
using UnityEngine;

[CreateAssetMenu(fileName = "HealOther", menuName = "Skill/Monk/HealOther", order = 1)]
public class HealOther: Skill
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
        }
    }
}
