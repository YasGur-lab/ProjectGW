using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using UnityEngine;

namespace GW.Statistics
{
    public class UIController : MonoBehaviour
    {
        private bool m_PressedK;
        [SerializeField] GameObject m_AttributesPanel;
        [SerializeField] GameObject m_TargetPanel;
        private GameObject m_Player;
        private BaseStats m_Stats;
        private TargetSystem m_TargetSystem;
        private Fighter m_Fighter;

        //--DataStorage--
        private int m_WeaponRange;
        private GameObject m_Target;

        // Start is called before the first frame update
        void Start()
        {
            m_Player = GameObject.FindWithTag("Player");
            m_Stats = m_Player.GetComponent<BaseStats>();
            m_TargetSystem = m_Player.GetComponent<TargetSystem>();
            m_Fighter = m_Player.GetComponent<Fighter>();
        }

        // Update is called once per frame
        void Update()
        {
            TargetPanel();
        }

        private void TargetPanel()
        {
            if (m_Target == null)
            {
                m_TargetPanel.SetActive(false);
                return;
            }


            m_Target = m_TargetSystem.GetTarget();

            if (m_Target && !m_TargetPanel.activeSelf)
            {
                m_TargetPanel.SetActive(true);
            }
            else if (!m_Target && m_TargetPanel.activeSelf) m_TargetPanel.SetActive(false);
            else if (m_Target && m_TargetPanel.activeSelf)
            {
                if (m_Target.GetComponent<Health>().IsDead())
                {
                    float distance = Vector3.Distance(transform.position, m_Target.transform.position);
                    if(m_WeaponRange != (int)m_Fighter.GetWeaponRange())
                        m_WeaponRange = (int)m_Fighter.GetWeaponRange();
                    if (distance > m_WeaponRange) m_TargetSystem.SetCombatTarget(null); ;
                }
            }
        }
    }
}