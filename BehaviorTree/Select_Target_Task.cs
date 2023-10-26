using System.Collections.Generic;
using System.Linq;
using GW.Attributes;
using GW.Combat;
using UnityEngine;

public class Select_Target_Task : Task
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
        
        var potentialTargets = (List<GameObject>)tree.GetValue("PotentialTargets");
        List<GameObject> aliveTargets = potentialTargets
            .Where(target => target != null && !target.GetComponent<Health>().IsDead())
            .ToList();

        if (potentialTargets.Count > 0 && aliveTargets.Count == 0)
        {
            tree.ResetBehabviorTree("WeaponAttack_Task", 1);
            //Debug.Log("Select_Target_Task: Target is dead");
            return NodeResult.FAILURE;
        }

        NodeResult result;
        
        if (m_CurrentTarget != null && aliveTargets.Contains(m_CurrentTarget))
        {
            result = children[0].Execute();
            return result;
        }
        else if(m_CurrentTarget != null)
        {
            m_CurrentTarget = null;
            tree.ResetBehabviorTree("WeaponAttack_Task", 1);
            return NodeResult.FAILURE;
        }

        GameObject randomTarget = RandomlySelectTarget(aliveTargets);
        tree.SetValue("ClosestTarget", randomTarget);
        m_CurrentTarget = randomTarget;
        m_Fighter.m_Target = randomTarget.GetComponent<Health>();

        result = children[0].Execute();
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
}
