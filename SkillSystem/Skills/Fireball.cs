using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using UnityEngine;

[CreateAssetMenu(fileName = "Fireball", menuName = "Skill/Mage/Fireball", order = 0)]
public class Fireball : Skill
{
    [SerializeField] private GameObject m_Fireball;

    public override void Activate(GameObject player, float damage, EffectType[] effects, List<string> tags, Vector3 pos = default,
        Health health = null)
    {
        Fighter fighter = player.GetComponent<Fighter>();
        Vector3 handPos = fighter.GetLeftHandTransform().position;
        if (health)
        {
            GameObject projectileInstance = Instantiate(m_Fireball, handPos, Quaternion.identity);
            projectileInstance.GetComponent<Projectile>().Init(player, damage, tags, health);
            projectileInstance.GetComponent<Projectile>().SetLookAt();
        }
        else
        {
            GameObject projectileInstance = Instantiate(m_Fireball, handPos, Quaternion.identity);
            projectileInstance.GetComponent<Projectile>().Init(player, damage, tags);
            pos.y += 1.0f;
            projectileInstance.GetComponent<Projectile>().SetLookAt(handPos + fighter.transform.forward);
        }
    }
}
