using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using UnityEngine;

public class Detect_Targets_Task : Task
{
    public Transform m_Transform;
    public float m_MinimumAggroRange;
    public LayerMask m_LayerMask;
    public LayerMask m_AllyLayerMask;
    public Fighter m_Fighter;
    public LayerMask m_DetectionLayer;
    public List<string> m_EnemyTags;
    public List<string> m_AllyTags;
    
    public override NodeResult Execute()
    {

        if (m_Transform.gameObject.GetComponent<Health>().IsDead())
        {
            return NodeResult.STOP;
        }



        //Collider[] colliders = Physics.OverlapSphere(m_Transform.position, m_MinimumAggroRange, m_LayerMask);
        //List<GameObject> potentialTargets = (List<GameObject>)tree.GetValue("PotentialTargets");

        bool canDetectTargets = (bool)tree.GetValue("CanDetectTargets");
        if (canDetectTargets)
        {
            canDetectTargets = false;
            tree.SetValue("CanDetectTargets", canDetectTargets);
            List<GameObject> potentialAllyTargets = (List<GameObject>)tree.GetValue("PotentialAllyTargets");
            List<GameObject> potentialTargets = (List<GameObject>)tree.GetValue("PotentialTargets");

            Collider[] colliders =
                Physics.OverlapSphere(m_Transform.position, m_MinimumAggroRange, m_DetectionLayer);

            // Filter colliders to only include alive targets
            List<GameObject> aliveTargets = colliders
                .Select(collider => collider.gameObject)
                .Where(target => {
                    // Check if the target is alive
                    Health health = target.GetComponent<Health>();
                    if (health && !health.IsDead())
                    {
                        // Check if the target's tag matches the enemy or ally tags
                        if (tree.IsTargetInTagList(target, m_EnemyTags) || tree.IsTargetInTagList(target, m_AllyTags))
                        {
                            return true;
                        }
                    }
                    return false;
                })
                .ToList();

            NodeResult result;

            // Check if there are any alive targets within the minimum aggro range
            if (aliveTargets.Count > 0)
            {

                // Add new alive targets to potentialTargets
                foreach (var target in aliveTargets)
                {
                    if (!potentialTargets.Contains(target) && tree.IsTargetInTagList(target, m_EnemyTags))
                    {
                        potentialTargets.Add(target);
                    }
                }

                foreach (var target in aliveTargets)
                {
                    if (!potentialAllyTargets.Contains(target) && tree.IsTargetInTagList(target, m_AllyTags))
                    {
                        potentialAllyTargets.Add(target);
                    }
                }

                tree.SetValue("PotentialTargets", potentialTargets);
                tree.SetValue("PotentialAllyTargets", potentialAllyTargets);
                //Debug.Log("Detect_Targets_Task has detected:" + aliveTargets.Count);
                result = children[0].Execute();
                return result;
            }

            if (m_Fighter.m_Target)
            {
                tree.ResetBehabviorTree("Remove_Target_Task");
            }

            result = children[0].Execute();
            Debug.Log("Detect_Targets_Task has detected:" + aliveTargets.Count);
            return result;
        }
        return children[0].Execute();
    }
}