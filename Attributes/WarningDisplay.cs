using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GW.Combat
{
    public class WarningDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_Label;
        private float m_CurrentTimerUntilWarningOff;
        [SerializeField] float m_MaxTimerUntilWarningOff;
        private bool m_WarningOn;

        private void Start()
        {
            m_Label.text = "";
        }

        public void Update()
        {
            if (m_WarningOn)
            {
                m_CurrentTimerUntilWarningOff -= Time.deltaTime;
                {
                    if (m_CurrentTimerUntilWarningOff < 0)
                    {
                        m_Label.text = "";
                        m_WarningOn = false;
                    }
                }
            }
        }

        public void UpdateWarningText(WarningMessages text)
        {
            m_WarningOn = true;
            m_CurrentTimerUntilWarningOff = m_MaxTimerUntilWarningOff;
            m_Label.text = text.ToString();
        }

    }
}
