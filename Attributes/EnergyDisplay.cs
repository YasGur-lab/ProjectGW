using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GW.Attributes
{
    public class EnergyDisplay : MonoBehaviour
    {
        [SerializeField] private Slider m_Slider;
        [SerializeField] private TextMeshProUGUI m_Label;
        [SerializeField] private GameObject[] m_RegenerationPips;
        [SerializeField] private GameObject[] m_DegenerationPips;
        int m_CurrentPipsCount;
        private Energy m_TargetSelection;

        public void Init(Energy selection)
        {
            m_TargetSelection = selection;

            m_TargetSelection.EnergyChangedEvent += OnEnergyChange;
            m_TargetSelection.EnergyRegenerationChangedEvent += OnEnergyRegenerationChange;
        }

        public void OnDestroy()
        {
            if (m_TargetSelection == null) return;
            m_TargetSelection.EnergyChangedEvent -= OnEnergyChange;
            m_TargetSelection.EnergyRegenerationChangedEvent -= OnEnergyRegenerationChange;
        }

        private void OnEnergyRegenerationChange(object sender, EnergyRegenerationChangedEventArgs e)
        {
            if ((Energy)sender == m_TargetSelection)
                UpdateEnergyBarPips(e.CurrentEnergyRenegeration);
        }

        private void OnEnergyChange(object sender, EnergyChangedEventArgs e)
        {
            if ((Energy)sender == m_TargetSelection)
                UpdateEnergyBar(e.CurrentEnergy, e.MaxEnergy);
        }

        public void UpdateEnergyBar(float currentEnergy, float originalEnergy)
        {
            m_Slider.value = currentEnergy / originalEnergy;
            int currentHealthToInt = (int)currentEnergy;
            m_Label.text = currentHealthToInt.ToString();
        }

        public void UpdateEnergyBarPips(int amount)
        {
            if (m_CurrentPipsCount == amount) return;

            if (amount == 0)
            {
                foreach (var regenerationPips in m_RegenerationPips)
                {
                    if (regenerationPips.activeSelf) regenerationPips.SetActive(false);
                }
                foreach (var degenerationPips in m_DegenerationPips)
                {
                    if (degenerationPips.activeSelf) degenerationPips.SetActive(false);
                }

                m_CurrentPipsCount = amount;
                return;
            }
            if (amount > 0)
            {
                for (int i = 0; i < m_RegenerationPips.Length; i++)
                {
                    if (i < amount)
                    {
                        m_RegenerationPips[i].SetActive(true);
                    }
                    else
                    {
                        m_RegenerationPips[i].SetActive(false);
                    }
                }
                foreach (var degenerationPips in m_DegenerationPips)
                {
                    if (degenerationPips.activeSelf) degenerationPips.SetActive(false);
                }
            }

            if (amount < 0)
            {
                for (int i = 0; i < m_DegenerationPips.Length; i++)
                {
                    if (i < Mathf.Abs(amount))
                    {
                        m_DegenerationPips[i].SetActive(true);
                    }
                    else
                    {
                        m_DegenerationPips[i].SetActive(false);
                    }
                }
                foreach (var regenerationPips in m_RegenerationPips)
                {
                    if (regenerationPips.activeSelf) regenerationPips.SetActive(false);
                }
            }

            m_CurrentPipsCount = amount;
        }
    }
}