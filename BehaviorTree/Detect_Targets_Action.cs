using System.Collections;
using System.Collections.Generic;
using GW.Combat;
using UnityEngine;
using TheKiwiCoder;
using System.Linq;
using GW.Attributes;
using GW.Control;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.Experimental.GraphView;

public class Detect_Targets_Action : ActionNode
{
    public Transform m_Transform;
    public float m_MinimumAggroRange;
    public Fighter m_Fighter;
    public LayerMask m_DetectionLayer;
    public List<string> m_EnemyTags;
    public List<string> m_AllyTags;
    public SkillAnimationController m_SkillAnimationController;
    public Health m_Target;

    protected override void OnStart() 
    {
        //context.m_Transform = m_Transform;
        //context.m_DetectionLayer = m_DetectionLayer;
        //context.m_MinimumAggroRange = m_MinimumAggroRange;
        //context.m_EnemyTags = m_EnemyTags;
        //context.m_AllyTags = m_AllyTags;
        //context.m_SkillAnimationController = m_SkillAnimationController;
        //context.m_Target = m_Target;

        m_Transform = context.m_Transform;
        m_DetectionLayer = context.m_DetectionLayer;
        m_MinimumAggroRange = context.m_MinimumAggroRange;
        m_EnemyTags = context.m_EnemyTags;
        m_AllyTags = context.m_AllyTags;
        m_SkillAnimationController = context.m_SkillAnimationController;
        m_Target = context.m_Target;
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() 
    {
        if (m_Transform.gameObject.GetComponent<Health>().IsDead())
        {
            return State.Failure;
        }



        //Collider[] colliders = Physics.OverlapSphere(m_Transform.position, m_MinimumAggroRange, m_LayerMask);
        //List<GameObject> potentialTargets = (List<GameObject>)tree.GetValue("PotentialTargets");

        bool canDetectTargets = blackboard.m_CanDetectTargets;
        if (canDetectTargets)
        {
            canDetectTargets = false;
            blackboard.m_CanDetectTargets =  canDetectTargets;
            List<GameObject> potentialAllyTargets = blackboard.m_PotentialAllyTargets;
            List<GameObject> potentialTargets = blackboard.m_PotentialTargets;

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
                        if (blackboard.IsTargetInTagList(target, m_EnemyTags) || blackboard.IsTargetInTagList(target, m_AllyTags))
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
                    if (!potentialTargets.Contains(target) && blackboard.IsTargetInTagList(target, m_EnemyTags))
                    {
                        potentialTargets.Add(target);
                    }
                }

                foreach (var target in aliveTargets)
                {
                    if (!potentialAllyTargets.Contains(target) && blackboard.IsTargetInTagList(target, m_AllyTags))
                    {
                        potentialAllyTargets.Add(target);
                    }
                }

                blackboard.m_PotentialTargets = potentialTargets;
                blackboard.m_PotentialAllyTargets = potentialAllyTargets;
                //Debug.Log("Detect_Targets_Task has detected:" + aliveTargets.Count);
                //result = children[0].Execute();
                //return result;

                Debug.Log("PotentialTargets Count: " + potentialTargets.Count);
                Debug.Log("PotentialAllyTargets Count: " + potentialAllyTargets.Count);
                return State.Success;
            }

            if (m_Fighter.m_Target)
            {
                ResetBlackboard();
            }

            //result = children[0].Execute();
            //Debug.Log("Detect_Targets_Task has detected:" + aliveTargets.Count);
            //return result;

            return State.Success;
        }
        //return children[0].Execute();
        return State.Success;
    }

    private void ResetBlackboard()
    {
        m_SkillAnimationController.StopAttack();
        m_Target = null;
        blackboard.m_ClosestTarget = null;
    }
}
