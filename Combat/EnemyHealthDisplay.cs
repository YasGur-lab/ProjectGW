using System.Collections;
using System.Collections.Generic;
using GW.Combat;
using GW.Control;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

namespace GW.Attributes
{
    public class EnemyHealthDisplay : MonoBehaviour
    {
        private Health m_Health; 

        [SerializeField] private Slider m_Slider;

        [SerializeField] private GameObject m_UI;
        // Start is called before the first frame update
        void Start()
        {
            //m_TargetSystem = GameObject.FindWithTag("Player").GetComponent<TargetSystem>();
            m_Health = GetComponent<Health>();
        }

        // Update is called once per frame
        void Update()
        {
            if(m_Health == null || m_UI == null || m_Slider == null) return;

            if (m_Health.IsDead())
            {
                if(m_UI.activeSelf)
                    m_UI.SetActive(false);
                return;
            }

            if (!m_Health.HasTakenDamage())
            {
                if (m_UI.activeSelf)
                    m_UI.SetActive(false);
            }
            else
            {
                if (!m_UI.activeSelf)
                    m_UI.SetActive(true);
            }
            
            //if (m_TargetSystem.GetTarget() == null) return;
            //if (m_TargetSystem.GetTarget().GetComponent<Health>() == null) return;
            //GetComponent<Text>().text = m_TargetSystem.GetTarget().GetComponent<Health>().GetHealth() + "/" +
            //                            m_TargetSystem.GetTarget().GetComponent<Health>().GetOriginalHealth();


        }

        public void UpdateHealthBar(float currentHealth, float originalHealth)
        {
            m_Slider.value = currentHealth / originalHealth;
        }
    }
}