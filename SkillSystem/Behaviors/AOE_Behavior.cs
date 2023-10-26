using System;
using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using UnityEngine;

public class AOE_Behavior : MonoBehaviour
{
    //--BASE
    private Health m_Target = null;
    private GameObject m_Instigator = null;

    //--AOE--
    [SerializeField] private GameObject m_EffectOnImpact;
    private List<Collider> m_HitColliders = new List<Collider>();
    private float m_Damage;
    [SerializeField] private float m_Radius;

    //--AOE PERSISTENT--
    [SerializeField] private GameObject m_PersistentEffect;
    [SerializeField] private bool m_IsPersistent;
    private bool m_PersistentAOEActive = false;
    [SerializeField] private float m_PersistentDuration = 2.0f;
    [SerializeField] private LayerMask m_TerrainLayerMask;
    private float m_Timer;
    private List<string> m_Tags;
    private LayerMask m_DetectionLayer;
    private Fighter m_Player;
    private TerrainMaterialUpdater m_TerrainMaterialUpdater;

    void Start()
    {
        float cooldownUpdateInterval = 1.0f; // Update every 0.1 seconds (adjust as needed)
        InvokeRepeating("UpdateCooldowns", cooldownUpdateInterval, cooldownUpdateInterval);
    }

    public void Init(GameObject instigator, float damage, List<string> tags, Health target = null)
    {
        if (target) m_Target = target;
        m_Damage = damage;
        m_Instigator = instigator;
        m_Tags = tags;
        m_DetectionLayer = m_Instigator.GetComponent<Fighter>().m_DetectionLayer;

        m_Player = GameObject.FindWithTag("Player").GetComponent<Fighter>();
        m_TerrainMaterialUpdater = FindAnyObjectByType<TerrainMaterialUpdater>();

        if (m_Player.m_EnemyTags.Contains(instigator.tag))
        {
            m_TerrainMaterialUpdater.AddEnemySkillTransformToArray(transform, m_Radius);
        }
        else if (m_Player.m_AllyTags.Contains(instigator.tag))
        {
            m_TerrainMaterialUpdater.AddAllySkillTransformToArray(transform, m_Radius);
        }
    }

    public void UpdateCooldowns()
    {
        if (m_IsPersistent)
        {
            m_Timer += 1.0f;
            if (m_Timer >= 1.0f && m_HitColliders.Count > 0)
            {
                m_HitColliders.Clear();
                m_Timer = 0.0f;
            }

            m_PersistentDuration -= 1.0f;

            if (m_PersistentDuration <= 0)
            {
                if (m_Player.m_EnemyTags.Contains(m_Instigator.tag))
                {
                    m_TerrainMaterialUpdater.RemoveEnemySkillTransformFromArray(transform, m_Radius);
                }
                else if (m_Player.m_AllyTags.Contains(m_Instigator.tag))
                {
                    m_TerrainMaterialUpdater.RemoveAllySkillTransformFromArray(transform, m_Radius);
                }
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (m_PersistentAOEActive == false)
        //{
        //    Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, m_Radius);
        //    foreach (var hitCollider in hitColliders)
        //    {
        //        int layerMask = m_Instigator.GetComponent<Fighter>().m_TargetMask;
        //        if ((layerMask & (1 << hitCollider.gameObject.layer)) == 0)
        //        {
        //            // The hitCollider's layer is not included in the layerMask.
        //        }
        //        else
        //        {
        //            if (m_EffectOnImpact)
        //            {
        //                Instantiate(m_EffectOnImpact, hitCollider.transform.position, Quaternion.identity);
        //                if (!m_IsPersistent)
        //                {
        //                    hitCollider.GetComponent<Health>().TakeDamage(m_Instigator, m_Damage);
        //                    Destroy(gameObject);
        //                }
        //            }
        //        }



        //        //if (hitCollider.tag == "Enemy")
        //        //{
        //        //    Instantiate(m_EffectOnImpact, hitCollider.transform.position, Quaternion.identity);
        //        //    if (!m_IsPersistent)
        //        //    {
        //        //        hitCollider.GetComponent<Health>().TakeDamage(m_Instigator, m_Damage);
        //        //        Destroy(gameObject);
        //        //    }
        //        //}
        //    }

        //    if (m_IsPersistent)
        //    {
        //        m_PersistentAOEActive = true;
        //    }
        //}
    }

    private void OnTriggerStay(Collider other)
    {
        if (m_IsPersistent)
        {
            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, m_Radius, m_DetectionLayer);

            foreach (var hitCollider in hitColliders)
            {
                if (!hitCollider.GetComponent<Health>()) continue;
                if (hitCollider.GetComponent<Health>().IsDead()) continue;
                if (!m_Tags.Contains(hitCollider.tag)) return;
                //if (hitCollider.GetComponent<Health>() == m_Instigator) continue;
                //if (hitCollider.GetComponent<Health>().gameObject.layer == m_Instigator.layer) continue;
                if (!m_HitColliders.Contains(hitCollider))
                {
                    m_HitColliders.Add(hitCollider);
                    hitCollider.GetComponent<Health>().TakeDamage(m_Instigator, m_Damage);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_Radius);
    }
}