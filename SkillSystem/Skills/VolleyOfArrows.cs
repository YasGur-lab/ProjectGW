using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "VolleyOfArrows", menuName = "Skill/Archer/VolleyOfArrows", order = 1)]
public class VolleyOfArrows : Skill
{
    [SerializeField] private GameObject m_Arrow;
    [SerializeField] private int m_NumFireballs = 10;
    [SerializeField] private float m_SpawnRadius = 1.0f;

    public override void Activate(GameObject player, float damage, EffectType[] effects, List<string> tags, Vector3 pos = default,
        Health health = null)
    {
        Fighter fighter = player.GetComponent<Fighter>();
        Vector3 handPos = fighter.GetLeftHandTransform().position;
        Vector3 direction;
        if (health) { direction = health.transform.position - player.transform.position; }
        else { direction = GetMouseDirectionFromInstigator(handPos, pos); }
        direction.Normalize();


        for (int i = 0; i < m_NumFireballs; i++)
        {
            float angle = i * m_SpreadAngle / (m_NumFireballs - 1) - m_SpreadAngle / 2f;

            Quaternion rotationOffset = Quaternion.Euler(0f, angle, 0f);
            Vector3 spawnOffset = Quaternion.LookRotation(direction) * rotationOffset * Vector3.forward * m_SpawnRadius;
            Vector3 spawnPosition = handPos + spawnOffset;
            Quaternion rotation = Quaternion.LookRotation(spawnOffset.normalized, Vector3.up);

            GameObject projectileInstance = Instantiate(m_Arrow, spawnPosition, rotation);
            if (health) { projectileInstance.GetComponent<Projectile>().Init(player, damage, tags, health); }
            else { projectileInstance.GetComponent<Projectile>().Init(player, damage, tags); }
        }
    }
}