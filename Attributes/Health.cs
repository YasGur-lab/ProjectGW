using System;
using System.Collections;
using System.Collections.Generic;
using GW.Combat;
using GW.Control;
using GW.Core;
using GW.Movement;
using GW.Statistics;
using Unity.VisualScripting;
using UnityEngine;

namespace GW.Attributes
{
    public enum HealthThresholds
    {
        LowAlert,
        MediumAlert,
        HighAlert,
        ExtremeAlert
    }

    public class HealthChangedEventArgs : EventArgs
    {
        public float CurrentHealth { get; }
        public float MaxHealth { get; }

        public HealthChangedEventArgs(float currentHealth, float maxHealth)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }

    public delegate void HealthChangedEventHandler(object sender, HealthChangedEventArgs e);

    public class HealthRegenerationChangedEventArgs : EventArgs
    {
        public int CurrentHealthRenegeration { get; }
        public HealthRegenerationChangedEventArgs(int currentHealthRenegeration)
        {
            CurrentHealthRenegeration = currentHealthRenegeration;
        }
    }

    public delegate void HealthRegenerationChangedEventHandler(object sender, HealthRegenerationChangedEventArgs e);

    public class Health : MonoBehaviour
    {
        [SerializeField] private LazyValue<float> m_HealthPoint;
        private bool m_isDead = false;

        private bool m_TookDamage;


        private float m_TimerBeforeAnimatorOff;

        private GameObject m_DamageInstigator;

        private float m_OriginalHealth;

        [SerializeField] EnemyHealthDisplay m_EnemyHealthDisplay;
        [SerializeField] HealthDisplay m_HealthDisplay;

        [SerializeField] private int m_HealthRegenerationRate;
        [SerializeField] private int m_HealthRegenerationRateViaSkills;
        float m_HealthRegenerationTimer;
        private int[] m_RegenerationRates = { 1, 2, 3, 4, 5, 6, 7 };
        private float[] m_RegenerationIntervals = { 2, 4, 6, 8, 10, 12, 14 };

        [SerializeField] private UI_Damage m_DamagePopupPrefab;
        private SkillAnimationController m_AnimatorController;
        private Fighter m_Fighter;
        private Mover m_Mover;
        private ActionScheduler m_ActionScheduler;
        private HealthThresholds m_HealthThresholds;
        public event HealthChangedEventHandler HealthChangedEvent;
        public event HealthRegenerationChangedEventHandler HealthRegenerationChangedEvent;

        //--THRESHOLDS
        private float[] m_Thresholds = { 0.75f, 0.5f, 0.25f, 0.1f };

        // Start is called before the first frame update
        void Awake()
        {
            m_HealthPoint = new LazyValue<float>(GetInitialHealth);
        }

        void Start()
        {
            m_HealthPoint.value = GetComponent<BaseStats>().GetCommonStat(Stat.Health);
            m_OriginalHealth = GetInitialHealth();

            m_AnimatorController = GetComponent<SkillAnimationController>();
            m_Fighter = GetComponent<Fighter>();
            m_ActionScheduler = GetComponent<ActionScheduler>();
            m_Mover = GetComponent<Mover>();

            float cooldownUpdateInterval = 0.1f; // Update every 0.1 seconds (adjust as needed)
            InvokeRepeating("UpdateHealth", cooldownUpdateInterval, cooldownUpdateInterval);

            if(gameObject.CompareTag("Player")) m_HealthDisplay.Init(this);
        }

        private void UpdateHealth()
        {
            if (m_isDead) return;

            HealthRegeneration();
        }

        private void HealthRegeneration()
        {
            CalculateHealthRegenerationRate();
            float increment = (m_HealthRegenerationRate + m_HealthRegenerationRateViaSkills) / (3f / 0.1f);
            m_HealthPoint.value += increment;

            if (m_EnemyHealthDisplay) m_EnemyHealthDisplay.UpdateHealthBar(m_HealthPoint.value, m_OriginalHealth);

            if (m_HealthPoint.value >= m_OriginalHealth)
            {
                m_HealthPoint.value = m_OriginalHealth;
                m_HealthRegenerationRate = 0;
                m_HealthRegenerationTimer = 0.0f;
            }

            HealthChangedEvent?.Invoke(this, new HealthChangedEventArgs(m_HealthPoint.value, m_OriginalHealth));
            HealthRegenerationChangedEvent?.Invoke(this, new HealthRegenerationChangedEventArgs(m_HealthRegenerationRate + m_HealthRegenerationRateViaSkills));
        }

        private void CalculateHealthRegenerationRate()
        {
            if (m_Fighter.IsInCombat())
            {
                m_HealthRegenerationRate = 0;
                m_HealthRegenerationTimer = 0.0f;
                return;
            }

            m_HealthRegenerationTimer += 0.1f;

            for (int i = 0; i < m_RegenerationRates.Length; i++)
            {
                if (m_HealthRegenerationTimer >= m_RegenerationIntervals[i])
                {
                    m_HealthRegenerationRate = m_RegenerationRates[i];
                }
            }
        }

        public void TakeDamage(GameObject instigator, float damage)
        {
            if(instigator == null) return;
            if (instigator.GetComponent<Health>().IsDead()) return;

            if (!m_isDead)
            {
                PartySystem partySystem = GetComponentInParent<PartySystem>();
                if (!m_TookDamage && instigator != gameObject)
                {
                    if (partySystem)
                        partySystem.SetChildrenHasTakenDamage(true, instigator);
                    m_TookDamage = true;
                }

                SetInstigator(instigator);

                m_HealthPoint.value = Mathf.Max(m_HealthPoint.value - damage, 0);

                UpdateHealthThreshold();

                if (m_DamagePopupPrefab && instigator.tag == "Player")
                {
                    UI_Damage popup = Instantiate(m_DamagePopupPrefab, transform.position + Vector3.up, instigator.transform.rotation);
                    popup.InitPopup((int)damage);
                }


                if (m_EnemyHealthDisplay) m_EnemyHealthDisplay.UpdateHealthBar(m_HealthPoint.value, m_OriginalHealth);
                HealthChangedEvent?.Invoke(this, new HealthChangedEventArgs(m_HealthPoint.value, m_OriginalHealth));
                if (m_Fighter)
                {
                    m_Fighter.SetInCombatStance(false, true);
                    instigator.GetComponent<Fighter>().SetInCombatStance(false, true);
                }

                if (m_HealthPoint.value <= 0)
                {
                    if (instigator.GetComponentInParent<PartySystem>().GetLeader().CompareTag("Player"))
                        AwardExperience(instigator.GetComponentInParent<PartySystem>().GetPartyMembers());
                    
                    Die(instigator);
                }
            }
        }

        private void UpdateHealthThreshold()
        {
            float healthPercentage = m_HealthPoint.value / m_OriginalHealth;

            for (int i = m_Thresholds.Length - 1; i >= 0; i--)
            {
                if (healthPercentage <= m_Thresholds[i])
                {
                    m_HealthThresholds = (HealthThresholds)i;
                    break;
                }
            }
        }

        public void ModifyHealthRegenerationRate(int regenerationRateToAdd)
        {
            //Debug.Log("Current m_HealthRegenerationRateViaSkills :" + m_HealthRegenerationRateViaSkills);
            //Debug.Log("RegenerationRateToAdd :" + regenerationRateToAdd);
            m_HealthRegenerationRateViaSkills += regenerationRateToAdd;
            //Debug.Log("new m_HealthRegenerationRateViaSkills :" + m_HealthRegenerationRateViaSkills);
            HealthRegenerationChangedEvent?.Invoke(this, new HealthRegenerationChangedEventArgs(m_HealthRegenerationRate + m_HealthRegenerationRateViaSkills));
        }

        public void ResetHealthRegenerationRate(int regenerationRateToAdd)
        {
            m_HealthRegenerationRateViaSkills = 0;
            HealthRegenerationChangedEvent?.Invoke(this, new HealthRegenerationChangedEventArgs(m_HealthRegenerationRate + m_HealthRegenerationRateViaSkills));
        }

        public void AddHealth(float amount)
        {
            m_HealthPoint.value += amount;
            if (m_HealthPoint.value > m_OriginalHealth) m_HealthPoint.value = m_OriginalHealth;
            if (m_EnemyHealthDisplay) m_EnemyHealthDisplay.UpdateHealthBar(m_HealthPoint.value, m_OriginalHealth);
            HealthChangedEvent?.Invoke(this, new HealthChangedEventArgs(m_HealthPoint.value, m_OriginalHealth));
        }

        public void RemoveHealth(GameObject instigator, float amount)
        {
            TakeDamage(instigator, amount);
            if (m_EnemyHealthDisplay) m_EnemyHealthDisplay.UpdateHealthBar(m_HealthPoint.value, m_OriginalHealth);
            HealthChangedEvent?.Invoke(this, new HealthChangedEventArgs(m_HealthPoint.value, m_OriginalHealth));
        }

        public void SetInstigator(GameObject instigator)
        {
            if (instigator == this) return;
            m_DamageInstigator = instigator;
        }

        public void SetTookDamage(bool b)
        {
            m_TookDamage = b;
            m_Fighter.SetInCombatStance(false, true);
        }

        public GameObject GetInstigator() => m_DamageInstigator;

        private void AwardExperience(List<GameObject> partyMembers)
        {
            foreach (var member in partyMembers)
            {
                Experience experience = member.GetComponent<Experience>();
                if (experience == null) continue;
                experience.GainExperience(member, member.GetComponent<BaseStats>().GetLevel());
            }

        }

        private void Die(GameObject instigator)
        {
            if (m_isDead) return;
            if (instigator.GetComponent<Fighter>().m_Target == gameObject)
            {
                instigator.GetComponent<SkillBar_Player>().SetLoopAutoAttack(false);
            }

            if (GetComponent<EffectComponent>() == null) GetComponent<EffectComponent>();
            if (GetComponent<EffectComponent>().GetEnchantementParticles())
            {
                if (GetComponent<EffectComponent>().GetEnchantementParticles().isPlaying)
                    GetComponent<EffectComponent>().GetEnchantementParticles().Stop();
            }

            GetComponent<EffectComponent>().RemoveAllEffects();

            m_isDead = true;
            if(m_AnimatorController) m_AnimatorController.TriggerDeathAnimation();
            if(m_ActionScheduler) m_ActionScheduler.CancelCurrentAction();
            if(m_Mover) m_Mover.Cancel();
            m_Fighter.m_Target = null;
            m_Fighter.m_AllyTarget = null;
        }

        public void ResetGameObject()
        {
            m_HealthPoint.value = GetInitialHealth();
            GetComponent<Energy>().ResetPlayer();
            m_isDead = false;
            m_AnimatorController.ResetCharacterAfterDeath();
            GetComponent<Animator>().enabled = true;
            GetComponent<AIControllerBT>().ResetBehabviorTree("Default");
            m_AnimatorController.StopSkillAnimations();
            m_AnimatorController.StopAttack();
            HealthChangedEvent?.Invoke(this, new HealthChangedEventArgs(m_HealthPoint.value, m_OriginalHealth));
            m_Fighter.m_Target = null;
            m_Fighter.m_AllyTarget = null;
            gameObject.SetActive(true);
        }

        public bool IsDead() { return m_isDead; }
        public float GetPercentage() { return m_HealthPoint.value / GetInitialHealth(); }
        public float GetHealth() { return m_HealthPoint.value; }
        public int GetHealthRegenerationRate() { return m_HealthRegenerationRateViaSkills + m_HealthRegenerationRate; }
        public float GetOriginalHealth() { return GetComponent<BaseStats>().GetCommonStat(Stat.Health); }
        private float GetInitialHealth() { return GetComponent<BaseStats>().GetCommonStat(Stat.Health); }
        private float GetInitialEnergy() { return GetComponent<BaseStats>().GetSpecificStat(Stat.Energy); }
        public HealthThresholds HealthThreshold => m_HealthThresholds;
        public bool HasTakenDamage() { return m_TookDamage; }

        public void SetHealth(float amount)
        {
            m_HealthPoint.value = amount;
            HealthChangedEvent?.Invoke(this, new HealthChangedEventArgs(m_HealthPoint.value, m_OriginalHealth));
        }

        public void SetHealthDisplay(HealthDisplay display)
        {
            m_HealthDisplay = display;
            if (m_HealthDisplay)
            {
                HealthChangedEvent?.Invoke(this, new HealthChangedEventArgs(m_HealthPoint.value, m_OriginalHealth));
                m_HealthDisplay.Init(this);
            }

        }

        public void RemoveHealthDisplay() { m_HealthDisplay = null; }
    }
}
