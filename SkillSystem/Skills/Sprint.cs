using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Sprint", menuName = "Skill/Warrior/Sprint", order = 1)]
public class Sprint : Skill
{
    public override void Activate(GameObject player, float damage, EffectType[] effects, List<string> mask, Vector3 pos = default,
        Health health = null)
    {
        foreach (var effectType in effects)
        {
            EffectType newEffectType = effectType;
            player.GetComponent<EffectComponent>().ApplyEffect(newEffectType, GetActiveTime(), this);
        }
    }
}
