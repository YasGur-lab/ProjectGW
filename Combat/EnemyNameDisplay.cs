using System.Collections;
using System.Collections.Generic;
using GW.Combat;
using GW.Control;
using UnityEngine;
using UnityEngine.UI;

namespace GW.Attributes
{
    public class EnemyNameDisplay : MonoBehaviour
    {
        private TargetSystem m_TargetSystem;
        // Start is called before the first frame update
        void Awake()
        {
            m_TargetSystem = GameObject.FindWithTag("Player").GetComponent<TargetSystem>();
        }

        // Update is called once per frame
        void Update()
        {
            if (m_TargetSystem.GetTarget() == null) return;
            if (m_TargetSystem.GetTarget().GetComponent<Health>() == null) return;
            if (GetComponent<Text>().text == m_TargetSystem.GetTarget().name) return;

            GetComponent<Text>().text = m_TargetSystem.GetTarget().name;
        }
    }
}