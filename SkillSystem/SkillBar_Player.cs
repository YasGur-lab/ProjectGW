using System;
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Statistics;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector3 = UnityEngine.Vector3;

namespace GW.Control
{
    public class SkillBar_Player : SkillBar_V2
    {
        [SerializeField] SkillBarInputs m_SkillBarInputs;
        //private Dictionary<Skill, KeyCode> m_KeyCodes = new Dictionary<Skill, KeyCode>();
        [SerializeField] private CastingDisplay m_CastingDisplay; 
        UI_SkillBar m_SkillBarUI;
        TerrainMaterialUpdater m_TerrainMaterialUpdater;
        public Vector3 m_SkillToMousePos { get; set; }

        [SerializeField] WarningDisplay m_WarningDisplay;

        [SerializeField] private LayerMask m_MouseLayerMask;
        private bool m_LoopAutoAttack;
        private bool m_GroundTargetVisualsActive;
        private Skill m_GroundTargetVisualsSkill;

        private Skill m_SkillInQueue;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            //if (gameObject.tag != "Player") return;

            SetWeaponSkill(m_Fighter.GetWeapon().GetSkill());

            m_TerrainMaterialUpdater = FindAnyObjectByType<TerrainMaterialUpdater>();
            m_SkillBarUI = FindObjectOfType<UI_SkillBar>();

            int index = 0;
            m_Skills = new List<Skill>(8);
            for (int i = 0; i < 8; i++)
            {
                m_Skills.Add(null);
            }

            if (m_SkillSet.skills.Count > 0)
            {
                SkillSet skillList = Instantiate(m_SkillSet);
                foreach (var skill in skillList.GetSkills)
                {
                    if (skill == null) continue;
                    m_SkillBarUI.CreateIconAndAddSkillToSkillBar(skill, index);
                    index++;
                }
                m_SkillBarUI.Coroutine_UpdateSkillBarSkills();
            }

            if (m_WeaponSkill)
            {
                m_WeaponSkillCastTime[m_WeaponSkill] = m_WeaponSkill.GetCastTime();
                m_WeaponSkillActiveTime[m_WeaponSkill] = 0;
                m_WeaponSkillCooldownTime[m_WeaponSkill] = 0;
                m_WeaponSkillState[m_WeaponSkill] = SkillState.ready;
            }

            float cooldownUpdateInterval = 0.02f; // Update every 0.1 seconds (adjust as needed)
            InvokeRepeating("UpdatePlayerCooldowns", cooldownUpdateInterval, cooldownUpdateInterval);
        }

        private void Update()
        {
            if (m_GroundTargetVisualsActive)
            {
                if (IsSkillPosInRange(m_GroundTargetVisualsSkill.GetRange()))
                    m_TerrainMaterialUpdater.SetMouse(GetMousePos(), m_GroundTargetVisualsSkill.GetRadius(), Color.green, m_GroundTargetVisualsSkill.VisualShape, m_GroundTargetVisualsSkill.GetMouseDirectionFromInstigator(gameObject.transform.position, GetMousePos()), m_GroundTargetVisualsSkill.AimSpreadAngle);
                else
                    m_TerrainMaterialUpdater.SetMouse(GetMousePos(), m_GroundTargetVisualsSkill.GetRadius(), Color.red, m_GroundTargetVisualsSkill.VisualShape, m_GroundTargetVisualsSkill.GetMouseDirectionFromInstigator(gameObject.transform.position, GetMousePos()), m_GroundTargetVisualsSkill.AimSpreadAngle);
            }

            if (m_LoopAutoAttack && m_WeaponSkillState[m_WeaponSkill] == SkillState.ready && m_CurrentSkillIndex < 0 && m_Fighter.m_Target)
            {
                m_CurrentCastingTime = 0.0f;
                m_CurrentSkillIndex = m_Skills.Count;
                m_WeaponSkillActiveTime[m_WeaponSkill] = m_WeaponSkill.GetActiveTime();
                ActivateSkill(m_WeaponSkill);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelSkillActivation();
                SetLoopAutoAttack(false);
                //m_TerrainMaterialUpdater.ResetMouse();
            }
        }


        public void ActivateSkill(int skillIndex)
        {
            Skill skill = m_Skills[skillIndex];
            switch (m_SkillStates[skill])
            {
                case SkillState.ready:
                    if (m_CurrentSkillIndex < 0)
                    {
                        SetLoopAutoAttack(false);
                        //CancelSkillActivation();
                        if (skill.IsGroundTarget && !IsSkillPosInRange(skill.GetRange()))
                        {
                            m_WarningDisplay.UpdateWarningText(WarningMessages.OutOfRange);
                            m_TerrainMaterialUpdater.ResetMouse();
                            break;
                        }
                        m_TerrainMaterialUpdater.ResetMouse();
                        m_CurrentCastingTime = 0.0f;
                        m_CurrentSkillIndex = m_Skills.IndexOf(skill);
                        m_ActiveTimes[skill] = skill.GetActiveTime();
                        m_GroundTargetVisualsActive = false;
                        ActivateSkill(skill);
                    }
                    else if(m_SkillInQueue ==  null)
                    {
                        m_SkillInQueue = skill;
                    }
                    break;
            }
        }

        public void ActivateGroundTargetVisuals(Skill skill)
        {
            if (!m_Energy.HasEnoughEnergy(skill.GetEnergyCost()))
            {
                m_WarningDisplay.UpdateWarningText(WarningMessages.NotEnoughEnergy);
                return;
            }
            if (GetSkillStates()[skill] != SkillState.ready)
            {
                m_WarningDisplay.UpdateWarningText(WarningMessages.SkillOnCooldown);
                return;
            }
            if (m_CurrentSkillIndex < 0 && skill.IsGroundTarget)
            {
                m_GroundTargetVisualsSkill = skill;
                m_GroundTargetVisualsActive = true;
            }
        }

        public void DeactivateGroundTargetVisuals()
        {
            m_TerrainMaterialUpdater.ResetMouse();
            m_GroundTargetVisualsSkill = null;
            m_GroundTargetVisualsActive = false;
        }

        public void UpdatePlayerCooldowns()
        {
            foreach (var skill in m_Skills)
            {
                if (skill == null) continue;
                switch (m_SkillStates[skill])
                {
                    case SkillState.cooldown:
                        if (m_CooldownTimes[skill] > 0)
                        {
                            m_CooldownTimes[skill] -= 0.02f;
                            m_SkillBarUI.SkillsCooldownUpdated?.Invoke(this, new SkillEventArgs(skill, m_CooldownTimes[skill]));
                            if (m_CooldownTimes[skill] <= 0.0f)
                            {
                                m_SkillStates[skill] = SkillState.ready;
                                m_SkillBarUI.SkillsCooldownReset?.Invoke(this, new SkillEventArgs(skill, m_CooldownTimes[skill]));
                            }
                        }
                        break;
                }
            }

            //Debug.Log(m_WeaponSkillState[m_WeaponSkill]);

            switch (m_WeaponSkillState[m_WeaponSkill])
            {
                case SkillState.cooldown:
                    if (m_WeaponSkillCooldownTime[m_WeaponSkill] > 0)
                    {
                        m_WeaponSkillCooldownTime[m_WeaponSkill] -= 0.02f;
                        if (m_WeaponSkillCooldownTime[m_WeaponSkill] <= 0.0f)
                        {
                            m_WeaponSkillState[m_WeaponSkill] = SkillState.ready;
                        }

                    }
                    break;
            }

            if (!m_Fighter.IsCasting())
            {
                if (m_CastingDisplay.GetSlider().gameObject.activeSelf)
                    m_CastingDisplay.GetSlider().gameObject.SetActive(false);
                return;
            }

            if (m_CurrentSkillIndex < 0) return;

            //COOLDOWNS
            m_CurrentCastingTime += 0.02f;

            //CASTING BAR
            if (!m_CastingDisplay.GetSlider().gameObject.activeSelf)
                m_CastingDisplay.GetSlider().gameObject.SetActive(true);

            if (m_CurrentSkillIndex < m_Skills.Count)
            {
                m_CastingDisplay.UpdateCastingBar(m_CurrentCastingTime, m_Skills[m_CurrentSkillIndex]);
            }
        }

        public void InitSkill(Skill skillToAdd, int skillBarIndex)
        {
            m_Skills[skillBarIndex] = skillToAdd;

            m_CastTimes[skillToAdd] = skillToAdd.GetCastTime();
            m_ActiveTimes[skillToAdd] = 0;
            m_CooldownTimes[skillToAdd] = 0;
            m_SkillStates[skillToAdd] = SkillState.ready;
            //m_KeyCodes[skillToAdd] = m_SkillBarInputs.GetInputs[skillBarIndex];
        }

        void SkillActivation()
        {
            if (m_CurrentSkillIndex == -1)
            {
                CancelSkillActivation();
                return;
            }

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
                //m_Fighter.SetHasHitTarget(true);
                skillToActivate = m_Skills[m_CurrentSkillIndex];
                if (skillToActivate.IsASupportiveSkill && m_Fighter.m_AllyTarget && !m_Fighter.m_AllyTarget.IsDead())
                {
                    skillToActivate.Activate(gameObject, GetComponent<BaseStats>().GetSkillStat(
                            m_Skills[m_CurrentSkillIndex].m_Skill, Stat.Damage,
                            m_Skills[m_CurrentSkillIndex].m_Attribute, m_Fighter.m_AllyTarget.gameObject),
                        m_Skills[m_CurrentSkillIndex].m_Effects, m_Fighter.m_AllyTags, m_SkillToMousePos,
                        m_Fighter.m_AllyTarget);
                }
                else if (skillToActivate.IsASupportiveSkill && m_Fighter.m_AllyTarget == null &&
                         !GetComponent<Health>().IsDead())
                {
                    skillToActivate.Activate(gameObject, GetComponent<BaseStats>().GetSkillStat(
                            m_Skills[m_CurrentSkillIndex].m_Skill, Stat.Damage,
                            m_Skills[m_CurrentSkillIndex].m_Attribute, gameObject),
                        m_Skills[m_CurrentSkillIndex].m_Effects,
                        m_Fighter.m_EnemyTags, m_SkillToMousePos);
                }
                else if (m_Fighter.m_Target)
                {
                    //Skill skillToActivate = m_Skills[m_CurrentSkillIndex];
                    skillToActivate.Activate(gameObject, GetComponent<BaseStats>().GetSkillStat(
                            m_Skills[m_CurrentSkillIndex].m_Skill, Stat.Damage,
                            m_Skills[m_CurrentSkillIndex].m_Attribute, m_Fighter.m_Target.gameObject),
                        m_Skills[m_CurrentSkillIndex].m_Effects, m_Fighter.m_EnemyTags, m_SkillToMousePos,
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
                        m_Fighter.m_EnemyTags, m_SkillToMousePos);
                    //Debug.Log("Activate: " + skillToActivate + " without a target.");
                }
            }
        }

        protected override void ReenableAutoAttack()
        {
            //SetLoopAutoAttack(true);
        }

        private Vector3 GetMousePos()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.CompareTag("Terrain") || hit.transform.CompareTag("Armor"))
                {
                    m_SkillToMousePos = hit.point;
                    return hit.point;
                }
            }
            return Vector3.zero;
        }

        private bool IsSkillPosInRange(float range)
        {
            float distance = Vector3.Distance(gameObject.transform.position, GetMousePos());
            if (distance <= range) return true;
            return false;
        }

        public void SetLoopAutoAttack(bool b) { m_LoopAutoAttack = b; }
    }
}

