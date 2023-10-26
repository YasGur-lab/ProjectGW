using GW.Attributes;
using GW.Combat;
using UnityEngine;

namespace GW.Control
{
    public class SkillAnimationController : MonoBehaviour
    {
        [SerializeField] RuntimeAnimatorController m_Controller;
        [SerializeField] Avatar m_Avatar;
        Animator m_Animator;
        private Fighter m_Fighter;
        [SerializeField] private AnimatorOverrideController m_ProfessionCombatController = null;
        private RuntimeAnimatorController m_ProfessionNonCombatController = null;
        private float m_Speed;

        private void Awake()
        {
            if (gameObject.tag == "Player") return;
            Animator animatorController = gameObject.AddComponent<Animator>();
            //RuntimeAnimatorController cont = Instantiate(m_Controller);
            AnimatorOverrideController overrcont = Instantiate(m_ProfessionCombatController);
            m_Animator = GetComponent<Animator>();
            m_Animator.runtimeAnimatorController = m_Controller;
            m_Animator.avatar = m_Avatar;
            m_ProfessionCombatController = overrcont;
            m_ProfessionCombatController.runtimeAnimatorController = m_Animator.runtimeAnimatorController;
            m_ProfessionNonCombatController = m_Animator.runtimeAnimatorController;
        }

        private void Start()
        {
            m_Fighter = GetComponent<Fighter>();
            m_Animator = GetComponent<Animator>();
            m_Speed = m_Animator.speed;
            
        }
        
        public void TriggerAttack()
        {
            if (!m_Fighter.AttackRequested)
            {
                //Debug.Log("TriggerAttack");
                m_Animator.SetBool("IsAttacking", true);
                m_Fighter.AttackRequested = true;
            }
        }

        public void StopAttack()
        {
            if (m_Fighter.AttackRequested)
            {
                //Debug.Log("StopAttack");
                m_Animator.SetBool("IsAttacking", false);
                m_Fighter.AttackRequested = false;
            }
        }

        public void TriggerSkillAnimation()
        {
            if (!m_Fighter.IsCasting())
            {
                //Debug.Log("TriggerSkillAnimation");
                m_Animator.SetBool("IsCasting", true);
                m_Fighter.m_IsCasting = true;
            }
        }

        public void StopSkillCompletion()
        {
            if (m_Fighter.IsCasting())
            {
                m_Animator.SetBool("IsCasting", false);
                m_Fighter.m_IsCasting = false;
            }
        }

        public void TriggerDeathAnimation()
        {
            m_Fighter.SetOutOfCombatStance();
            ResetAnimatorControllerToProfessionOffCombat();
            m_Animator.SetTrigger("IsDeadTrigger");
            StopSkillCompletion();
            //StopSkillCasting();
            StopAttack();

            Invoke("DisableAnimator", 3.0f);
        }

        public void ResetCharacterAfterDeath()
        {
            //m_Animator.SetTrigger("IsDead");
            m_Animator.ResetTrigger("IsDeadTrigger");
            m_Animator.SetBool("IsDead", false);
        }

        private void DisableAnimator()
        {
            if(!gameObject.GetComponent<Health>().IsDead()) return;
            m_Animator.enabled = false;
            //gameObject.SetActive(false);
        }

        public void SetAnimatorController(float castTime, AnimationClip skillAnimation)
        {
            if (m_Animator == null)
            {
                Debug.LogError("No Animator component found on SkillAnimationController");
                return;
            }
            
            var overrideController = m_Animator.runtimeAnimatorController as AnimatorOverrideController;

            if (m_ProfessionCombatController != null)
            {
                m_ProfessionCombatController["2Hand-Sword-Attack8"] = skillAnimation;
                m_Animator.runtimeAnimatorController = m_ProfessionCombatController; 
            }
            else if (overrideController != null)
            {
                m_Animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
            }

            m_Animator.SetFloat("CastingSpeed", CalculateAnimationSpeed(castTime, skillAnimation.length));
        }

        public void PlayAutoAttackAnimation(float castTime)
        {
            if (m_Animator == null)
            {
                Debug.LogError("No Animator component found on SkillAnimationController");
                return;
            }
            
            var overrideController = m_Animator.runtimeAnimatorController as AnimatorOverrideController;

            if (m_ProfessionCombatController != null)
            {
                m_Animator.runtimeAnimatorController = m_ProfessionCombatController;
            }
            else if (overrideController != null)
            {
                m_Animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
            }

            m_Animator.SetFloat("AutoAttackSpeed", CalculateAnimationSpeed(castTime, m_ProfessionCombatController["Unarmed-Attack-L3"].length));
        }

        float CalculateAnimationSpeed(float skillCastTime, float animationTime)
        {
            float newAnimationSpeed = animationTime / skillCastTime;
            return newAnimationSpeed;
        }

        public void ResetAnimatorControllerToProfessionOffCombat()
        {
            m_Animator.speed = m_Speed;
            m_Animator.SetFloat("AutoAttackSpeed", CalculateAnimationSpeed(1.0f, 1.0f));
            m_Animator.SetFloat("CastingSpeed", CalculateAnimationSpeed(1.0f, 1.0f));
            m_Animator.runtimeAnimatorController = m_ProfessionNonCombatController;
        }

        public void ResetAnimatorCastingAndAttackingFloatSpeed()
        {
            //m_Animator.SetFloat("AutoAttackSpeed", CalculateAnimationSpeed(1.0f, 1.0f));
            m_Animator.SetFloat("CastingSpeed", CalculateAnimationSpeed(1.0f, 1.0f));
        }
        
        public void StopSkillAnimations()
        {
            ResetAnimatorCastingAndAttackingFloatSpeed();
            StopSkillCompletion();
            //StopSkillCasting();
            //Debug.Log("StopSkillAnimations");
        }
    }
}