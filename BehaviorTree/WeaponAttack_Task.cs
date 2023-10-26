using GW.Core;
using GW.Movement;
using GW.Attributes;
using GW.Combat;
using UnityEngine;
using GW.Statistics;
using UnityEngine.AI;
using GW.Control;

public class WeaponAttack_Task : Task
{
    public Transform m_Transform;
    public Fighter m_Fighter;
    public ActionScheduler m_Scheduler;
    public Mover m_Mover;
    public BaseStats m_Stats;
    public Weapon m_Weapon;
    public Transform m_LeftHandTransform;
    public Transform m_RightHandTransform;
    public NavMeshAgent m_NavMeshAgent;
    public SkillAnimationController m_SkillAnimationController;
    private float m_TimeSinceLastAttack = 1.0f;
    private GameObject m_Target;
    public SkillBar_V2 m_SkillBar;

    private new BehaviorTree tree;


    public WeaponAttack_Task(BehaviorTree tree)
    {
        this.tree = tree;
    }

    public override NodeResult Execute()
    {
        if (m_Transform.GetComponent<Health>().IsDead())
        {
            m_TimeSinceLastAttack = m_Weapon.m_TimeBetweenAttacks;
            return NodeResult.STOP;
        }

        if(!m_SkillBar.IsSkillCompleted()) { return NodeResult.FAILURE; }


        //m_Fighter.SetInCombatStance();
        m_Fighter.SetHasHitTarget(false);

        m_Target = (GameObject)tree.GetValue("ClosestTarget");

        if (m_Target == null)
        {
            m_TimeSinceLastAttack = m_Weapon.m_TimeBetweenAttacks;
            //tree.SetValue("m_Requested", false);
            tree.ResetBehabviorTree("WeaponAttack_Task", 0);
            return NodeResult.FAILURE;
        }

        if (!m_Target.GetComponent<Health>().IsDead())
        {
            float distance = Vector3.Distance(m_Transform.position, m_Target.transform.position);
            if (distance > (float)tree.GetValue("m_ChaseDistance") || distance > m_Weapon.GetRange())
            {
                m_TimeSinceLastAttack = m_Weapon.m_TimeBetweenAttacks;
                return NodeResult.FAILURE;
            }

            m_TimeSinceLastAttack += 0.01f; //Time.deltaTime;

            if (m_TimeSinceLastAttack <= m_Weapon.m_TimeBetweenAttacks)
            {
                return NodeResult.RUNNING;
            }


            if (!m_Fighter.AttackRequested)
            {
                m_SkillAnimationController.StopSkillAnimations();
                m_SkillAnimationController.PlayAutoAttackAnimation(m_Weapon.m_TimeBetweenAttacks);
                m_Fighter.SetHasHitTarget(false);
                LookAtTarget(m_Target.transform.position);
                m_SkillAnimationController.TriggerAttack();
                tree.SetValue("ClosestTarget", m_Target);
                PartySystem party = m_Transform.GetComponentInParent<PartySystem>();
                if (party)
                    party.SetChildrenHasTakenDamage(true, m_Target);
                tree.SetValue("m_Requested", false);
            }

            LookAtTarget(m_Target.transform.position);
            m_TimeSinceLastAttack = 0.0f;
            return NodeResult.SUCCESS;
        }

        //Debug.Log("WeaponAttack_Task: Target is dead");

        m_TimeSinceLastAttack = m_Weapon.m_TimeBetweenAttacks;
        tree.ResetBehabviorTree("WeaponAttack_Task", 1);
        tree.SetValue("m_Requested", false);
        return NodeResult.FAILURE;
    }

    private void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - m_Transform.position;
        direction.y = 0.0f;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        m_Transform.rotation = targetRotation;
    }
}
