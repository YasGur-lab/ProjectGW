using GW.Attributes;
using GW.Core;
using GW.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToSpawnPoint_Task : Task
{
    public Transform m_Transform;
    public Vector3 m_OriginalPosition;
    public Mover m_Mover;

    // Start is called before the first frame update
    public override void Reset()
    {
        base.Reset();
    }

    public override NodeResult Execute()
    {
        if (m_Transform.GetComponent<Health>().IsDead())
        {
            return NodeResult.FAILURE;
        }

        List<GameObject> potentialTargets = (List<GameObject>)tree.GetValue("PotentialTargets");
        if (potentialTargets.Count > 0) return NodeResult.FAILURE;

        m_Transform.LookAt(m_OriginalPosition);

        float distance = Vector3.Distance(m_Transform.gameObject.transform.position, m_OriginalPosition);

        if (distance > 2.0f)
        {
            m_Mover.StartMoveAction(m_OriginalPosition, true, 1);

            return NodeResult.RUNNING;
        }
        return NodeResult.SUCCESS;
    }
}
