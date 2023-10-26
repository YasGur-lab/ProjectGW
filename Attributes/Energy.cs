using System;
using System.Collections;
using System.Collections.Generic;
using GW.Combat;
using GW.Core;
using GW.Movement;
using GW.Statistics;
using Unity.VisualScripting;
using UnityEngine;

namespace GW.Attributes
{
    public class EnergyChangedEventArgs : EventArgs
    {
        public float CurrentEnergy { get; }
        public float MaxEnergy { get; }

        public EnergyChangedEventArgs(float currentEnergy, float maxEnergy)
        {
            CurrentEnergy = currentEnergy;
            MaxEnergy = maxEnergy;
        }
    }

    public delegate void EnergyChangedEventHandler(object sender, EnergyChangedEventArgs e);

    public class EnergyRegenerationChangedEventArgs : EventArgs
    {
        public int CurrentEnergyRenegeration { get; }
        public EnergyRegenerationChangedEventArgs(int currentEnergyRenegeration)
        {
            CurrentEnergyRenegeration = currentEnergyRenegeration;
        }
    }

    public delegate void EnergyRegenerationChangedEventHandler(object sender, EnergyRegenerationChangedEventArgs e);

    public class Energy : MonoBehaviour
    {
        [SerializeField] private LazyValue<float> m_EnergyPoint;
        private bool m_isDead = false;

        private bool m_TookDamage;

        private float m_TimerBeforeNotInCombat;
        private float m_TimerBeforeAnimatorOff;

        private GameObject m_DamageInstigator;

        private float m_OriginalEnergy;

        [SerializeField] EnergyDisplay m_EnergyDisplay;

        [SerializeField] int m_EnergyRegenerationRate;

        private float m_RegenerationTimer = 0f;
        private const float REGENERATION_INTERVAL = 3f;

        private bool m_RegenerationTimerReset;
        private int m_EnergyRegenerationRateViaSkills;

        public event EnergyChangedEventHandler EnergyChangedEvent;
        public event EnergyRegenerationChangedEventHandler EnergyRegenerationChangedEvent;

        private Health m_Health;
        // Start is called before the first frame update
        void Awake()
        {
            m_EnergyPoint = new LazyValue<float>(GetInitialEnergy);
        }

        void Start()
        {
            m_EnergyPoint.value = GetComponent<BaseStats>().GetSpecificStat(Stat.Energy);
            m_OriginalEnergy = GetComponent<BaseStats>().GetSpecificStat(Stat.Energy);

            m_EnergyRegenerationRate = (int)GetComponent<BaseStats>().GetSpecificStat(Stat.EnergyRegeneration);

            float cooldownUpdateInterval = 0.1f; // Update every 0.1 seconds (adjust as needed)
            InvokeRepeating("UpdateEnergy", cooldownUpdateInterval, cooldownUpdateInterval);
            if (gameObject.CompareTag("Player")) m_EnergyDisplay.Init(this);

            m_Health = GetComponent<Health>();
        }

        private void UpdateEnergy()
        {
            if (m_Health.IsDead()) return;
            EnergyRegeneration(m_EnergyRegenerationRate);
        }

        private void EnergyRegeneration(int energyToAdd)
        {
            float increment = (m_EnergyRegenerationRate + m_EnergyRegenerationRateViaSkills) / (3f / 0.1f);
            m_EnergyPoint.value += increment;

            if (m_EnergyPoint.value > m_OriginalEnergy)
            {
                m_EnergyPoint.value = m_OriginalEnergy;
            }

            EnergyChangedEvent?.Invoke(this, new EnergyChangedEventArgs(m_EnergyPoint.value, m_OriginalEnergy));
            EnergyRegenerationChangedEvent?.Invoke(this, new EnergyRegenerationChangedEventArgs(m_EnergyRegenerationRate + m_EnergyRegenerationRateViaSkills));
        }

        public void ModifyEnergyRegenerationRate(int regenerationRateToAdd)
        {
            m_EnergyRegenerationRateViaSkills += regenerationRateToAdd;
            EnergyRegenerationChangedEvent?.Invoke(this, new EnergyRegenerationChangedEventArgs(m_EnergyRegenerationRate + m_EnergyRegenerationRateViaSkills));
        }

        public void ResetEnergyRegenerationRate(int regenerationRateToRemove)
        {
            m_EnergyRegenerationRateViaSkills -= regenerationRateToRemove;
            EnergyRegenerationChangedEvent?.Invoke(this, new EnergyRegenerationChangedEventArgs(m_EnergyRegenerationRate + m_EnergyRegenerationRateViaSkills));
        }

        public void ReduceEnergy(float cost)
        {
            m_EnergyPoint.value -= cost;
            EnergyChangedEvent?.Invoke(this, new EnergyChangedEventArgs(m_EnergyPoint.value, m_OriginalEnergy));
        }

        public void AddEnergy(float cost)
        {
            m_EnergyPoint.value += cost;
            EnergyChangedEvent?.Invoke(this, new EnergyChangedEventArgs(m_EnergyPoint.value, m_OriginalEnergy));
        }

        public bool HasEnoughEnergy(int cost)
        {
            if (m_EnergyPoint.value >= cost) return true;
            else return false;
        }

        public void ResetPlayer()
        {
            m_EnergyPoint.value = GetInitialEnergy();
        }

        public float GetPercentage() { return m_EnergyPoint.value / GetInitialEnergy(); }
        public float GetEnergy() { return m_EnergyPoint.value; }
        public float GetInitialEnergy() { return m_OriginalEnergy; }

        public void SetEnergy(float amount)
        {
            m_EnergyPoint.value = amount;
            EnergyChangedEvent?.Invoke(this, new EnergyChangedEventArgs(m_EnergyPoint.value, m_OriginalEnergy));
        }

        public void SetEnergyDisplay(EnergyDisplay display)
        {
            m_EnergyDisplay = display;
            m_EnergyDisplay.Init(this);
            EnergyChangedEvent?.Invoke(this, new EnergyChangedEventArgs(m_EnergyPoint.value, m_OriginalEnergy));
            
        }

        public void RemoveEnergyDisplay() { m_EnergyDisplay = null; }

        public int GetEnergyRegenerationRate()
        {
            return m_EnergyRegenerationRate + m_EnergyRegenerationRateViaSkills;
        }
    }
}
