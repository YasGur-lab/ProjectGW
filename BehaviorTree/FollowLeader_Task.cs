using GW.Core;
using GW.Movement;
using GW.Attributes;
using GW.Combat;
using UnityEngine;
using GW.Statistics;
using UnityEngine.AI;
using System.Collections.Generic;
using StarterAssets;
using static UnityEngine.ParticleSystem;

public class FollowLeader_Task : Task
{
    public Transform m_Transform;
    public ActionScheduler m_Scheduler;
    public Mover m_Mover;
    public Mover_V2 m_MoverV2;
    public float m_RoamingRadius;

    //private bool m_Requested;
    public float m_TimeBetweenNewDestination;
    private Vector3 m_RoamingDestination;
    public Vector3 m_OriginalPosition;
    private new BehaviorTree tree;
    public Fighter m_Fighter;
    private PartySystem m_Party;

    private GameObject m_PartyLeader;
    private float m_CurrentTimeBetweenNewDestination;
    private bool m_AtDestination;
    private float m_DelayBeforeFollow = 1.0f;
    private bool m_StartFollowing = false;

    public FollowLeader_Task(BehaviorTree tree)
    {
        this.tree = tree;
    }

    public override NodeResult Execute()
    {

        if (m_Fighter.IsCasting() || m_Fighter.IsAutoAttackng()) return NodeResult.FAILURE;
        if (m_Fighter.m_Target)
        {
            return NodeResult.FAILURE; // Fail if a target is detected
        }

        if (m_Transform.GetComponent<Health>().IsDead())
        {
            return NodeResult.STOP;  
        }

        if (m_Party == null || m_Party != m_Transform.GetComponentInParent<PartySystem>())
        {
            m_Party = m_Transform.GetComponentInParent<PartySystem>();
        }

        bool requested = (bool)tree.GetValue("m_Requested");

        if (requested)
        {
            m_CurrentTimeBetweenNewDestination -= 0.01f;
            if (m_CurrentTimeBetweenNewDestination <= 0)
            {
                requested = false;
                m_CurrentTimeBetweenNewDestination = m_TimeBetweenNewDestination;
            }
        }

        if (m_PartyLeader == null) m_PartyLeader = m_Party.GetLeader();
        if (m_PartyLeader.tag != "Player")
        {
            if (m_Party.HasLeaderMoved() && !requested)
            {
                if (!m_PartyLeader.GetComponent<Health>().IsDead())
                {
                    m_RoamingDestination = m_Party.GetGameObjectPartyPosition(m_Transform);
                }
                else
                {
                    m_RoamingDestination = GetNavMeshDestination();
                }



                //m_Mover.StartMoveAction(m_RoamingDestination, true, 1.0f);
                m_CurrentTimeBetweenNewDestination = m_TimeBetweenNewDestination;
                m_MoverV2.SetCurrentDestination(m_RoamingDestination);

                //LookAtTarget(m_RoamingDestination);
                tree.SetValue("m_Requested", true);
            }
        }
        else
        {
            if (m_PartyLeader.GetComponent<ThirdPersonController>().CurrentlyMoving)
            {
                if (!m_StartFollowing)
                {
                    //Debug.Log(m_DelayBeforeFollow);
                    m_DelayBeforeFollow -= 0.01f;
                    if (m_DelayBeforeFollow <= 0)
                    {
                        m_StartFollowing = true;
                        m_DelayBeforeFollow = 1.0f; // Reset the delay timer
                    }
                }

                if (!m_PartyLeader.GetComponent<Health>().IsDead())
                {
                    m_RoamingDestination = m_Party.GetGameObjectPartyPosition(m_Transform);
                }
                else
                {
                    m_RoamingDestination = GetNavMeshDestination();
                }

                if (m_StartFollowing && !requested)
                {
                    //m_Mover.StartMoveAction(m_RoamingDestination, true, 1.0f);
                    //LookAtTarget(m_RoamingDestination);

                    m_MoverV2.SetCurrentDestination(m_RoamingDestination);
                    m_CurrentTimeBetweenNewDestination = m_TimeBetweenNewDestination;
                    tree.SetValue("m_Requested", true);
                }
            }
            else
            {
                m_StartFollowing = false;
                //m_AtDestination = false;
                m_DelayBeforeFollow = 1.0f;
                tree.SetValue("m_Requested", false);
            }
        }

        if (!m_AtDestination)
        {
            float distance = Vector3.Distance(m_Transform.position, m_RoamingDestination);

            float stoppingDistance = m_MoverV2.GetNavMeshAgent().stoppingDistance;
            float bufferDistance = 1.1f;
            //Debug.Log("First if: " + "m_AtDestination: " + m_AtDestination + " requested: " + requested);
            if (distance < stoppingDistance + bufferDistance && distance > stoppingDistance - bufferDistance)
            {
                
                m_AtDestination = true;
                //Debug.Log("First if: " + "m_AtDestination: " + m_AtDestination + " requested: " + requested);
            }
        }

        if (m_AtDestination)
        {
            m_StartFollowing = false;
            m_AtDestination = false;
            m_DelayBeforeFollow = 1.0f;
            tree.SetValue("m_Requested", false);
        }
        

        return NodeResult.SUCCESS;
    }

    private void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - m_Transform.position;
        direction.y = 0.0f;
        if (direction.sqrMagnitude > Mathf.Epsilon)
        {
            direction.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            m_Transform.rotation = targetRotation;
        }
    }

    public Vector3 GetNavMeshDestination()
    {
        m_RoamingDestination = m_Party.GetGameObjectPartyPosition(m_Transform);
        tree.SetValue("RoamingDestination", m_RoamingDestination);
        return m_RoamingDestination;
    }
}
