using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using UnityEngine;

public class Select_TargetToSupport_Task : Task
{
    public Transform m_Transform;
    public Fighter m_Fighter;
    private GameObject m_CurrentTarget;

    public override NodeResult Execute()
    {
        if (m_Transform.GetComponent<Health>().IsDead())
        {
            return NodeResult.STOP;
        }

        //if(m_Fighter.IsCasting()) return NodeResult.SUCCESS;

        List<GameObject> alliesInNeed = (List<GameObject>)tree.GetValue("AlliesInNeed");
        if (alliesInNeed.Count == 0)
        {
            return NodeResult.FAILURE;
        }

        List<GameObject> aliveTargets = alliesInNeed
            .Where(target => target != null && !target.GetComponent<Health>().IsDead())
            .ToList();

        if (aliveTargets.Count == 0)
        {
            tree.SetValue("ClosestAllyInNeed", null);
            alliesInNeed.Clear();
            tree.SetValue("AlliesInNeed", null);
            return NodeResult.FAILURE;
        }

        //tree.SetValue("ClosestAllyInNeed", null);
        //m_Fighter.m_AllyTarget = null;
       
        GameObject randomTarget = GetLowestHealthAlly(aliveTargets);
        tree.SetValue("ClosestAllyInNeed", randomTarget);
        m_CurrentTarget = randomTarget;
        m_Fighter.m_AllyTarget = randomTarget.GetComponent<Health>();
        NodeResult result = children[0].Execute();
        return result;
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
