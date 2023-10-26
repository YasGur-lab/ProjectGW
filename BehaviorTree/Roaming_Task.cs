using GW.Core;
using GW.Movement;
using GW.Attributes;
using GW.Combat;
using UnityEngine;
using GW.Statistics;
using UnityEngine.AI;
using System.Collections.Generic;

public class Roaming_Task : Task
{
    public Transform m_Transform;
    public ActionScheduler m_Scheduler;
    public Mover m_Mover;
    public float m_RoamingRadius;

    //private bool m_Requested;
    public float m_TimeBetweenNewDestination;
    private Vector3 m_RoamingDestination;
    public Vector3 m_OriginalPosition;
    private new BehaviorTree tree;
    public Fighter m_Fighter;
    private float m_CurrentTimeBetweenNewDestination;
    private bool m_AtDestination;
    private PartySystem m_Party;
    private bool NavMeshStopped;
    public Mover_V2 m_MoverV2;

    public Roaming_Task(BehaviorTree tree)
    {
        this.tree = tree;
    }

    public override NodeResult Execute()
    {

        if (m_Fighter.IsInCombat() || m_Fighter.IsCasting() || m_Fighter.IsAutoAttackng()) return NodeResult.FAILURE;
        if (m_Fighter.m_Target)
        {
            return NodeResult.FAILURE; // Fail if a target is detected
        }

        if (m_Transform.GetComponent<Health>().IsDead())
        {
            //tree.ResetBehabviorTree();
            return NodeResult.STOP; // Stop if the entity is dead
        }

        if (m_Party == null || m_Party != m_Transform.GetComponentInParent<PartySystem>())
        {
            m_Party = m_Transform.GetComponentInParent<PartySystem>();
        }

        bool requested = (bool)tree.GetValue("m_Requested");

        if (!requested)
        {
            tree.SetValue("m_Requested", true);
            //GetRandomNavMeshPoint();
            //m_Mover.StartMoveAction(GetRandomNavMeshPoint(), true, 1.0f);

            m_MoverV2.SetCurrentDestination(GetRandomNavMeshPoint());

            m_Party.SetLeaderHasMoved(true);
            //LookAtTarget(m_RoamingDestination);
            m_CurrentTimeBetweenNewDestination = m_TimeBetweenNewDestination;
            //return NodeResult.RUNNING;
        }

        // Check if we have reached the destination
        if (!m_AtDestination)
        {
            float distance = Vector3.Distance(m_Transform.position, m_RoamingDestination);

            float stoppingDistance = m_MoverV2.GetNavMeshAgent().stoppingDistance;
            float bufferDistance = 1.1f;

            if (distance < stoppingDistance + bufferDistance && distance > stoppingDistance - bufferDistance)
            {
                //m_Mover.Cancel();
                m_AtDestination = true;
            }
        }

        if (m_AtDestination)
        {
            
            if (!NavMeshStopped)
            {
                NavMeshStopped = true;
                //m_MoverV2.Cancel();
                m_Party.SetLeaderHasMoved(false);
            }

            m_CurrentTimeBetweenNewDestination -= 0.01f;
            if (m_CurrentTimeBetweenNewDestination <= 0.0f)
            {
                //m_Party.SetLeaderHasMoved(false);
                tree.SetValue("m_Requested", false);
                m_AtDestination = false;
                NavMeshStopped = false;
            }
        }

        return NodeResult.SUCCESS;
    }

    private void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - m_Transform.position;
        if (direction.sqrMagnitude > Mathf.Epsilon)
        {
            direction.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            m_Transform.rotation = targetRotation;
        }
    }

    public Vector3 GetRandomNavMeshPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * m_RoamingRadius;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(m_OriginalPosition + randomDirection, out hit, m_RoamingRadius, NavMesh.AllAreas))
        {
            m_RoamingDestination = hit.position;
            tree.SetValue("RoamingDestination", m_RoamingDestination);
            return m_RoamingDestination;
        }
        else
        {
            return m_OriginalPosition;
        }
    }
}
