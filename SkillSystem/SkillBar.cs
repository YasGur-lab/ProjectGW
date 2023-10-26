//using System;
//using System.Collections;
//using System.Collections.Generic;
//using GW.Attributes;
//using GW.Combat;
//using GW.Movement;
//using GW.Statistics;
//using TMPro;
//using UnityEditor.Experimental.GraphView;
//using UnityEngine;

//namespace GW.Control
//{
//    public class SkillBar : MonoBehaviour
//    {
//        [SerializeField] private List<KeyCode> m_KeyCodesSetup;

//        private Dictionary<Skill, KeyCode> m_KeyCodes = new Dictionary<Skill, KeyCode>();
//        [SerializeField] private List<Skill> m_Skills = new List<Skill>(8);
//        private List<Skill> skillsToProcess = new List<Skill>();

//        private Dictionary<Skill, float> m_CastTimes = new Dictionary<Skill, float>();
//        private Dictionary<Skill, float> m_ActiveTimes = new Dictionary<Skill, float>();
//        private Dictionary<Skill, float> m_CooldownTimes = new Dictionary<Skill, float>();
//        private Dictionary<Skill, SkillState> m_SkillStates = new Dictionary<Skill, SkillState>();

//        private int m_CurrentSkillIndex;
//        //private bool m_IsCasting;
//        //private bool m_IsCasting;
//        [SerializeField] private float m_AngleRequieredToUseSkills;
//        [SerializeField] private CastingDisplay m_CastingDisplay;
//        [SerializeField] private WarningDisplay m_WarningDisplay;
//        [SerializeField] private TextMeshProUGUI[] m_SkillTexts;

//        private float m_CurrentCastingTime;
//        private const float m_RegargeTimeIfCanceled = 0.25f;

//        private Fighter m_Fighter;
//        private bool m_SkillActivated;

//        private SkillAnimationController m_SkillAnimationController;

//        public enum SkillState
//        {
//            ready,
//            active,
//            cooldown
//        }

//        // Start is called before the first frame update
//        void Start()
//        {
//            m_Fighter = GetComponent<Fighter>();
//            m_SkillAnimationController = GetComponent<SkillAnimationController>();
//            m_CurrentSkillIndex = -1;

//            if (gameObject.tag == "Player")
//            {
//                m_Skills = new List<Skill>(8);
//                for (int i = 0; i < 8; i++)
//                {
//                    m_Skills.Add(null);
//                }
//            }

//            if (m_Skills.Count > 0)
//            {

//                // Initialize the dictionaries
//                foreach (Skill skill in m_Skills)
//                {
//                    if(skill == null)  continue;

//                    m_CastTimes[skill] = skill.GetCastTime();
//                    m_ActiveTimes[skill] = 0;
//                    m_CooldownTimes[skill] = 0;
//                    m_SkillStates[skill] = SkillState.ready;
//                    m_KeyCodes[skill] = KeyCode.Alpha0;
//                }

//                for (int i = 0; i < m_KeyCodesSetup.Count; i++)
//                {
//                    if (m_Skills[i] == null) continue;
//                    m_KeyCodes[m_Skills[i]] = m_KeyCodesSetup[i];
//                }
//            }
//        }

//        // Update is called once per frame
//        void Update()
//        {
//            //UpdateSkillUI();

//            //ACTIVATING SKILL
//            for (int i = 0; i < m_Skills.Count; i++)
//            {
//                if (m_Skills[i] == null) continue;

//                switch (m_SkillStates[m_Skills[i]])
//                {
//                    case SkillState.ready:
//                            if (m_CurrentSkillIndex < 0)
//                        {
//                            if (GetCurrentKey() == m_KeyCodes[m_Skills[i]])
//                            {
//                                m_CurrentCastingTime = 0.0f;
//                                m_CurrentSkillIndex = i;
//                                m_SkillStates[m_Skills[i]] = SkillState.active;
//                                m_ActiveTimes[m_Skills[i]] = m_Skills[i].GetActiveTime();
//                                StartCoroutine(Activate(m_Skills[i]));
//                                i = -1;
//                            }
//                        }

//                        break;
                        
//                    case SkillState.active:
//                        if (m_ActiveTimes[m_Skills[i]] > 0)
//                        {
//                            m_ActiveTimes[m_Skills[i]] -= Time.deltaTime;
//                        }
//                        else
//                        {
//                            m_SkillStates[m_Skills[i]] = SkillState.cooldown;
//                        }

//                        break;
//                    case SkillState.cooldown:
//                        if (m_CooldownTimes[m_Skills[i]] > 0)
//                        {
//                            m_CooldownTimes[m_Skills[i]] -= Time.deltaTime;
//                        }
//                        else
//                        {
//                            m_SkillStates[m_Skills[i]] = SkillState.ready;
//                        }

//                        break;
//                }
//            }

//            if (gameObject.tag != "Player") return;
//            if (!m_Fighter.IsCasting())
//            {
//                if (m_CastingDisplay.GetSlider().gameObject.activeSelf)
//                    m_CastingDisplay.GetSlider().gameObject.SetActive(false);
//                return;
//            }

//            if (m_CurrentSkillIndex < 0) return;

//            //COOLDOWNS
//            m_CurrentCastingTime += Time.deltaTime;

//            //CASTING BAR

//            if (!m_CastingDisplay.GetSlider().gameObject.activeSelf)
//                m_CastingDisplay.GetSlider().gameObject.SetActive(true);
//            m_CastingDisplay.UpdateCastingBar(m_CurrentCastingTime, m_Skills[m_CurrentSkillIndex]);
//        }

//        //private void UpdateSkillUI()
//        //{
//        //    for (int i = 0; i < m_Skills.Count; i++)
//        //    {
//        //        if (m_Skills[i] == null) continue;

//        //        switch (m_SkillStates[m_Skills[i]])
//        //        {
//        //            case SkillState.ready:
//        //                m_SkillTexts[i].text = "";
//        //                break;
//        //            case SkillState.cooldown:
//        //                m_SkillTexts[i].text = $"{Mathf.Ceil(m_CooldownTimes[m_Skills[i]])}";
//        //                break;
//        //        }
//        //    }
//        //}

//        public void ActivateSKill(Skill skill)
//        {
//            m_CurrentCastingTime = 0.0f;
//            m_CurrentSkillIndex = m_Skills.IndexOf(skill);
//            m_SkillStates[m_Skills[m_CurrentSkillIndex]] = SkillState.cooldown;
//            m_ActiveTimes[m_Skills[m_CurrentSkillIndex]] = skill.GetActiveTime();

//            StartCoroutine(Activate(m_Skills[m_CurrentSkillIndex]));
//        }

//        private IEnumerator Activate(Skill skill)
//        {
//            //PREVENTIONS
//            m_SkillActivated = false;

//            if (skill.DoesRequireTarget())
//            {
//                if (IsTargetDead())
//                {
//                    m_WarningDisplay.UpdateWarningText(WarningMessages.NoTargetAvailable);
//                    m_CooldownTimes[skill] = m_RegargeTimeIfCanceled;
//                    m_CurrentSkillIndex = -1;
//                    yield break;
//                }

//                if (!m_Fighter.m_Target.gameObject.transform)
//                {
//                    m_WarningDisplay.UpdateWarningText(WarningMessages.NoTargetAvailable);
//                    m_CooldownTimes[skill] = m_RegargeTimeIfCanceled;
//                    m_CurrentSkillIndex = -1;
//                    yield break;
//                }

//                if (!IsInRangeOfTarget())
//                {
//                    m_WarningDisplay.UpdateWarningText(WarningMessages.OutOfRange);

//                    m_CooldownTimes[skill] = m_RegargeTimeIfCanceled;
//                    m_CurrentSkillIndex = -1;
//                    yield break;
//                }
//            }

//            int cost = skill.GetEnergyCost();

//            if (gameObject.tag == "Player" && !GetComponent<Energy>().HasEnoughEnergy(cost))
//            {
//                m_WarningDisplay.UpdateWarningText(WarningMessages.NotEnoughEnergy);
//                m_CurrentSkillIndex = -1;
//                yield break;
//            }
//            else if(!GetComponent<Energy>().HasEnoughEnergy(cost))
//            {
//                m_CurrentSkillIndex = -1;
//                yield break;
//            }

//            //SETTERS
//            m_Fighter.SetInCombatStance(skill.HasFullBodyAnimation());
//            m_Fighter.SetHasHitTarget(false);
//            m_SkillAnimationController.StopAttack();
//            GetComponent<Energy>().ReduceEnergy(cost);

//            //ANIMATIONS
//            m_SkillAnimationController.TriggerSkillAnimation();
//            GetComponent<SkillAnimationController>().SetAnimatorController(skill.GetCastTime(), skill.GetAnimation());
            
//            if (skill.DoesRequireTarget() && !m_Fighter.m_Target.gameObject.transform)
//            {
//                m_WarningDisplay.UpdateWarningText(WarningMessages.NoTargetAvailable);
//                m_CurrentSkillIndex = -1;
//                m_CooldownTimes[skill] = m_RegargeTimeIfCanceled;
//                yield break;
//            }
            

//            if (!m_Fighter.IsCasting()) yield break;
//            yield return new WaitUntil(() => m_SkillActivated);

//            //RESETS
//            m_Fighter.CancelCurrentSkill();
//            m_SkillAnimationController.StopSkillAnimations();

//            GetSkillStates()[skill] = SkillState.cooldown;
//            GetCooldownTimes()[skill] = GetEquipedSkills()[GetCurrentSkillIndex()].GetCooldownTime();
//            SetCurrentSkillIndex(-1);


//            //autoattack after skill
//            if (skill.DoesRequireTarget() && m_Fighter.tag == "Player")
//            {
//                m_SkillAnimationController.TriggerAttack();
//                m_Fighter.Attack(m_Fighter.m_Target.gameObject);
//            }
//        }

//        //void SkillActivation()
//        //{
//        //    if (m_CurrentSkillIndex == -1) return;
//        //    m_Fighter.SetHasHitTarget(true);

//        //    if (m_Skills[m_CurrentSkillIndex].DoesRequireTarget() && !m_Fighter.m_Target.gameObject.transform)
//        //    {
//        //        m_WarningDisplay.UpdateWarningText(WarningMessages.NoTargetAvailable);
//        //        m_CurrentSkillIndex = -1;
//        //        return;
//        //    }

//        //    //Debug.Log("Casting skill " + m_Skills[m_CurrentSkillIndex].name);

//        //    if (m_Skills[m_CurrentSkillIndex].DoesRequireTarget())
//        //    {
//        //        m_Skills[m_CurrentSkillIndex].Activate(gameObject,
//        //            m_Fighter.m_Target, //gameObject.GetComponent<TargetSystem>().GetTarget().GetComponent<Health>(),
//        //            GetComponent<BaseStats>().GetSkillStat(m_Skills[m_CurrentSkillIndex].m_Skill, Stat.Damage,
//        //                m_Skills[m_CurrentSkillIndex].m_Attribute, m_Fighter.m_Target.gameObject),
//        //            m_Skills[m_CurrentSkillIndex].m_Effects);
//        //    }
//        //    else
//        //    {
//        //        m_Skills[m_CurrentSkillIndex].Activate(gameObject,
//        //            GetComponent<Health>(), //gameObject.GetComponent<TargetSystem>().GetTarget().GetComponent<Health>(),
//        //            GetComponent<BaseStats>().GetSkillStat(m_Skills[m_CurrentSkillIndex].m_Skill, Stat.Damage,
//        //                m_Skills[m_CurrentSkillIndex].m_Attribute, gameObject),
//        //            m_Skills[m_CurrentSkillIndex].m_Effects);
//        //    }
//        //}

//        //void SkillCompletion()
//        //{
//        //    if (m_CurrentSkillIndex == -1) return;
            
//        //    if (!m_Fighter.m_Target && m_Skills[m_CurrentSkillIndex].DoesRequireTarget() && !m_Fighter.m_Target.gameObject.transform)
//        //    {
//        //        m_WarningDisplay.UpdateWarningText(WarningMessages.NoTargetAvailable);
//        //        m_CurrentSkillIndex = -1;
//        //        return;
//        //    }
//        //    m_SkillActivated = true;
//        //    m_Fighter.SetHasHitTarget(false);
//        //    m_Fighter.SetInCombatStance(false);
//        //}
        
//        public void InitSkill(Skill skillToAdd, int skillBarIndex)
//        {
//            m_Skills[skillBarIndex] = skillToAdd;
            
//            m_CastTimes[skillToAdd] = skillToAdd.GetCastTime();
//            m_ActiveTimes[skillToAdd] = 0;
//            m_CooldownTimes[skillToAdd] = 0;
//            m_SkillStates[skillToAdd] = SkillState.ready;
//            m_KeyCodes[skillToAdd] = m_KeyCodesSetup[skillBarIndex];
//        }

//        public bool DoesContains(Skill skillToAdd) => m_Skills.Contains(skillToAdd);
//        public int GetCurrentSkillIndex() => m_CurrentSkillIndex;
//        public void SetCurrentSkillIndex(int i) { m_CurrentSkillIndex = i; }
//        public List<Skill> GetEquipedSkills() { return m_Skills; }
//        public Dictionary<Skill, SkillState> GetSkillStates() { return m_SkillStates; }
//        public Dictionary<Skill, float> GetCooldownTimes() { return m_CooldownTimes; }

//        private void LookAtTarget(Transform targetPosition)
//        {
//            Vector3 direction = targetPosition.position - transform.position;
//            Quaternion targetRotation = Quaternion.LookRotation(direction);
//            transform.rotation = targetRotation;
//        }

//        private bool IsPlayerLookingInDirectionOfTarget(Transform targetTransform)
//        {
//            Vector3 directionToTarget = targetTransform.position - transform.position;
//            float angle = Vector3.Angle(transform.forward, directionToTarget);

//            if (angle > m_AngleRequieredToUseSkills)
//            {
//                //Debug.Log("player is not looking at target. The angle is: " + angle);
//                return false;
//            }

//            return true;
//        }

//        private KeyCode GetCurrentKey()
//        {
//            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
//            {
//                if (Input.GetKeyDown(keyCode))
//                {
//                    return keyCode;
//                }
//            }

//            return KeyCode.None;
//        }

//        public SkillState GetSkillState(Skill skill) => m_SkillStates[skill];
        
//        private bool IsInRangeOfTarget()
//        {
//            float distance = Vector3.Distance(transform.position,
//                m_Fighter.m_Target.gameObject.transform.position);
//            if (distance <= GetComponent<Fighter>().GetCastingRange())
//                return true;
//            else return false;
//        }

//        private bool IsTargetDead()
//        {
//            return m_Fighter.m_Target.gameObject.GetComponent<Health>().IsDead();
//        }

//        public float GetCurrentCastingTime() => m_CurrentCastingTime;
//        public Skill GetCurrentSkill() => m_Skills[m_CurrentSkillIndex];

//        public bool IsActivatingSkill()
//        {
//            if(m_CurrentSkillIndex == -1) return false;
//            return true;
//        }

//        public int GetIndexOfSkill(Skill skill) => m_Skills.IndexOf(skill);
//    }
//}
