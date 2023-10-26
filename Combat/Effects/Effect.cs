using System;
using GW.Attributes;
using GW.Movement;
using StarterAssets;
using UnityEngine;

public class Effect
{
    public delegate void EffectExpiredDelegate(Effect expiredEffect);
    public event EffectExpiredDelegate OnEffectExpired;

    private EffectType m_EffectType;
    private Skill m_ComingFromSkill;
    private float m_OriginalDuration;
    private float m_RemainingDuration;
    private int m_CurrentStacks;
    private String m_Description;

    public Effect(EffectType type, Skill skill, String description)
    {
        m_EffectType = type;
        m_RemainingDuration = type.duration;
        m_OriginalDuration = type.duration;
        m_CurrentStacks = 1;
        m_ComingFromSkill = skill;
        m_Description = description;
    }

    public EffectType GetEffectType()
    {
        return m_EffectType;
    }

    public float GetRemainingDuration()
    {
        return m_RemainingDuration;
    }

    public bool IsExpired()
    {
        if(m_RemainingDuration <= 0) return true;
        return false;
    }

    public int GetCurrentStacks()
    {
        return m_CurrentStacks;
    }

    public void AddStack()
    {
        if (m_EffectType.canStack && m_CurrentStacks < m_EffectType.maxStacks)
        {
            m_CurrentStacks++;
        }
    }

    public void Tick(float deltaTime)
    {
        m_RemainingDuration += deltaTime;
        if (m_RemainingDuration <= 0)
        {
            OnEffectExpired?.Invoke(this);
        }
    }
    
    public void ApplyBuffEffects(GameObject entity, Skill skill)
    {
        if (m_EffectType is SpeedEffectType speedEffectType)
        {
            // Increase the entity's movement speed by the speedMultiplier
            if(entity.GetComponent<Mover_V2>()) entity.GetComponent<Mover_V2>().ModifyMovementSpeed(speedEffectType.SpeedMultiplier);
            else entity.GetComponent<ThirdPersonController>().ModifyMovementSpeed(speedEffectType.SpeedMultiplier);
        }
        else if (m_EffectType is RegenerationEffectType regenerationEffectType)
        {
            // Increase the entity's movement speed by the speedMultiplier
           // Debug.Log("ApplyBuffEffects" + regenerationEffectType.RenegerationTick);
            entity.GetComponent<Health>().ModifyHealthRegenerationRate(regenerationEffectType.RenegerationTick);
        }
        else if (m_EffectType is DegenerationEffectType degenerationEffectType)
        {
            // Reset the entity's movement speed to its original value
            entity.GetComponent<Health>().ModifyHealthRegenerationRate(degenerationEffectType.DenegerationTick);
        }
        else if (m_EffectType is BleedingEffectType bleedingEffectType)
        {
            // Reset the entity's movement speed to its original value
            entity.GetComponent<Health>().ModifyHealthRegenerationRate(bleedingEffectType.BleedingTick);
        }
        else if (m_EffectType is BurningEffectType burningEffectType)
        {
            // Reset the entity's movement speed to its original value
            entity.GetComponent<Health>().ModifyHealthRegenerationRate(burningEffectType.BurningTick);
        }
        // Add more checks and effects for other buff types as needed
    }

    public void RemoveBuffEffects(GameObject entity, Skill skill)
    {
        if (m_EffectType is SpeedEffectType)
        {
            // Reset the entity's movement speed to its original value
            if(entity.GetComponent<Mover_V2>()) entity.GetComponent<Mover_V2>().ResetMovementSpeed();
            else entity.GetComponent<ThirdPersonController>().ResetMovementSpeed();
        }
        else if (m_EffectType is RegenerationEffectType regenerationEffectType)
        {
            // Reset the entity's movement speed to its original value
            //Debug.Log("RemoveBuffEffects" + regenerationEffectType.RenegerationTick);
            entity.GetComponent<Health>().ModifyHealthRegenerationRate(-regenerationEffectType.RenegerationTick);
        }
        else if (m_EffectType is DegenerationEffectType degenerationEffectType)
        {
            // Reset the entity's movement speed to its original value
            entity.GetComponent<Health>().ModifyHealthRegenerationRate(-degenerationEffectType.DenegerationTick);
        }
        else if (m_EffectType is BleedingEffectType bleedingEffectType)
        {
            // Reset the entity's movement speed to its original value
            entity.GetComponent<Health>().ModifyHealthRegenerationRate(-bleedingEffectType.BleedingTick);
        }
        else if (m_EffectType is BurningEffectType burningEffectType)
        {
            // Reset the entity's movement speed to its original value
            entity.GetComponent<Health>().ModifyHealthRegenerationRate(-burningEffectType.BurningTick);
        }
        // Add more checks and effects for other buff types as needed
    }

    public float SetRemainingDuration(float value) => m_RemainingDuration = value;
    public float OriginalDuration
    {
        get => m_OriginalDuration;
        set => m_OriginalDuration = value;
    }

    public Skill FromSkill => m_ComingFromSkill;
    public EffectType EffectType => m_EffectType;

    public string Description => m_Description;
}