using GW.Attributes;
using GW.Statistics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Skill : ScriptableObject
{
    [SerializeField] private GameObject m_Player;
    [SerializeField] public SkillList m_Skill;
    [SerializeField] public Sprite m_Icon;
    [SerializeField] public ProfessionsAttributes m_Attribute;
    [SerializeField] private SkillType m_SkillType;
    [SerializeField] private Stat[] m_Stat;
    [SerializeField] protected string m_Description = "Skill description.";

    protected float m_Damage;
    [SerializeField] protected float m_Range;


    [SerializeField] protected float m_CooldownTime;
    [SerializeField] protected float m_ActiveTime;

    [SerializeField] protected float m_CastTime;
    [SerializeField] protected float m_TimeBeforeCasting;
    [SerializeField] protected int m_EnergyCost;
    [SerializeField] protected bool m_GroundTarget;
    [SerializeField] protected VisualShapes m_VisualShape;
    [SerializeField] protected float m_Radius = 0.0f;
    [SerializeField] protected float m_SpreadAngle = 0.0f;
    [SerializeField] protected bool m_RequiresTarget;
    [SerializeField] private AnimationClip skillAnimation;
    [SerializeField] protected bool m_FullBodyAnimation;
    [SerializeField] public AnimatorOverrideController m_SkillController = null;

    [SerializeField] public EffectType[] m_Effects;
    [SerializeField] private bool m_IsASupportiveSkill;
    [SerializeField] private HealthThresholds m_HealthThresholds;
    [SerializeField] private bool m_CanSelfTarget = true;

    //public Skill(Skill mSkill)
    //{
    //    m_Skill = mSkill.m_Skill;
    //    m_Icon = mSkill.m_Icon;
    //    m_Attribute = mSkill.m_Attribute;
    //    m_SkillType = mSkill.m_SkillType;
    //    m_Description = mSkill.m_Description;
    //    m_Damage = mSkill.m_Damage;
    //    m_Range = mSkill.m_Range;
    //    m_CooldownTime = mSkill.m_CooldownTime;
    //    m_ActiveTime = mSkill.m_ActiveTime;
    //    m_CastTime = mSkill.m_CastTime;
    //    m_TimeBeforeCasting = mSkill.m_TimeBeforeCasting;
    //    m_EnergyCost = mSkill.m_EnergyCost;
    //    skillAnimation = mSkill.skillAnimation;
    //    m_FullBodyAnimation = mSkill.m_FullBodyAnimation;
    //    m_Effects = mSkill.m_Effects;

    //}

    //public virtual void Activate(GameObject player) { }

    public virtual void Activate(GameObject player, float damage, EffectType[] effects, List<string> tags, Vector3 pos,
        Health health = null) { }
    //public virtual void Activate(GameObject player, Health health, float damage, EffectType[] effects) { }

    public static Vector3 GetMousePos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.CompareTag("Terrain"))
            {
                return hit.point;
            }
        }
        return Vector3.zero;
    }

    public bool IsTargetInTagList(GameObject target, List<string> tagList)
    {
        return tagList.Contains(target.tag);
    }

    public Vector3 GetMouseDirectionFromInstigator(Vector3 instigatorPos, Vector3 mousePos)
    {
        mousePos.y += 1.0f;
        Vector3 direction = instigatorPos - mousePos;
        return -direction;
    }

    public virtual void Deactivate() {}

    public float GetCooldownTime() { return m_CooldownTime; }
    public float GetActiveTime() { return m_ActiveTime; }

    public float GetCastTime() { return m_CastTime; }
    public float GetTimeBeforeCasting() { return m_TimeBeforeCasting; }

    public float GetRange() { return m_Range; }

    public void SetDamage(float damage) { m_Damage = damage; }

    public ProfessionsAttributes GetAttribute() { return m_Attribute; }

    //public float GetDamage() { return m_Damage; }
    public int GetEnergyCost() { return m_EnergyCost; }
    public SkillType GetSkillType() { return m_SkillType; }

    public string GetDescription() { return m_Description; }
    public AnimationClip GetAnimation() { return skillAnimation; }
    public bool HasFullBodyAnimation() => m_FullBodyAnimation;
    public bool DoesRequireTarget() => m_RequiresTarget;

    //public Stat GetStat(Stat stat)
    //{

    //}

    public HealthThresholds HealthThreshold => m_HealthThresholds;
    public bool IsASupportiveSkill => m_IsASupportiveSkill;
    public bool IsSelfTarget => m_CanSelfTarget;

    public float GetRadius() => m_Radius;
    public bool IsGroundTarget => m_GroundTarget;
    public VisualShapes VisualShape => m_VisualShape;
    public float AimSpreadAngle => m_SpreadAngle;
}
