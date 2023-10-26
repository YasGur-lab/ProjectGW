using System;
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Statistics;
using UnityEngine;

namespace GW.Control
{
    public class SkillBar_NPC : SkillBar_V2
    {
        [SerializeField] private CastingDisplay m_CastingDisplay;
        //public event SkillActivatedEventHandler SkillCooldownCompleted;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();

            m_Skills = new List<Skill>(8);
            for (int i = 0; i < 8; i++)
            {
                m_Skills.Add(null);
            }

            if (m_SkillSet != null && m_SkillSet.skills.Count > 0)
            {
                int index = 0;
                SkillSet skillList = Instantiate(m_SkillSet);
                foreach (var skill in skillList.GetSkills)
                {
                    if (skill == null) continue;
                    m_Skills[index] = skill;
                    m_CastTimes[skill] = skill.GetCastTime();
                    m_ActiveTimes[skill] = 0;
                    m_CooldownTimes[skill] = 0;
                    m_SkillStates[skill] = SkillState.ready;
                    index++;
                }
            }

            if (m_WeaponSkill)
            {
                m_WeaponSkillCastTime[m_WeaponSkill] = m_WeaponSkill.GetCastTime();
                m_WeaponSkillActiveTime[m_WeaponSkill] = 0;
                m_WeaponSkillCooldownTime[m_WeaponSkill] = 0;
                m_WeaponSkillState[m_WeaponSkill] = SkillState.ready;
            }

            float cooldownUpdateInterval = 0.5f; // Update every 0.1 seconds (adjust as needed)
            InvokeRepeating("UpdateCooldowns", cooldownUpdateInterval, cooldownUpdateInterval);
        }

        public void UpdateCooldowns()
        {
            foreach (var skill in m_Skills)
            {
                if (skill == null) continue;
                switch (m_SkillStates[skill])
                {
                    case SkillState.cooldown:
                        if (m_CooldownTimes[skill] > 0)
                        {

                            m_CooldownTimes[skill] -= 0.5f;
                            if (m_CooldownTimes[skill] <= 0.0f)
                            {
                                m_SkillStates[skill] = SkillState.ready;
                            }
                        }
                        break;
                }
            }

            switch (m_WeaponSkillState[m_WeaponSkill])
            {
                case SkillState.cooldown:
                    if (m_WeaponSkillCooldownTime[m_WeaponSkill] > 0)
                    {
                        m_WeaponSkillCooldownTime[m_WeaponSkill] -= 0.5f;
                        if (m_WeaponSkillCooldownTime[m_WeaponSkill] <= 0.0f)
                        {
                            m_WeaponSkillState[m_WeaponSkill] = SkillState.ready;
                        }
                    }
                    break;
            }
        }

        private void OnSkillCooldownCompleted(object sender, SkillEventArgs args)
        {
            //Debug.Log("OnSkillCooldownCompleted");
            Debug.Log(args.skill + " SkillState: " + SkillState.ready);
            m_SkillStates[args.skill] = SkillState.ready;
        }

        void SkillActivation()
        {
            if (m_CurrentSkillIndex == -1)
            {
                CancelSkillActivation();
                return;
            }

            //m_Fighter.SetHasHitTarget(true);
            Skill skillToActivate;
            if (m_CurrentSkillIndex >= m_Skills.Count)
            {
                skillToActivate = m_WeaponSkill;

                if (m_Fighter.m_Target)
                {
                    //Skill skillToActivate = m_Skills[m_CurrentSkillIndex];
                    skillToActivate.Activate(gameObject, GetComponent<BaseStats>().GetSkillStat(
                            m_WeaponSkill.m_Skill, Stat.Damage,
                            m_WeaponSkill.m_Attribute, m_Fighter.m_Target.gameObject),
                        m_WeaponSkill.m_Effects, m_Fighter.m_EnemyTags, Vector3.zero, m_Fighter.m_Target);
                    //Debug.Log("Activate: " + skillToActivate + " on target: " + m_Fighter.m_Target);
                }
                else
                {
                    //Skill skillToActivate = m_Skills[m_CurrentSkillIndex];
                    skillToActivate.Activate(gameObject, GetComponent<BaseStats>().GetSkillStat(
                            m_WeaponSkill.m_Skill, Stat.Damage,
                            m_WeaponSkill.m_Attribute, gameObject), m_WeaponSkill.m_Effects,
                        m_Fighter.m_EnemyTags, Vector3.zero);
                    //Debug.Log("Activate: " + skillToActivate + " without a target.");

                }
            }
            else
            {
                skillToActivate = m_Skills[m_CurrentSkillIndex];


                if (skillToActivate.IsASupportiveSkill && m_Fighter.m_AllyTarget && !m_Fighter.m_AllyTarget.IsDead())
                {
                    skillToActivate.Activate(gameObject, GetComponent<BaseStats>().GetSkillStat(
                            m_Skills[m_CurrentSkillIndex].m_Skill, Stat.Damage,
                            m_Skills[m_CurrentSkillIndex].m_Attribute, m_Fighter.m_AllyTarget.gameObject),
                        m_Skills[m_CurrentSkillIndex].m_Effects, m_Fighter.m_AllyTags, Vector3.zero,
                        m_Fighter.m_AllyTarget);
                }
                else if (skillToActivate.IsASupportiveSkill && m_Fighter.m_AllyTarget == null &&
                         !GetComponent<Health>().IsDead())
                {
                    skillToActivate.Activate(gameObject, GetComponent<BaseStats>().GetSkillStat(
                            m_Skills[m_CurrentSkillIndex].m_Skill, Stat.Damage,
                            m_Skills[m_CurrentSkillIndex].m_Attribute, gameObject),
                        m_Skills[m_CurrentSkillIndex].m_Effects,
                        m_Fighter.m_EnemyTags, Vector3.zero);
                }
                else if (m_Fighter.m_Target)
                {
                    //Skill skillToActivate = m_Skills[m_CurrentSkillIndex];
                    skillToActivate.Activate(gameObject, GetComponent<BaseStats>().GetSkillStat(
                            m_Skills[m_CurrentSkillIndex].m_Skill, Stat.Damage,
                            m_Skills[m_CurrentSkillIndex].m_Attribute, m_Fighter.m_Target.gameObject),
                        m_Skills[m_CurrentSkillIndex].m_Effects, m_Fighter.m_EnemyTags, Vector3.zero,
                        m_Fighter.m_Target);
                    //Debug.Log("Activate: " + skillToActivate + " on target: " + m_Fighter.m_Target);
                }
                else
                {
                    //Skill skillToActivate = m_Skills[m_CurrentSkillIndex];
                    skillToActivate.Activate(gameObject, GetComponent<BaseStats>().GetSkillStat(
                            m_Skills[m_CurrentSkillIndex].m_Skill, Stat.Damage,
                            m_Skills[m_CurrentSkillIndex].m_Attribute, gameObject),
                        m_Skills[m_CurrentSkillIndex].m_Effects,
                        m_Fighter.m_EnemyTags, Vector3.zero);
                    //Debug.Log("Activate: " + skillToActivate + " without a target.");

                }
            }
        }
    }

}
