using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "DealDamageToSelf", menuName = "Skill/Mage/DealDamageToSelf", order = 3)]
public class DealDamageToSelf : Skill
{
    public override void Activate(GameObject player, float amount, EffectType[] effects, List<string> mask, Vector3 pos = default,
        Health health = null)
    {
        player.GetComponent<Health>().RemoveHealth(player, 10);

        //foreach (var effectType in effects)
        //{f
        //    EffectType newEffectType = effectType;
        //    player.GetComponent<EffectComponent>().ApplyEffect(newEffectType, GetActiveTime(), this);
        //}
    }
}
