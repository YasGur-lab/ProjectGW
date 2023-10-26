using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using System.Linq;
using GW.Attributes;
using GW.Control;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.Experimental.GraphView;

public class Remove_Target_Action : ActionNode
{
    public Transform m_Transform;
    public float m_MaxDistance;
    public LayerMask m_DetectionLayer;
    public List<string> m_EnemyTags;
    public List<string> m_AllyTags;
    public SkillAnimationController m_SkillAnimationController;
    public Health m_Target;
    protected override void OnStart()
    {
        //context.m_Transform = m_Transform;
        //context.m_DetectionLayer = m_DetectionLayer;
        //context.m_MaxDistance = m_MaxDistance;
        //context.m_EnemyTags = m_EnemyTags;
        //context.m_AllyTags = m_AllyTags;
        //context.m_SkillAnimationController = m_SkillAnimationController;
        //context.m_Target = m_Target;

        m_Transform = context.m_Transform;
        m_DetectionLayer = context.m_DetectionLayer;
        m_MaxDistance = context.m_MaxDistance;
        m_EnemyTags = context.m_EnemyTags;
        m_AllyTags = context.m_AllyTags;
        m_SkillAnimationController = context.m_SkillAnimationController;
        m_Target = context.m_Target;
    }

    protected override void OnStop() 
    {

    }

    protected override State OnUpdate() 
    {
        if (m_Transform.gameObject.GetComponent<Health>().IsDead())
        {
            //should be stop;
            return State.Failure;
        }


        bool canOverlapSphere = blackboard.m_CanOverlapSphere;
        if (!canOverlapSphere)
        {
            return State.Success;
        }

        NodeResult result;
        List<GameObject> potentialAllyTargets = blackboard.m_PotentialAllyTargets;
        List<GameObject> potentialTargets = blackboard.m_PotentialTargets;
        Collider[] colliders =
            Physics.OverlapSphere(m_Transform.position, m_MaxDistance, m_DetectionLayer);

        blackboard.m_CanOverlapSphere = false;

        if (colliders.Length == 0)
        {
            ResetBlackboard();
            //result = children[0].Execute();
            //return result;
            return State.Success;
        }

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

        if (potentialTargets.Count > 0)
        {
            HandlePotentialTargets(potentialTargets, aliveTargets, m_EnemyTags, true);
        }

        // Handle potential ally targets
        if (potentialAllyTargets.Count > 0)
        {
            HandlePotentialTargets(potentialAllyTargets, aliveTargets, m_AllyTags, false);
        }

        if (potentialTargets.Count > 0)
        {
            blackboard.m_Requested = false;
            //result = children[0].Execute();
            //return result;
            return State.Success;
        }

        //No potential targets found
        ResetBlackboard();


        //result = children[0].Execute();
        //return result;
        return State.Success;
    }

    private void HandlePotentialTargets(List<GameObject> potentialTargetsList, List<GameObject> aliveColliders, List<string> tags, bool isEnemyLayer)
    {
        // Remove targets that are no longer within the max distance
        List<GameObject> targetsToRemove = new List<GameObject>();
        foreach (var potentialTarget in potentialTargetsList)
        {
            if (!aliveColliders.Contains(potentialTarget) && blackboard.IsTargetInTagList(potentialTarget, tags))
            {
                targetsToRemove.Add(potentialTarget);
            }
        }

        foreach (var targetToRemove in targetsToRemove)
        {
            potentialTargetsList.Remove(targetToRemove);
        }

        if (isEnemyLayer) blackboard.m_PotentialTargets =  potentialTargetsList;
        else blackboard.m_PotentialAllyTargets = potentialTargetsList;
    }

    private void ResetBlackboard()
    {
        m_SkillAnimationController.StopAttack();
        m_Target = null;
        blackboard.m_ClosestTarget = null;
    }
}
