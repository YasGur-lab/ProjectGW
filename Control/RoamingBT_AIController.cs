using System.Collections.Generic;

using GW.Attributes;
using GW.Combat;
using GW.Core;
using GW.Movement;
using GW.Saving;
using GW.Statistics;

using UnityEngine;
using UnityEngine.AI;


public class RoamingBT_AIController : BehaviorTree, ISaveable
{
    [Header("TARGET")] 
    public float m_RoamingRadius;
    private Vector3 m_RoamingDestination;

    private Transform m_StartingTransform;

    // Start is called before the first frame update
    void Start()
    {
        m_StartingTransform = transform;
        Sequence treeSelector = new Sequence();

        IsInCombat_Task isInCombat = new IsInCombat_Task();
        isInCombat.m_Transform = transform;

        Roaming_Task romaingTask = new Roaming_Task(this);
        romaingTask.m_Transform = transform;
        romaingTask.m_Mover = GetComponent<Mover>();
        romaingTask.m_RoamingRadius = m_RoamingRadius;
        romaingTask.m_Scheduler = GetComponent<ActionScheduler>();
        romaingTask.m_OriginalPosition = GetComponent<Mover>().GetInitialPosition();

        SetValue("RoamingDestination", m_RoamingDestination);
        //SetValue("StartingTransform", m_StartingTransform);

        treeSelector.tree = this;
        romaingTask.tree = this;

        treeSelector.children.Add(romaingTask);

        root = treeSelector;
    }

    //public override void Update()
    //{
    //    base.Update();
    //}

    public object CaptureState()
    {
        return null;
    }

    public void RestoreState(object state)
    {
    }

    //Called by unity
    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawWireSphere(transform.position, m_SpawnRadius);
    //    //Vector3 pos = (Vector3)GetValue("RoamingDestination");
    //    //Gizmos.DrawSphere((Vector3)GetValue("RoamingDestination"), 1.0f);
    //}
}
