using GW.Attributes;
using GW.Combat;
using GW.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsInCombat_Task : Task
{
    public Transform m_Transform;
    public Health m_Health;
    public float m_MaxDistance;
    public float m_ChaseBuffer;
    public LayerMask m_LayerMask;

    private float m_TimeSinceLastTargetDetection = 2.0f;
    public Fighter m_Fighter;

    // Start is called before the first frame update
    public override void Reset()
    {
        base.Reset();
    }

    public override NodeResult Execute()
    {
        if (m_Health.IsDead())
        {
            return NodeResult.STOP;
        }
        GameObject target = (GameObject)tree.GetValue("ClosestTarget");
        NodeResult result;
        if (m_Fighter.IsInCombat() && m_Health.GetInstigator() && target == null)
        {
            //Debug.Log("IsInCombat_Task");
            tree.SetValue("ClosestTarget", m_Health.GetInstigator());
            //List<GameObject> targets = (List<GameObject>)tree.GetValue("PotentialTargets");
            //targets.Add(m_Health.GetInstigator());
            //tree.SetValue("PotentialTargets", targets);
            m_Fighter.m_Target = m_Health.GetInstigator().GetComponent<Health>();
            //split attack casting task
            result = children[1].Execute();
            return result;
        }

        //remove target task
        //result = children[0].Execute(); 
        return NodeResult.SUCCESS;
    }
}
