using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/EffectType")]
public class EffectType : ScriptableObject
{
    public String m_Description;

    // Attribute modifiers
    public int strengthModifier;
    public int defenseModifier;
    // Add more attribute modifiers as needed

    // Resource modifiers
    public int lifeModifier;
    public int manaModifier;
    // Add more resource modifiers as needed

    // Effect to apply (e.g., DOT effect, on-hit effect)
    public GameObject effectPrefab;
    // You can have more than one effect properties if there are multiple effects

    // Buff duration
    public float duration;

    // Buff stacks
    public bool canStack;
    public int maxStacks;

    // Add more properties and methods as needed
}