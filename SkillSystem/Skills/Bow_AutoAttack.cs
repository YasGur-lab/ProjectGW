
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using StarterAssets;
using UnityEngine;

[CreateAssetMenu(fileName = "Bow_AutoAttack", menuName = "Skill/Archer/Bow_AutoAttack", order = 2)]
public class Bow_AutoAttack : Skill
{
    [SerializeField] private GameObject m_Arrow;

    public override void Activate(GameObject player, float damage, EffectType[] effects, List<string> tags, Vector3 pos = default,
        Health health = null)
    {
        Fighter fighter = player.GetComponent<Fighter>();
        if (health)
        {
            GameObject projectileInstance = Instantiate(m_Arrow, fighter.GetLeftHandTransform().position, Quaternion.identity);
            projectileInstance.GetComponent<Projectile>().Init(player, damage, tags, health);
            projectileInstance.GetComponent<Projectile>().SetLookAt();
        }
        else
        {
            GameObject projectileInstance = Instantiate(m_Arrow, fighter.GetLeftHandTransform().position, Quaternion.identity);
            projectileInstance.GetComponent<Projectile>().Init(player, damage, tags);
            pos.y += 1.0f;
            projectileInstance.GetComponent<Projectile>().SetLookAt(fighter.GetLeftHandTransform().position + fighter.transform.forward);
        }
    }
}
