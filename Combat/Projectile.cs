using System.Collections.Generic;
using GW.Attributes;
using UnityEngine;

namespace GW.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float m_Speed = 1f;
        [SerializeField] private float m_AimHeight = 5f;
        [SerializeField] private float m_MaxLifeTime = 5.0f;
        [SerializeField] private float m_LifeAfterImpact = 0.1f;
        [SerializeField] private bool m_IsFollowingTarget = false;
        [SerializeField] private GameObject m_HitEffect = null;
        [SerializeField] private GameObject[] m_DestroyOnHit = null;
        private float m_Damage = 0.0f;

        private Health m_Target = null;
        private GameObject m_Instigator = null;
        private List<string> m_Tags;
        private List<Collider> m_HitColliders = new List<Collider>();

        // Start is called before the first frame update
        void Start()
        {
            //transform.LookAt(GetAimLocation());
        }

        // Update is called once per frame
        void Update()
        {
            //if (m_Target == null) return;
            if (m_Target && m_Target.IsDead()) Destroy(gameObject);
            if (m_IsFollowingTarget) transform.LookAt(GetAimLocation());
            transform.Translate(Vector3.forward * m_Speed * Time.deltaTime);
        }

        public void Init(GameObject instigator, float damage, List<string> tags, Health target = null)
        {
            if (target)
            {
                m_Target = target;
            }
            m_Damage = damage;
            m_Instigator = instigator;
            m_Tags = tags;
            Destroy(gameObject, m_MaxLifeTime);
        }

        //public void SetTarget(Health target, GameObject instigator, float damage)
        //{
        //    m_Target = target;
        //    m_Damage = damage;
        //    m_Instigator = instigator;s
        //    Destroy(gameObject, m_MaxLifeTime);
        //}

        private Vector3 GetAimLocation()
        {
            if (m_Target == null) return transform.position + Vector3.forward;
            BoxCollider targetBoxCollider = m_Target.GetComponent<BoxCollider>();
            if (targetBoxCollider == null)
            {
                Vector3 pos = m_Target.transform.position;
                pos.y += 2.0f;
                return pos;
            }
            return m_Target.transform.position + Vector3.up * targetBoxCollider.size.y / 2;
        }

        public void SetLookAt()
        {
            transform.LookAt(GetAimLocation());
        }

        public void SetLookAt(Vector3 dir)
        {
            transform.LookAt(dir);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.GetComponent<Health>()) return;
            if (other.GetComponent<Health>().IsDead()) return;
            if (m_Tags != null && !m_Tags.Contains(other.tag)) return;
            if (m_HitColliders.Contains(other)) return;

            //if (other.GetComponent<Health>().gameObject.layer == m_Instigator.layer) return;
                m_Speed = 0.0f;
            if (m_HitEffect)
                Instantiate(m_HitEffect, GetAimLocation(), Quaternion.identity);

            //foreach (GameObject toDestroy in m_DestroyOnHit)
            //{
            //    Destroy(toDestroy);
            //}

            //if (m_Target.IsDead())
            //{
            //    Destroy(gameObject);
            //    return;
            //}

            //if (other.GetComponent<Health>() != m_Target) return;
            //m_Target.TakeDamage(m_Instigator, m_Damage);

            //if (hitCollider.GetComponent<Health>() == m_Instigator) continue;
            //if (hitCollider.GetComponent<Health>().gameObject.layer == m_Instigator.layer) continue;
            m_HitColliders.Add(other);
                other.GetComponent<Health>().TakeDamage(m_Instigator, m_Damage);
               // Debug.Log("hit: " + other);
               Destroy(gameObject);
        }
    }
}
