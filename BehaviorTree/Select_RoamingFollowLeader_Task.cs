using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using UnityEngine;

public class Select_RoamingFollowLeader_Task : Task
{
    public Transform m_Transform;
    private PartySystem m_Party;

    public override NodeResult Execute()
    {
        if (m_Transform.GetComponent<Health>().IsDead())
        {
            return NodeResult.STOP;
        }
        
        if (m_Party == null || m_Party != m_Transform.GetComponentInParent<PartySystem>())
        {
            m_Party = m_Transform.GetComponentInParent<PartySystem>();
        }

        if (m_Party == null) return NodeResult.FAILURE;

        NodeResult result;
        if (m_Transform.gameObject == m_Party.GetLeader())
        {
            //roaming task
            result = children[0].Execute();
            return result;
        }
        else
        {
            //follow leader task
            result = children[1].Execute();
            return result;
        }

    }

    GameObject RandomlySelectTarget(List<GameObject> potentialTargets)
    {
        GameObject instigator = m_Transform.gameObject.GetComponent<Health>().GetInstigator();
        if (instigator != null)
        {
            potentialTargets.Add(instigator);
        }

        int randomIndex = Random.Range(0, potentialTargets.Count);
        return potentialTargets[randomIndex];
    }

    GameObject GetLowestHealthAlly(List<GameObject> potentialTargets)
    {
        GameObject lowestHealthObject = potentialTargets
            .Where(go => go != null && go.GetComponent<Health>() != null)
            .OrderBy(go => go.GetComponent<Health>().GetHealth())
            .FirstOrDefault();

        return lowestHealthObject;
    }
}
