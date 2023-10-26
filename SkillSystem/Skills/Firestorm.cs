using GW.Attributes;
using System.Collections;
using System.Collections.Generic;
using GW.Combat;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Firestorm", menuName = "Skill/Mage/Firestorm", order = 1)]
public class Firestorm : Skill
{
    [SerializeField] private GameObject m_Firestorm;

    public override void Activate(GameObject player, float damage, EffectType[] effects, List<string> tags, Vector3 pos = default,
        Health health = null)
    {
        if (health && !player.CompareTag("Player"))
        {
            GameObject skill = Instantiate(m_Firestorm, health.transform.position, m_Firestorm.transform.rotation);
            skill.GetComponent<AOE_Behavior>().Init(player, damage, tags, health);
        }
        else
        {
            GameObject skill = Instantiate(m_Firestorm, pos, m_Firestorm.transform.rotation);
            skill.GetComponent<AOE_Behavior>().Init(player, damage, tags);
        }
    }
}

