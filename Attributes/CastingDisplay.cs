using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GW.Combat
{
    public class CastingDisplay : MonoBehaviour
    {
        [SerializeField] private Slider m_Slider;
        [SerializeField] private TextMeshProUGUI m_Label;
        private float m_CurrentTimerUntilCastingBarOff;
        [SerializeField] float m_MaxTimerUntilOffCombat;
        private bool m_Casting;
        public void Update()
        {
            //if (m_Casting)
            //{
            //    m_CurrentTimerUntilCastingBarOff -= Time.deltaTime;
            //    {
            //        if (m_CurrentTimerUntilCastingBarOff < 0)
            //        {
            //            gameObject.SetActive(false);
            //        }
            //    }
            //}
        }

        public void UpdateCastingBar(float currentCastingTime, Skill skill)
        {
            //m_Casting = true;
            //m_CurrentTimerUntilCastingBarOff = m_MaxTimerUntilOffCombat;
            m_Slider.value = currentCastingTime / skill.GetCastTime();
            m_Label.text = skill.name;
        }

        public Slider GetSlider()

        {
            return m_Slider;
        }
    }
}
