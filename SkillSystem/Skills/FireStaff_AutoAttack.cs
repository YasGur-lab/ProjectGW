
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using UnityEngine;

[CreateAssetMenu(fileName = "FireStaff_AutoAttack", menuName = "Skill/Mage/FireStaff_AutoAttack", order = 4)]
public class FireStaff_AutoAttack : Skill
{
    [SerializeField] private GameObject m_StaffProjectile;

    public override void Activate(GameObject player, float damage, EffectType[] effects, List<string> tags, Vector3 pos = default,
        Health health = null)
    {
        Fighter fighter = player.GetComponent<Fighter>();
        if (health)
        {
            GameObject projectileInstance = Instantiate(m_StaffProjectile, fighter.GetLeftHandTransform().position, Quaternion.identity);
            projectileInstance.GetComponent<Projectile>().Init(player, damage, tags, health);
            projectileInstance.GetComponent<Projectile>().SetLookAt();
        }
        else
        {
            GameObject projectileInstance = Instantiate(m_StaffProjectile, fighter.GetLeftHandTransform().position, Quaternion.identity);
            projectileInstance.GetComponent<Projectile>().Init(player, damage, tags);
            pos.y += 1.0f;
            projectileInstance.GetComponent<Projectile>().SetLookAt(fighter.GetLeftHandTransform().position + fighter.transform.forward);
        }
    }
}
