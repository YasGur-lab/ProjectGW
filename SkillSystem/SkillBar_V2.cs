using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using UnityEngine;
using static SkillEventArgs;

namespace GW.Control
{
    public class SkillBar_V2 : MonoBehaviour
    {
        public event SkillActivatedEventHandler SkillActivated;
        //public event SkillActivatedEventHandler SkillCompleted;

        [SerializeField] protected SkillSet m_SkillSet;
        protected List<Skill> m_Skills;
        protected Skill m_WeaponSkill;

        protected Dictionary<Skill, float> m_CastTimes = new Dictionary<Skill, float>();
        protected Dictionary<Skill, float> m_ActiveTimes = new Dictionary<Skill, float>();
        protected Dictionary<Skill, float> m_CooldownTimes = new Dictionary<Skill, float>();
        protected Dictionary<Skill, SkillBar_V2.SkillState> m_SkillStates = new Dictionary<Skill, SkillBar_V2.SkillState>();

        protected Dictionary<Skill, float> m_WeaponSkillCastTime = new Dictionary<Skill, float>();
        protected Dictionary<Skill, float> m_WeaponSkillActiveTime = new Dictionary<Skill, float>();
        protected Dictionary<Skill, float> m_WeaponSkillCooldownTime = new Dictionary<Skill, float>();
        protected Dictionary<Skill, SkillBar_V2.SkillState> m_WeaponSkillState = new Dictionary<Skill, SkillBar_V2.SkillState>();

        protected int m_CurrentSkillIndex;

        protected float m_CurrentCastingTime;
        private const float m_RegargeTimeIfCanceled = 0.25f;

        protected Fighter m_Fighter;
        private bool m_SkillActivated;
        private bool m_SkillIsCompleted;
        protected SkillAnimationController m_SkillAnimationController;
        protected Energy m_Energy;

        public enum SkillState
        {
            ready,
            active,
            cooldown
        }

        protected virtual void Start()
        {
            m_SkillStates.Clear();
            m_CastTimes.Clear();
            m_ActiveTimes.Clear();
            m_CooldownTimes.Clear();
            m_Skills = new List<Skill>();
            m_CurrentSkillIndex = -1;

            m_Fighter = GetComponent<Fighter>();
            m_SkillAnimationController = GetComponent<SkillAnimationController>();
            m_CurrentSkillIndex = -1;
            m_SkillIsCompleted = true;

            m_Energy = GetComponent<Energy>();
        }
        
        public void ActivateSkill(Skill skill)
        {
            if (skill != m_WeaponSkill)
            {
                if (!m_Skills.Contains(skill))
                {
                    Debug.Log("Skill " + skill.name + " doesn't exist in skill bar");
                    return;
                }


                SkillActivated = null;
                SkillActivated += OnSkillActivated;

                if (m_SkillStates[skill] == SkillState.ready)
                {
                    m_CastTimes[skill] = skill.GetCastTime();
                    m_CooldownTimes[skill] = skill.GetCooldownTime(); // Set the cooldown time here
                    SkillActivated?.Invoke(this, new SkillEventArgs(skill));

                }
            }
            else
            {
                SkillActivated = null;
                SkillActivated += OnSkillActivated;

                if (m_WeaponSkillState[skill] == SkillState.ready)
                {
                    m_WeaponSkillCastTime[skill] = skill.GetCastTime();
                    m_WeaponSkillCooldownTime[skill] = skill.GetCooldownTime(); // Set the cooldown time here
                    SkillActivated?.Invoke(this, new SkillEventArgs(skill));

                }
            }
        }

        protected IEnumerator Activate(Skill skill)
        {
            //SETTERS
            m_SkillIsCompleted = false;
            m_SkillActivated = false;
            m_Fighter.SetHasHitTarget(false);

            //SKILL COST
            int cost = skill.GetEnergyCost();
            if (!GetComponent<Energy>().HasEnoughEnergy(cost))
            {
                CancelSkillActivation();
                yield break;
            }

            m_Energy.ReduceEnergy(cost);

            //ANIMATIONS

            //m_Fighter.SetInCombatStance(skill.HasFullBodyAnimation(), !skill.IsASupportiveSkill);
            m_Fighter.SetSkillAnimationLayer(skill.HasFullBodyAnimation());
            GetComponent<SkillAnimationController>().SetAnimatorController(skill.GetCastTime(), skill.GetAnimation());
            m_SkillAnimationController.TriggerSkillAnimation();

            if (m_CurrentSkillIndex == -1)
            {
                CancelSkillActivation();
                yield break;
            }

            yield return new WaitUntil(() => m_SkillActivated);

            //RESETS
            // Debug.Log("Request: OnSkillCompleted");
            //SkillCompleted?.Invoke(this, new SkillEventArgs(skill));
            SkillCompleted();
        }

        void SkillCompletion()
        {
            if (m_CurrentSkillIndex == -1) return;

            m_SkillActivated = true;
            m_Fighter.SetHasHitTarget(false);
            ReenableAutoAttack();
        }

        protected virtual void ReenableAutoAttack()
        {
            
        }

        private void OnSkillActivated(object sender, SkillEventArgs args)
        {
            m_CurrentCastingTime = 0.0f;
            if (args.skill == m_WeaponSkill)
            {
                m_WeaponSkillState[args.skill] = SkillState.active;
                m_WeaponSkillActiveTime[args.skill] = args.skill.GetActiveTime();
                m_CurrentSkillIndex = m_Skills.Count;
            }
            else
            {
                m_SkillStates[args.skill] = SkillState.active;
                m_ActiveTimes[args.skill] = args.skill.GetActiveTime();
                m_CurrentSkillIndex = m_Skills.IndexOf(args.skill);
            }

            StartCoroutine(Activate(args.skill));
        }
        
        private void SkillCompleted()
        {
            if (m_CurrentSkillIndex >= 0 && m_CurrentSkillIndex < m_Skills.Count)
            {
                m_Fighter.SetHasHitTarget(false);
                m_SkillAnimationController.StopSkillAnimations();

                m_SkillStates[m_Skills[m_CurrentSkillIndex]] = SkillState.cooldown;
                m_CooldownTimes[m_Skills[m_CurrentSkillIndex]] = m_Skills[m_CurrentSkillIndex].GetCooldownTime();
                SetCurrentSkillIndex(-1);
                //m_SkillAnimationController.TriggerAttack();
                m_Fighter.ResetLayersWeight();
                StartCoroutine(SkillIsCompleted());
            }
            else
            {
                m_Fighter.SetHasHitTarget(false);
                m_SkillAnimationController.StopSkillAnimations();

                m_WeaponSkillState[m_WeaponSkill] = SkillState.cooldown;
                m_WeaponSkillCooldownTime[m_WeaponSkill] = m_WeaponSkill.GetCooldownTime();
                SetCurrentSkillIndex(-1);
                m_Fighter.ResetLayersWeight();

                //m_SkillAnimationController.TriggerAttack();
                StartCoroutine(SkillIsCompleted());
            }
        }

        private IEnumerator SkillIsCompleted()
        {
            yield return new WaitForSeconds(0.5f);
            m_SkillIsCompleted = true;
            //Debug.Log("OnSkillCompleted");
        }

        private void OnSkillCooldownCompleted(object sender, SkillEventArgs args)
        {
            //Debug.Log("OnSkillCooldownCompleted");
            Debug.Log(args.skill+ " SkillState: " + SkillState.ready);
            m_SkillStates[args.skill] = SkillState.ready;
        }
        public void CancelSkillActivation()
        {
            if (m_CurrentSkillIndex != -1)
            {
                if (m_CurrentSkillIndex != m_Skills.Count)
                {
                    m_SkillAnimationController.StopSkillAnimations();
                    m_CooldownTimes[m_Skills[m_CurrentSkillIndex]] = 0.0f;
                    m_SkillStates[m_Skills[m_CurrentSkillIndex]] = SkillState.ready;
                    SetCurrentSkillIndex(-1);
                    m_SkillIsCompleted = true;
                }
                else
                {
                    m_SkillAnimationController.StopSkillAnimations();
                    m_WeaponSkillCooldownTime[m_WeaponSkill] = 0.0f;
                    m_WeaponSkillState[m_WeaponSkill] = SkillState.ready;
                    SetCurrentSkillIndex(-1);
                    m_SkillIsCompleted = true;
                }
            }
        }

        public void SetWeaponSkill(Skill skill) => m_WeaponSkill = skill;
        public Skill GetWeaponSkill() => m_WeaponSkill;
        public Dictionary<Skill, SkillState> GetSkillStates() { return m_SkillStates; }
        public Dictionary<Skill, float> GetCooldownTimes() { return m_CooldownTimes; }
        public List<Skill> GetEquipedSkills() { return m_Skills; }
        public int GetCurrentSkillIndex() => m_CurrentSkillIndex;
        public void SetCurrentSkillIndex(int i) { m_CurrentSkillIndex = i; }
        private bool IsTargetDead() { return m_Fighter.m_Target.gameObject.GetComponent<Health>().IsDead(); }
        public float GetCurrentCastingTime() => m_CurrentCastingTime;
        public Skill GetSkillAtIndex(int index) => m_Skills[index];
        public void SetSkillSet(SkillSet mSkillSet) { m_SkillSet = mSkillSet; }
        public int FindSkillIndex(Skill skillToFind) { return m_Skills.FindIndex(skill => skill == skillToFind); }
        public bool DoesContains(Skill skillToAdd) => m_Skills.Contains(skillToAdd);
        public bool IsSkillCompleted() => m_SkillIsCompleted;
        public bool IsActivatingSkill()
        {
            if (m_CurrentSkillIndex == -1) return false;
            return true;
        }
        private bool IsInRangeOfTarget()
        {
            float distance = Vector3.Distance(transform.position,
                m_Fighter.m_Target.gameObject.transform.position);
            if (distance <= GetComponent<Fighter>().GetCastingRange())
                return true;
            else return false;
        }

        public Dictionary<Skill, SkillState> GetWeaponSkillState() { return m_WeaponSkillState; }
    }
}
