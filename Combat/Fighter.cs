using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using GW.Attributes;
using GW.Control;
using GW.Core;
using GW.Movement;
using GW.Saving;
using GW.Statistics;
using UnityEngine;
using UnityEngine.AI;

namespace GW.Combat
{
    public class Fighter : MonoBehaviour, ISaveable, IModifierProvider
    {
        [Header("STATES")] 
        public bool AttackRequested = false;
        [SerializeField] bool m_InCombat;
        public bool m_IsCasting;
        [SerializeField] bool m_HasHitTarget;
        [SerializeField] bool m_StationaryUnit;

        [Header("DATA")] [SerializeField] 
        public float m_ChaseDistance;

        [SerializeField] public float m_TimeBetweenNewDestination = 5.0f;
        [SerializeField] public float m_DangerZoneRadius;
        //[SerializeField] public LayerMask m_TargetMask;
        //[SerializeField] public LayerMask m_AllyTargetMask;
        [SerializeField] public LayerMask m_DetectionLayer;
        [SerializeField] public List<string> m_EnemyTags = new List<string>();
        [SerializeField] public List<string> m_AllyTags = new List<string>();
        private Animator m_Animator;
        [SerializeField] public Transform m_LeftHandTransform = null;
        [SerializeField] public Transform m_RightHandTransform = null;
        [SerializeField] private Weapon m_DefaultWeapon = null;
        private Weapon m_CurrentWeapon;
        [SerializeField] private SkillSet m_SkillSet;
        [SerializeField][Range(0.0f, 1.0f)] private float m_SkillUsageProbability;
        [SerializeField][Range(0.0f, 10.0f)] private float m_ChaseDuration;

        private LazyValue<NavMeshAgent> NavMeshAgent;
        public Health m_Target;
        public Health m_AllyTarget;
        private float timeSinceLastAttack = Mathf.Infinity;
        [SerializeField] float m_MaxTimerUntilOffCombat;
        [SerializeField] float m_CurrentTimerUntilOffCombat;
        [SerializeField] float m_InCombatLayerWeightSpeed = 0.5f;
        float m_CurrentInCombatLayerUpperBodyWeight = 0.0f;
        float m_CurrentInCombatLayerFullBodyWeight = 0.0f;
        [SerializeField] WarningDisplay m_WarningDisplay;
        [SerializeField] private ParticleSystem m_EnchantementParticles;
        private SkillAnimationController m_SkillAnimationController;
        private PartySystem m_Party;
        private Outline m_Outline;
        private SkillBar_Player m_SkillBar;
        [SerializeField] UI_Effects m_UI_Effects;
        void Awake()
        {
            m_CurrentWeapon = GetDefaultWeapon();
            NavMeshAgent = new LazyValue<NavMeshAgent>(GetNavMeshAgent);
            m_SkillAnimationController = GetComponent<SkillAnimationController>();
            m_Party = GetComponentInParent<PartySystem>();

            EffectComponent effectComponent = gameObject.AddComponent<EffectComponent>();
            effectComponent.SetEnchantementParticles(m_EnchantementParticles);

            if (gameObject.tag == "Player")
            {
                m_SkillBar = GetComponent<SkillBar_Player>();
                effectComponent.SetUIEffects(m_UI_Effects);
            }
            else
            {
                SkillBar_NPC skillBar = gameObject.AddComponent<SkillBar_NPC>();
                skillBar.SetWeaponSkill(m_CurrentWeapon.GetSkill());
                SkillSet skillset = Instantiate(m_SkillSet);
                skillBar.SetSkillSet(skillset);

                //AIControllerBT AIBT = gameObject.AddComponent<AIControllerBT>();
                //AIBT.SetSkillUsageProbability(m_SkillUsageProbability);
                //AIBT.SetChaseDuration(m_ChaseDuration);
                //AIBT.SetRoamingRadius(m_DangerZoneRadius);
                //AIBT.SetTimeBetweenNewDestination(m_TimeBetweenNewDestination);
                //AIBT.SetIsStationaryUnit(m_StationaryUnit);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            NavMeshAgent.ForceInit();
            m_Animator = GetComponent<Animator>();

            float cooldownUpdateInterval = 1.0f; // Update every 0.1 seconds (adjust as needed)
            InvokeRepeating("UpdateCooldowns", cooldownUpdateInterval, cooldownUpdateInterval);
        }

        public void CancelCurrentSkill()
        {
            if (IsCasting() || IsAutoAttackng())
            {
                //ResetTimeSinceLastAttack();
                SetHasHitTarget(false);
            }
        }

        public void SetInCombatStance(bool fullBodyMask, bool inCombat)
        {
            m_CurrentTimerUntilOffCombat = m_MaxTimerUntilOffCombat;
            int layerIndexUpperBody = m_Animator.GetLayerIndex("InCombatUpperBody");
            int layerIndexFullBody = m_Animator.GetLayerIndex("InCombatFullBody");
            if (!fullBodyMask)
            {
                m_CurrentInCombatLayerFullBodyWeight = 0.0f;
                StartCoroutine(IncreaseUpperBodyLayerWeightOverTime(layerIndexUpperBody, m_InCombatLayerWeightSpeed));
                //m_Animator.SetLayerWeight(layerIndexFullBody, m_CurrentInCombatLayerFullBodyWeight);
            }
            else
            {
                m_CurrentInCombatLayerUpperBodyWeight = 0.0f;
                StartCoroutine(IncreaseFullBodyLayerWeightOverTime(layerIndexFullBody, m_InCombatLayerWeightSpeed));
                //m_Animator.SetLayerWeight(layerIndexUpperBody, m_CurrentInCombatLayerUpperBodyWeight);
            }

            if (!m_InCombat && inCombat)
            {
                m_InCombat = true;
                FindAnyObjectByType<UI_Party>().HideRemoveButton();
                FindAnyObjectByType<UI_Party>().HideAddButton();
            }
        }

        public void SetSkillAnimationLayer(bool fullBodyMask)
        {
            int layerIndexUpperBody = m_Animator.GetLayerIndex("InCombatUpperBody");
            int layerIndexFullBody = m_Animator.GetLayerIndex("InCombatFullBody");
            if (!fullBodyMask)
            {
                m_CurrentInCombatLayerFullBodyWeight = 0.0f;
                StartCoroutine(IncreaseUpperBodyLayerWeightOverTime(layerIndexUpperBody, m_InCombatLayerWeightSpeed));
                //m_Animator.SetLayerWeight(layerIndexFullBody, m_CurrentInCombatLayerFullBodyWeight);
            }
            else
            {
                m_CurrentInCombatLayerUpperBodyWeight = 0.0f;
                StartCoroutine(IncreaseFullBodyLayerWeightOverTime(layerIndexFullBody, m_InCombatLayerWeightSpeed));
                //m_Animator.SetLayerWeight(layerIndexUpperBody, m_CurrentInCombatLayerUpperBodyWeight);
            }
        }

        private IEnumerator IncreaseUpperBodyLayerWeightOverTime(int layerIndex, float speed)
        {
            //Debug.Log("IncreaseLayerWeightOverTime");
            if(layerIndex < 0) yield return null; 

            while (m_CurrentInCombatLayerUpperBodyWeight < 1.0f)
            {
                //Debug.Log(m_CurrentInCombatLayerUpperBodyWeight);
                m_CurrentInCombatLayerUpperBodyWeight =
                    Mathf.MoveTowards(m_CurrentInCombatLayerUpperBodyWeight, 1.0f, speed * Time.deltaTime);
                m_Animator.SetLayerWeight(layerIndex, m_CurrentInCombatLayerUpperBodyWeight);
                yield return null;
            }
        }
        private IEnumerator IncreaseFullBodyLayerWeightOverTime(int layerIndex, float speed)
        {
            //Debug.Log("IncreaseLayerWeightOverTime");
            while (m_CurrentInCombatLayerFullBodyWeight < 1.0f)
            {
                //Debug.Log(m_CurrentInCombatLayerFullBodyWeight);
                m_CurrentInCombatLayerFullBodyWeight =
                    Mathf.MoveTowards(m_CurrentInCombatLayerFullBodyWeight, 1.0f, speed * Time.deltaTime);
                m_Animator.SetLayerWeight(layerIndex, m_CurrentInCombatLayerFullBodyWeight);
                yield return null;
            }
        }

        public void ResetLayersWeight()
        {
            int layerIndexUpperBody = m_Animator.GetLayerIndex("InCombatUpperBody");
            int layerIndexFullBody = m_Animator.GetLayerIndex("InCombatFullBody");

            //StartCoroutine(DecreaseUpperBodyLayerWeightOverTime(layerIndexUpperBody, m_InCombatLayerWeightSpeed));
            StartCoroutine(DecreaseLayerFullBodyWeightOverTime(layerIndexFullBody, m_InCombatLayerWeightSpeed));

            //m_CurrentInCombatLayerUpperBodyWeight = 0.0f;
            //m_CurrentInCombatLayerFullBodyWeight = 0.0f;
            //m_Animator.SetLayerWeight(layerIndexUpperBody, m_CurrentInCombatLayerUpperBodyWeight);
            //m_Animator.SetLayerWeight(layerIndexFullBody, m_CurrentInCombatLayerFullBodyWeight);
        }

        public void SetOutOfCombatStance()
        {
            if (m_InCombat)
            {
                //StartCoroutine(DecreaseLayerWeightOverTime("InCombat", m_InCombatLayerWeightSpeed));
                int layerIndexUpperBody = m_Animator.GetLayerIndex("InCombatUpperBody");
                int layerIndexFullBody = m_Animator.GetLayerIndex("InCombatFullBody");

                m_CurrentInCombatLayerUpperBodyWeight = 0.0f;
                m_CurrentInCombatLayerFullBodyWeight = 0.0f;
                m_Animator.SetLayerWeight(layerIndexUpperBody, m_CurrentInCombatLayerUpperBodyWeight);
                m_Animator.SetLayerWeight(layerIndexFullBody, m_CurrentInCombatLayerFullBodyWeight);

                //StartCoroutine(DecreaseUpperBodyLayerWeightOverTime(layerIndexUpperBody, m_InCombatLayerWeightSpeed));
                //StartCoroutine(DecreaseLayerFullBodyWeightOverTime(layerIndexFullBody, m_InCombatLayerWeightSpeed));

                m_InCombat = false;
            }
        }

        private IEnumerator DecreaseUpperBodyLayerWeightOverTime(int layerIndex, float speed)
        {
            //Debug.Log("DecreaseLayerWeightOverTime");
            while (m_CurrentInCombatLayerUpperBodyWeight > 0.0f)
            {
                //Debug.Log(m_CurrentInCombatLayerUpperBodyWeight);
                m_CurrentInCombatLayerUpperBodyWeight = Mathf.MoveTowards(m_CurrentInCombatLayerUpperBodyWeight, 0.0f, speed * Time.deltaTime);
                m_Animator.SetLayerWeight(layerIndex, m_CurrentInCombatLayerUpperBodyWeight);
                yield return null;
            }
            //GetComponent<SkillAnimationController>().ResetAnimatorControllerToProfessionOffCombat();
        }

        private IEnumerator DecreaseLayerFullBodyWeightOverTime(int layerIndex, float speed)
        {
            //Debug.Log("DecreaseLayerWeightOverTime");
            while (m_CurrentInCombatLayerFullBodyWeight > 0.0f)
            {
                //Debug.Log(m_CurrentInCombatLayerFullBodyWeight);
                m_CurrentInCombatLayerFullBodyWeight = Mathf.MoveTowards(m_CurrentInCombatLayerFullBodyWeight, 0.0f, speed * Time.deltaTime);
                m_Animator.SetLayerWeight(layerIndex, m_CurrentInCombatLayerFullBodyWeight);
                yield return null;
            }
            //GetComponent<SkillAnimationController>().ResetAnimatorControllerToProfessionOffCombat();
        }

        public void EquipWeapon(Weapon weapon)
        {
            m_CurrentWeapon = weapon;
            AttachWeapon(weapon);
        }

        private void AttachWeapon(Weapon weapon)
        {
            Animator anim = GetComponent<Animator>();
            weapon.Spawn(m_RightHandTransform, m_LeftHandTransform, anim, this);
        }

        public void AutoAttackBehaviour()
        {
            //SetInCombatStance();
            
            if (timeSinceLastAttack > m_CurrentWeapon.GetTimeBetweenAttacks() && AttackRequested && !m_Target.IsDead())
            {
                GetComponent<SkillAnimationController>().PlayAutoAttackAnimation(m_CurrentWeapon.GetTimeBetweenAttacks());
                LookAtTarget(m_Target.transform);
                //This will trigger the hit event
                m_HasHitTarget = false;
                GetComponent<SkillAnimationController>().TriggerAttack();
                timeSinceLastAttack = 0.0f;
            }
        }

        //Auto-Attack Animation Event
        void Hit()
        {
            if (m_Target == null) return;
            //if (m_HasHitTarget) return;
            m_HasHitTarget = true;
            float damage = gameObject.GetComponent<BaseStats>().GetWeaponStat(m_CurrentWeapon, m_Target.gameObject);

            //Debug.Log("Auto Attack is dealing " + "damage " + " damage with " + m_CurrentWeapon.name);

            if (m_CurrentWeapon.HasProjectile())
            {
                m_CurrentWeapon.LaunchProjectile(m_RightHandTransform, m_LeftHandTransform, m_Target, gameObject,
                    damage, m_EnemyTags);
            }
            else
            {
                m_Target.TakeDamage(gameObject, damage);
            }
            //m_HasHitTarget = false;
        }

        public void UpdateCooldowns()
        {
            timeSinceLastAttack += 1.0f;

            if (m_InCombat && !m_IsCasting && !AttackRequested)
            {
                m_CurrentTimerUntilOffCombat -= 1.0f;
                {
                    if (m_CurrentTimerUntilOffCombat <= 0)
                        SetOutOfCombatStance();
                }
            }

            //if (gameObject.tag != "Player")
            //{
            //    return;
            //}


            //if (m_Target == null) return;
            //if (m_Target.IsDead())
            //{
            //    AttackRequested = false;
            //    return;
            //}

            //if (m_Target && !GetIsInRange() && AttackRequested)
            //{
            //    m_WarningDisplay.UpdateWarningText(WarningMessages.OutOfRange);
            //    m_SkillAnimationController.StopAttack();
            //    AttackRequested = false;
            //}
            //else if (!m_IsCasting)
            //{
            //    AutoAttackBehaviour();
            //}
        }

        public bool CanAttack(GameObject combatTarget)
        {
            if (combatTarget == null)
            {
                return false;
            }

            Health targetToTest = combatTarget.GetComponent<Health>();
            if (targetToTest != null && !targetToTest.IsDead())
            {
                return true;
            }
            else
            {
                AttackRequested = false;
                return false;
            }
        }

        public void SwitchTargetToAttack(GameObject combatTarget)
        {
            if(combatTarget != null)
                m_Target = combatTarget.GetComponent<Health>();
        }

        public void Attack()
        {
            if (GetIsInRange())
            {
                //m_SkillAnimationController.PlayAutoAttackAnimation(m_CurrentWeapon.GetTimeBetweenAttacks());
                //m_SkillAnimationController.TriggerAttack();
                m_SkillBar.SetLoopAutoAttack(true);

                //m_SkillBar.ActivateSkill(m_CurrentWeapon.GetSkill());

                m_Party.SetChildrenHasTakenDamage(true, m_Target.gameObject);
            }
            else
            {
                m_WarningDisplay.UpdateWarningText(WarningMessages.OutOfRange);
            }
        }
        
        private void LookAtTarget(Transform targetPosition)
        {
            Vector3 direction = targetPosition.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }

        private bool IsGameObjectInTagList(List<string> tagList)
        {
            return tagList.Contains(gameObject.tag);
        }

        private Vector3 GetVectorLookAt(Vector3 vectorToChange)
        {
            Vector3 lookPos = vectorToChange;
            lookPos.y = transform.position.y;
            return lookPos;
        }

        public bool GetIsInRange()
        {
            if (m_Target)
            {
                return Vector3.Distance(transform.position, m_Target.transform.position) <
                       m_CurrentWeapon.GetSkill().GetRange();
            }

            return false;
        }

        public float GetWeaponRange()
        {
            return m_CurrentWeapon.GetRange();
        }

        public Weapon GetWeapon()
        {
            return m_CurrentWeapon;
        }

        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return m_CurrentWeapon.m_MaximumDamage;
            }
        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return m_CurrentWeapon.m_PercentageBonus;
            }
        }

        NavMeshAgent GetNavMeshAgent()
        {
            return GetComponent<NavMeshAgent>();
        }

        private Weapon GetDefaultWeapon()
        {
            AttachWeapon(m_DefaultWeapon);
            return m_DefaultWeapon;
        }

        public object CaptureState()
        {
            return m_CurrentWeapon.name;
        }


        public void RestoreState(object state)
        {
            string weaponName = (string)state;
            Weapon weapon = Resources.Load(weaponName) as Weapon;
            EquipWeapon(weapon);
        }

        public float GetCastingRange()
        {
            return 1.2f * m_DangerZoneRadius;
        }

        public float GetMeleeRange()
        {
            return 0.15f * m_DangerZoneRadius;
        }

        public bool IsInCombat() => m_InCombat;
        public bool IsCasting() => m_IsCasting;
        public bool IsAutoAttackng() => AttackRequested;
        public void SetHasHitTarget(bool b) => m_HasHitTarget = b;
        public bool HasHitTarget() => m_HasHitTarget;
        public void ResetTimeSinceLastAttack() => timeSinceLastAttack = 0.0f;

        public bool HasTargetAHealthComponent(GameObject go)
        {
            Health targetToTest = go.GetComponent<Health>();
            if (targetToTest != null && !targetToTest.IsDead())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetTarget(GameObject target)
        {
            if(target)
                m_Target = target.GetComponent<Health>();
            else
            {
                m_Target = null;
                m_SkillAnimationController.StopAttack();
            }
        }

        public void SetAllyTarget(GameObject target)
        {
            if (target)
                m_AllyTarget = target.GetComponent<Health>();
            else
            {
                m_AllyTarget = null;
            }
        }

        public Transform GetLeftHandTransform() => m_LeftHandTransform;
        public Transform GetReftHandTransform() => m_RightHandTransform;
    }
}
