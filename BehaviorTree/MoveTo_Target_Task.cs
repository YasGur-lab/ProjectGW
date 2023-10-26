using GW.Attributes;
using GW.Combat;
using GW.Core;
using GW.Movement;
using GW.Statistics;
using System.Collections;
using System.Collections.Generic;
using GW.Control;
using UnityEngine;
using UnityEngine.AI;

public class MoveTo_Target_Task : Task
{
    public Transform m_Transform;
    public Mover m_Mover;
    public float m_WeaponRange;
    public float m_MinimumAggroRange;
    public float m_ChaseDistance;

    private GameObject m_Target;
    private bool m_AttackedWasStopped;
    private float m_ChasingDuration;
    private float m_CurrentChasingTime;

    private float m_CurrentSkillRange;
    private Skill m_SkillToUse;
    public Mover_V2 m_MoverV2;

    private bool m_AtDestination;

    private float m_CurrentTimeBetweenNewDestination;

    private float m_TimeBetweenNewDestination = 0.5f;

    // Start is called before the first frame updatea
    public override void Reset()
    {
        base.Reset();
        m_CurrentChasingTime = 0f;
        m_AttackedWasStopped = false;
        m_Target = null;
    }

    public override NodeResult Execute()
    {
        if (m_Transform.GetComponent<Health>().IsDead())
        {
            return NodeResult.STOP;
        }
        
        m_Target = (GameObject)tree.GetValue("ClosestTarget");
        if (m_Target == null || m_Target.GetComponent<Health>().IsDead())
        {
            return NodeResult.FAILURE;
        }

        m_ChasingDuration = (float)tree.GetValue("MaxChasingDuration");
        if (m_CurrentChasingTime >= m_ChasingDuration)
        {
            m_CurrentChasingTime = 0.0f;
            m_AtDestination = false;
            tree.SetValue("ChasingRequested", false);
            m_SkillToUse = null;
            tree.ResetBehabviorTree("MoveTo_Target_Task");
            return NodeResult.FAILURE;
        }

        bool requested = (bool)tree.GetValue("ChasingRequested");
        Vector3 targetPos = m_Target.transform.position;

        if (requested)
        {
            m_CurrentTimeBetweenNewDestination -= 0.01f;
            if (m_CurrentTimeBetweenNewDestination <= 0)
            {
                requested = false;
                m_CurrentTimeBetweenNewDestination = m_TimeBetweenNewDestination;
            }
        }

        m_SkillToUse = (Skill)tree.GetValue("CurrentSkillInUse");
        if (m_SkillToUse != null)
        {
            m_CurrentSkillRange = m_SkillToUse.GetRange();
        }

        if (!requested)
        {
            m_CurrentTimeBetweenNewDestination = m_TimeBetweenNewDestination;
            Vector3 dir = m_Transform.position - targetPos;
            dir.Normalize();
            Vector3 destination = targetPos;
            m_MoverV2.SetCurrentDestination(destination + dir * (m_CurrentSkillRange - 1.0f));
            tree.SetValue("ChasingRequested", true);
        }
        return NodeResult.SUCCESS;
    }

}
