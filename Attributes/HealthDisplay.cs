using GW.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GW.Attributes
{
    public class HealthDisplay : MonoBehaviour, IPointerClickHandler
    {
        private Health m_GOHealth;
        private TargetSystem m_TargetSystem;
        [SerializeField] private Slider m_Slider;
        [SerializeField] private TextMeshProUGUI m_Label;
        [SerializeField] private GameObject[] m_RegenerationPips;
        [SerializeField] private GameObject[] m_DegenerationPips;
        int m_CurrentPipsCount;
        private UI_Party m_PartyUI;

        public void Init(Health selection)
        {
            m_TargetSystem = FindAnyObjectByType<TargetSystem>();
            m_GOHealth = selection;
            m_PartyUI = GetComponentInParent<UI_Party>();

            if (m_Slider == null)
            {
                Debug.Log("Slider: " + null);
            }

            m_GOHealth.HealthChangedEvent += OnHealthChange;
            m_GOHealth.HealthRegenerationChangedEvent += OnHealthRegenerationChange;

            UpdateHealthBar(m_GOHealth.GetHealth(), m_GOHealth.GetOriginalHealth());
        }

        public void OnDestroy()
        {
            if (m_GOHealth == null) return;
            m_GOHealth.HealthChangedEvent -= OnHealthChange;
            m_GOHealth.HealthRegenerationChangedEvent -= OnHealthRegenerationChange;
        }

        private void OnHealthRegenerationChange(object sender, HealthRegenerationChangedEventArgs e)
        {
            //Debug.Log("OnHealthRegenerationChange");
            if ((Health)sender == m_GOHealth)
            {
                if (m_Slider == null)
                {
                    Debug.Log("Slider: " + null);
                }
                UpdateHealthBarPips(e.CurrentHealthRenegeration);
            }
                
        }

        private void OnHealthChange(object sender, HealthChangedEventArgs e)
        {
            //Debug.Log("OnHealthChange");
            if ((Health)sender == m_GOHealth)
                UpdateHealthBar(e.CurrentHealth, e.MaxHealth);
        }

        public void UpdateHealthBar(float currentHealth, float originalHealth)
        {
            m_Slider.value = currentHealth / originalHealth;
            int currentHealthToInt = (int)currentHealth;
            m_Label.text = currentHealthToInt.ToString();
        }

        public void UpdateHealthBarPips(int amount)
        {
            if (m_CurrentPipsCount == amount) return;

            if (amount == 0)
            {
                foreach (var regenerationPips in m_RegenerationPips)
                {
                    if(regenerationPips.activeSelf) regenerationPips.SetActive(false);
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

        public void OnPointerClick(PointerEventData eventData)
        {
            if(m_TargetSystem == null || m_PartyUI == null) return;
            Fighter target = m_TargetSystem.GetComponent<Fighter>();
            m_TargetSystem.SetAllyTarget(m_GOHealth.gameObject);
            target.m_AllyTarget = m_GOHealth;
            if(!target.IsInCombat())
                m_PartyUI.ShowRemoveButton();
        }
    }
}