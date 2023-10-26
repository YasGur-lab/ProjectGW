using System;
using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using GW.Control;
using TMPro;
using UnityEngine;

public class UI_Target : MonoBehaviour
{
    //--TARGET--
    private TargetSystem m_TargetSystem;
    [SerializeField] private Health m_HealthTarget;
    [SerializeField] private Energy m_EnergyTarget;
    [SerializeField] private EffectComponent m_EffectComponent;
    [SerializeField] private SkillBar_V2 m_CastingTarget;
    private GameObject m_CurrentTarget;

    //--UI--
    [SerializeField] TextMeshProUGUI m_TargetName;
    [SerializeField] private HealthDisplay m_HealthDisplay;
    [SerializeField] private EnergyDisplay m_EnergyDisplay;
    [SerializeField] private UI_Effects m_Effects;

    // Start is called before the first frame update
    void Start()
    {
        m_TargetSystem = GameObject.FindWithTag("Player").GetComponent<TargetSystem>();
        m_TargetSystem.TargetChangedEvent += OnTargetChanged;
        transform.GetChild(0).gameObject.SetActive(false);
    }

    private void OnTargetChanged(object sender, TargetChangedEventArgs e)
    {
        if (e.NewTarget && e.NewTarget != m_CurrentTarget)
        {
            m_CurrentTarget = e.NewTarget;
            m_TargetName.text = e.NewTarget.name;

            m_HealthTarget = e.NewTarget.GetComponent<Health>();
            
            m_HealthTarget.HealthChangedEvent += OnHealthChange;
            m_HealthTarget.HealthRegenerationChangedEvent += OnHealthRegenerationChange;

            m_EnergyTarget = e.NewTarget.GetComponent<Energy>();
            m_EnergyTarget.EnergyChangedEvent += OnEnergyChange;
            m_EnergyTarget.EnergyRegenerationChangedEvent += OnEnergyRegenerationChange;

            List<Effect> effects = m_Effects.GetEffectsList();
            List<Effect> effectsToRemove = new List<Effect>();

            if (effects.Count > 0)
            {
                foreach (var effect in effects)
                {
                    effectsToRemove.Add(effect);
                }

                foreach (var effect in effectsToRemove)
                {
                    m_Effects.RemoveEffectToDisplay(effect);
                }
            }

            m_EffectComponent = e.NewTarget.GetComponent<EffectComponent>();
            foreach (var effect in m_EffectComponent.GetActiveEffects())
            {
                m_Effects.AddEffectToDisplay(effect, effect.FromSkill);
            }
            m_EffectComponent.EffectAddedEvent += OnEffectAdded;
            m_EffectComponent.EffectUpdatedEvent += OnEffectUpdated;
            m_EffectComponent.EffectRemovedEvent += OnEffectRemoved;

            m_HealthDisplay.UpdateHealthBar(m_HealthTarget.GetHealth(), m_HealthTarget.GetOriginalHealth());
            m_EnergyDisplay.UpdateEnergyBar(m_EnergyTarget.GetEnergy(), m_EnergyTarget.GetInitialEnergy());

            transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    //--HEALTH--
    private void OnHealthChange(object sender, HealthChangedEventArgs e)
    {
        if((Health)sender == m_HealthTarget)
            m_HealthDisplay.UpdateHealthBar(e.CurrentHealth, e.MaxHealth);
    }

    private void OnHealthRegenerationChange(object sender, HealthRegenerationChangedEventArgs e)
    {
        if ((Health)sender == m_HealthTarget)
            m_HealthDisplay.UpdateHealthBarPips(e.CurrentHealthRenegeration);
    }

    //--ENERGY--
    private void OnEnergyChange(object sender, EnergyChangedEventArgs e)
    {
        if ((Energy)sender == m_EnergyTarget)
            m_EnergyDisplay.UpdateEnergyBar(e.CurrentEnergy, e.MaxEnergy);
    }
    private void OnEnergyRegenerationChange(object sender, EnergyRegenerationChangedEventArgs e)
    {
        if ((Energy)sender == m_EnergyTarget)
            m_EnergyDisplay.UpdateEnergyBarPips(e.CurrentEnergyRenegeration);
    }

    //--EFFECTS--
    private void OnEffectAdded(object sender, EffectsChangedEventArgs e)
    {
        if ((EffectComponent)sender == m_EffectComponent)
        {
            if (m_Effects.GetEffectsList().Contains(e.NewEffect)) return;
            m_Effects.AddEffectToDisplay(e.NewEffect, e.NewSkill);
        }
    }
    private void OnEffectUpdated(object sender, EffectsChangedEventArgs e)
    {
        if ((EffectComponent)sender == m_EffectComponent)
        {
            m_Effects.UpdateEffectDuration(e.NewEffect);
        }
    }
    private void OnEffectRemoved(object sender, EffectsChangedEventArgs e)
    {
        if ((EffectComponent)sender == m_EffectComponent)
        {
            m_Effects.RemoveEffectToDisplay(e.NewEffect);
        }
    }
}
