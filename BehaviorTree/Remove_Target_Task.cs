using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using GW.Movement;
using Unity.VisualScripting;
using UnityEngine;

public class Remove_Target_Task : Task
{
    public Transform m_Transform;
    public float m_MaxDistance;
    public LayerMask m_LayerMask;
    public LayerMask m_AllyLayerMask;
    public bool m_CanOverlapSphere;
    public LayerMask m_DetectionLayer;
    public List<string> m_EnemyTags;
    public List<string> m_AllyTags;
    
    public override NodeResult Execute()
    {
        if (m_Transform.gameObject.GetComponent<Health>().IsDead())
        {
            return NodeResult.STOP;
        }


        bool canOverlapSphereb = (bool)tree.GetValue("CanOverlapSphere");
        if (!canOverlapSphereb)
        {
            //result = children[0].Execute();
            return NodeResult.SUCCESS;
        }

        NodeResult result;
        List<GameObject> potentialAllyTargets = (List<GameObject>)tree.GetValue("PotentialAllyTargets");
        List<GameObject> potentialTargets = (List<GameObject>)tree.GetValue("PotentialTargets");

        //LayerMask combinedLayerMask = m_LayerMask | m_AllyLayerMask;
        Collider[] colliders =
            Physics.OverlapSphere(m_Transform.position, m_MaxDistance, m_DetectionLayer);

        //Debug.Log("m_CanOverlapSphere: " + canOverlapSphereb);
        tree.SetValue("CanOverlapSphere", false);

        // Check if there are any alive targets within the max distance
        if (colliders.Length == 0)
        {
            tree.ResetBehabviorTree("Remove_Target_Task", 0);
            result = children[0].Execute();
            return result;
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
                    if (tree.IsTargetInTagList(target, m_EnemyTags) || tree.IsTargetInTagList(target, m_AllyTags))
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
            tree.ResetBehabviorTree("Remove_Target_Task", 1);
            result = children[0].Execute();
            return result;
        }

        //No potential targets found
        tree.ResetBehabviorTree("Remove_Target_Task", 2);
        result = children[0].Execute();
        return result;

        //if (potentialTargets.Count > 0)
        //{
        //    //Collider[] colliders =
        //    //    Physics.OverlapSphere(m_Transform.position, m_MaxDistance, m_LayerMask);
        //    //List<GameObject> aliveColliders = new List<GameObject>();

        //    //Remove dead targets from potentialTargets
        //    foreach (var collider in colliders)
        //    {
        //        if (collider.GetComponent<Health>().IsDead())
        //        {
        //            potentialTargets.Remove(collider.gameObject);
        //            continue;
        //        }

        //        aliveColliders.Add(collider.gameObject);
        //    }

        //    // Check if there are any alive targets within the max distance
        //    if (aliveColliders.Count == 0)
        //    {
        //        tree.ResetBehabviorTree("Remove_Target_Task", 0);
        //        return NodeResult.SUCCESS;
        //    }

        //    // Remove targets that are no longer within the max distance
        //    List<GameObject> targetsToRemove = new List<GameObject>();
        //    foreach (var potentialTarget in potentialTargets)
        //    {
        //        if (!aliveColliders.Contains(potentialTarget))
        //        {
        //            targetsToRemove.Add(potentialTarget);
        //        }
        //    }

        //    foreach (var targetToRemove in targetsToRemove)
        //    {
        //        potentialTargets.Remove(targetToRemove);
        //    }

        //    tree.ResetBehabviorTree("Remove_Target_Task", 1);
        //    return NodeResult.SUCCESS;
        //}

        //// No potential targets found
        //tree.ResetBehabviorTree("Remove_Target_Task", 2);
        //return NodeResult.SUCCESS;
    }

    private void HandlePotentialTargets(List<GameObject> potentialTargetsList, List<GameObject> aliveColliders, List<string> tags, bool isEnemyLayer)
    {
        // Remove targets that are no longer within the max distance
        List<GameObject> targetsToRemove = new List<GameObject>();
        foreach (var potentialTarget in potentialTargetsList)
        {
            if (!aliveColliders.Contains(potentialTarget) && tree.IsTargetInTagList(potentialTarget, tags))
            {
                targetsToRemove.Add(potentialTarget);
            }
        }

        foreach (var targetToRemove in targetsToRemove)
        {
            potentialTargetsList.Remove(targetToRemove);
        }

        if (isEnemyLayer) tree.SetValue("PotentialTargets", potentialTargetsList);
        else tree.SetValue("PotentialAllyTargets", potentialTargetsList);
    }
}

//public override NodeResult Execute()
//{
//    if (m_Transform.gameObject.GetComponent<Health>().IsDead())
//    {
//        return NodeResult.STOP;
//    }


//    bool canOverlapSphereb = (bool)tree.GetValue("CanOverlapSphere");
//    if (!canOverlapSphereb)
//    {
//        //result = children[0].Execute();
//        return NodeResult.SUCCESS;
//    }

//    NodeResult result;
//    List<GameObject> potentialAllyTargets = (List<GameObject>)tree.GetValue("PotentialAllyTargets");
//    List<GameObject> potentialTargets = (List<GameObject>)tree.GetValue("PotentialTargets");

//    LayerMask combinedLayerMask = m_LayerMask | m_AllyLayerMask;
//    Collider[] colliders =
//        Physics.OverlapSphere(m_Transform.position, m_MaxDistance, combinedLayerMask);

//    //Debug.Log("m_CanOverlapSphere: " + canOverlapSphereb);
//    tree.SetValue("CanOverlapSphere", false);

//    // Check if there are any alive targets within the max distance
//    if (colliders.Length == 0)
//    {
//        tree.ResetBehabviorTree("Remove_Target_Task", 0);
//        result = children[0].Execute();
//        return result;
//    }

//    // Filter colliders to only include alive targets
//    List<GameObject> aliveTargets = colliders
//        .Select(collider => collider.gameObject.GetComponent<Health>())
//        .Where(health => health && !health.IsDead())
//        .Select(health => health.gameObject)
//        .ToList();

//    if (potentialTargets.Count > 0)
//    {
//        HandlePotentialTargets(potentialTargets, aliveTargets, m_LayerMask, true);
//    }

//    // Handle potential ally targets
//    if (potentialAllyTargets.Count > 0)
//    {
//        HandlePotentialTargets(potentialAllyTargets, aliveTargets, m_AllyLayerMask, false);
//    }

//    if (potentialTargets.Count > 0)
//    {
//        tree.ResetBehabviorTree("Remove_Target_Task", 1);
//        result = children[0].Execute();
//        return result;
//    }

//    //No potential targets found
//    tree.ResetBehabviorTree("Remove_Target_Task", 2);
//    result = children[0].Execute();
//    return result;

//    //if (potentialTargets.Count > 0)
//    //{
//    //    //Collider[] colliders =
//    //    //    Physics.OverlapSphere(m_Transform.position, m_MaxDistance, m_LayerMask);
//    //    //List<GameObject> aliveColliders = new List<GameObject>();

//    //    //Remove dead targets from potentialTargets
//    //    foreach (var collider in colliders)
//    //    {
//    //        if (collider.GetComponent<Health>().IsDead())
//    //        {
//    //            potentialTargets.Remove(collider.gameObject);
//    //            continue;
//    //        }

//    //        aliveColliders.Add(collider.gameObject);
//    //    }

//    //    // Check if there are any alive targets within the max distance
//    //    if (aliveColliders.Count == 0)
//    //    {
//    //        tree.ResetBehabviorTree("Remove_Target_Task", 0);
//    //        return NodeResult.SUCCESS;
//    //    }

//    //    // Remove targets that are no longer within the max distance
//    //    List<GameObject> targetsToRemove = new List<GameObject>();
//    //    foreach (var potentialTarget in potentialTargets)
//    //    {
//    //        if (!aliveColliders.Contains(potentialTarget))
//    //        {
//    //            targetsToRemove.Add(potentialTarget);
//    //        }
//    //    }

//    //    foreach (var targetToRemove in targetsToRemove)
//    //    {
//    //        potentialTargets.Remove(targetToRemove);
//    //    }

//    //    tree.ResetBehabviorTree("Remove_Target_Task", 1);
//    //    return NodeResult.SUCCESS;
//    //}

//    //// No potential targets found
//    //tree.ResetBehabviorTree("Remove_Target_Task", 2);
//    //return NodeResult.SUCCESS;
//}

//private void HandlePotentialTargets(List<GameObject> potentialTargetsList, List<GameObject> aliveColliders, LayerMask layerMask, bool isEnemyLayer)
//{
//    // Remove targets that are no longer within the max distance
//    List<GameObject> targetsToRemove = new List<GameObject>();
//    foreach (var potentialTarget in potentialTargetsList)
//    {
//        if (!aliveColliders.Contains(potentialTarget) && ((1 << potentialTarget.layer) & layerMask) != 0)
//        {
//            targetsToRemove.Add(potentialTarget);
//        }
//    }

//    foreach (var targetToRemove in targetsToRemove)
//    {
//        potentialTargetsList.Remove(targetToRemove);
//    }

//    if(isEnemyLayer) tree.SetValue("PotentialTargets", potentialTargetsList);
//    else tree.SetValue("PotentialAllyTargets", potentialTargetsList);
//}
