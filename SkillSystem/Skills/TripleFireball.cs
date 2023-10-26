using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using UnityEngine;

[CreateAssetMenu(fileName = "TripleFireball", menuName = "Skill/Mage/TripleFireball", order = 2)]
public class TripleFireball : Skill
{
    [SerializeField] private GameObject m_Fireball;
    [SerializeField] private int m_NumFireballs = 3;
    [SerializeField] private float m_SpawnRadius = 1.5f;

    public override void Activate(GameObject player, float damage, EffectType[] effects, List<string> tags, Vector3 pos = default,
        Health health = null)
    {
        Fighter fighter = player.GetComponent<Fighter>();
        Vector3 handPos = fighter.GetLeftHandTransform().position;
        Vector3 direction;
        if (health)
        {
            direction = health.transform.position - player.transform.position;
        }
        else
        {
            direction = GetMouseDirectionFromInstigator(handPos, pos);

        }
        //direction.y = 0.0f;
        direction.Normalize();

        for (int i = 0; i < m_NumFireballs; i++)
        {
            float angle = i * m_SpreadAngle / (m_NumFireballs - 1) - m_SpreadAngle / 2f;

            Quaternion rotationOffset = Quaternion.Euler(0f, angle, 0f);
            Vector3 spawnOffset = Quaternion.LookRotation(direction) * rotationOffset * Vector3.forward * m_SpawnRadius;
            Vector3 spawnPosition = handPos + spawnOffset;
            Quaternion rotation = Quaternion.LookRotation(spawnOffset.normalized, Vector3.up);

            GameObject projectileInstance = Instantiate(m_Fireball, spawnPosition, rotation);
            if (health != null) { projectileInstance.GetComponent<Projectile>().Init(player, damage, tags, health); }
            else
            {
                projectileInstance.GetComponent<Projectile>().Init(player, damage, tags);
            }
        }
    }
}