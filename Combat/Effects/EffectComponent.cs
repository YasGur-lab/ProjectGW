using System;
using System.Collections.Generic;
using System.Linq;
using GW.Statistics;
using UnityEngine;

public class EffectsChangedEventArgs : EventArgs
{
    public Effect NewEffect { get; }
    public Skill NewSkill { get; }

    public EffectsChangedEventArgs(Effect effect = null, Skill skill = null)
    {
        NewEffect = effect;
        NewSkill = skill;
    }
}

public delegate void EffectAddedEventHandler(object sender, EffectsChangedEventArgs e);
public delegate void EffectUpdatedEventHandler(object sender, EffectsChangedEventArgs e);
public delegate void EffectRemovedEventHandler(object sender, EffectsChangedEventArgs e);

public class EffectComponent : MonoBehaviour
{
    [SerializeField] private List<Effect> m_ActiveEffects = new List<Effect>();
    UI_Effects m_UIEffects;
    private ParticleSystem m_EnchantementParticles;
    public event EffectAddedEventHandler EffectAddedEvent;
    public event EffectUpdatedEventHandler EffectUpdatedEvent;
    public event EffectRemovedEventHandler EffectRemovedEvent;

    void Start()
    {

        if(m_EnchantementParticles) 
            m_EnchantementParticles.Stop();
        float cooldownUpdateInterval = 0.2f;
        InvokeRepeating("UpdateEffects", cooldownUpdateInterval, cooldownUpdateInterval);
    }

    public void ApplyEffect(EffectType effectType, float duration, Skill skill)
    {
        Effect existingEffect = m_ActiveEffects.FirstOrDefault(effect => effect.FromSkill == skill);


        if (existingEffect != null)
        {
            // Refresh the effect's duration
            existingEffect.SetRemainingDuration(duration);

            //if (effectType.canStack)
            //{
            //    existingEffect.AddStack();
            //}
        }
        else
        {
            Effect newEffect = new Effect(effectType, skill, effectType.m_Description);
            newEffect.SetRemainingDuration(duration);
            newEffect.OriginalDuration = duration;
            OnEffectApplied(newEffect, skill);
            newEffect.OnEffectExpired += OnEffectExpired;

            if (skill.GetSkillType() == SkillType.Enchantment && !m_EnchantementParticles.isPlaying)
            {
                m_EnchantementParticles.Play();
            }
        }
    }

    private void OnEffectExpired(Effect expiredEffect)
    {
        expiredEffect.RemoveBuffEffects(gameObject, expiredEffect.FromSkill);
        m_ActiveEffects.Remove(expiredEffect);
        
        if (m_UIEffects) m_UIEffects.RemoveEffectToDisplay(expiredEffect);
        
        bool hasSameTypeEffect = m_ActiveEffects.Any(effect => effect.GetEffectType() == expiredEffect.GetEffectType());

        // Stop the particle system if there are no more effects of the same type
        if (!hasSameTypeEffect)
        {
            if (expiredEffect.GetEffectType() is RegenerationEffectType regenerationEffectType && m_EnchantementParticles.isPlaying)
            {
                m_EnchantementParticles.Stop();
            }
        }
        EffectRemovedEvent?.Invoke(this, new EffectsChangedEventArgs(expiredEffect));
    }

    private void OnEffectApplied(Effect newEffect, Skill skill)
    {
        m_ActiveEffects.Add(newEffect);
        newEffect.ApplyBuffEffects(gameObject, skill);
        if(m_UIEffects) 
            m_UIEffects.AddEffectToDisplay(newEffect, skill);
        EffectAddedEvent?.Invoke(this, new EffectsChangedEventArgs(newEffect, skill));
    }

    public void UpdateEffects()
    {
        HashSet<Effect> activeEffectsCopy = new HashSet<Effect>(m_ActiveEffects);

        foreach (var effect in activeEffectsCopy)
        {
            effect.Tick(-0.2f);
            if (m_UIEffects) { m_UIEffects.UpdateEffectDuration(effect); }
            EffectUpdatedEvent?.Invoke(this, new EffectsChangedEventArgs(effect));
        }
    }

    public void RemoveAllEffects()
    {
        foreach (var effect in m_ActiveEffects)
        {
            effect.RemoveBuffEffects(gameObject, effect.FromSkill);
            if (m_UIEffects) m_UIEffects.RemoveEffectToDisplay(effect);
            EffectRemovedEvent?.Invoke(this, new EffectsChangedEventArgs(effect));
        }
        m_ActiveEffects.Clear();
    }

    public void SetEnchantementParticles(ParticleSystem particles) => m_EnchantementParticles = particles;

    public ParticleSystem GetEnchantementParticles() => m_EnchantementParticles;

    public List<Effect> GetActiveEffects() => m_ActiveEffects;

    public void SetUIEffects(UI_Effects display) => m_UIEffects = display;

    public void RemoveUIEffects() => m_UIEffects = null;
}