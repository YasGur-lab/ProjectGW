using System.Collections.Generic;

using GW.Attributes;
using GW.Combat;
using GW.Control;
using GW.Core;
using GW.Movement;
using GW.Saving;
using GW.Statistics;

using UnityEngine;
using UnityEngine.AI;


public class SkillUsage_AIControllerBT : BehaviorTree, ISaveable
{
    [Header("FIGHTER")]
    private Fighter m_Fighter;

    [Header("SkillBar")]
    //private SkillBar m_SkillBar;

    [Header("TARGET")]
    public List<GameObject> m_PotentialTargets;
    public GameObject m_ClosestTarget;
    private float m_TimeSinceLastTargetSelection;

    [Header("FOES LAYER")]
    private LayerMask m_TargetMask;

    [Header("AI SETTINGS")]
    private float m_ChaseDistance;
    private float m_MinimumAggroRange;
    private float m_TimeSinceLastChasingRequested;
    private bool m_ChasingRequested;
    private Transform m_LeftHandTransform;
    private Transform m_RightHandTransform;

    [Header("WEAPON SETTINGS")]
    private Weapon m_DefaultWeapon = null;
    private Weapon m_CurrentWeapon;

    private SkillAnimationController m_SkillAnimationController;

    // Start is called before the first frame update
    void Start()
    {
        m_Fighter = GetComponent<Fighter>();
        //m_SkillBar = GetComponent<SkillBar>();
        m_SkillAnimationController = GetComponent<SkillAnimationController>();

        m_CurrentWeapon = m_Fighter.GetWeapon();

        m_LeftHandTransform = m_Fighter.m_LeftHandTransform;
        m_RightHandTransform = m_Fighter.m_RightHandTransform;

        m_ChaseDistance = m_Fighter.m_ChaseDistance;
        m_MinimumAggroRange = m_Fighter.m_DangerZoneRadius;

        //m_TargetMask = m_Fighter.m_TargetMask;

        Sequence treeSelector = new Sequence();

        Remove_Target_Task removeTarget = new Remove_Target_Task();
        removeTarget.m_Transform = transform;
        removeTarget.m_MaxDistance = m_ChaseDistance;
        removeTarget.m_LayerMask = m_TargetMask;
        //removeTarget.m_Mover = GetComponent<Mover>();
        //removeTarget.m_Fighter = m_Fighter;
        //removeTarget.m_MinimumAggroRange = m_MinimumAggroRange;
        //removeTarget.m_SkillAnimationController = m_SkillAnimationController;

        IsInCombat_Task isInCombat = new IsInCombat_Task();
        isInCombat.m_Transform = transform;

        Detect_Targets_Task detectTarget = new Detect_Targets_Task();
        detectTarget.m_Transform = transform;
        //detectTarget.m_MaxDistance = m_ChaseDistance;
        detectTarget.m_LayerMask = m_TargetMask;
        detectTarget.m_MinimumAggroRange = m_MinimumAggroRange;

        Select_Target_Task selectTarget = new Select_Target_Task();
        selectTarget.m_Transform = transform;
        selectTarget.m_Fighter = m_Fighter;
        //selectTarget.m_WeaponRange = m_CurrentWeapon.GetRange();
        //selectTarget.m_MinimumAggroRange = m_MinimumAggroRange;
        //selectTarget.m_SkillAnimationController = m_SkillAnimationController;

        MoveTo_Target_Task moveToTarget = new MoveTo_Target_Task();
        moveToTarget.m_Transform = transform;
        moveToTarget.m_Mover = GetComponent<Mover>();
        moveToTarget.m_WeaponRange = m_CurrentWeapon.GetRange();

        //USE SKILL
        SkillUsage_Task skillUsage = new SkillUsage_Task(this);
        //skillUsage.m_SkillBar = m_SkillBar;
        skillUsage.m_Transform = transform;
        skillUsage.m_Weapon = m_CurrentWeapon;
        skillUsage.m_Fighter = m_Fighter;

        //ReturnToSpawnPoint_Task returnToSpawnPoint = new ReturnToSpawnPoint_Task();
        //returnToSpawnPoint.m_Transform = transform;
        //returnToSpawnPoint.m_Mover = GetComponent<Mover>();
        //returnToSpawnPoint.m_OriginalPosition = GetComponent<Mover>().GetInitialPosition();

        SetValue("PotentialTargets", m_PotentialTargets);
        SetValue("ClosestTarget", m_ClosestTarget);
        SetValue("TimeSinceLastTargetSelection", m_TimeSinceLastTargetSelection);
        SetValue("m_ChaseDistance", m_ChaseDistance);
        SetValue("m_LayerMasks", m_TargetMask);

        treeSelector.tree = this;
        removeTarget.tree = this;
        isInCombat.tree = this;
        detectTarget.tree = this;
        selectTarget.tree = this;
        moveToTarget.tree = this;
        skillUsage.tree = this;
        //returnToSpawnPoint.tree = this;

        treeSelector.children.Add(removeTarget);
        treeSelector.children.Add(isInCombat);
        treeSelector.children.Add(detectTarget);
        treeSelector.children.Add(selectTarget);
        treeSelector.children.Add(moveToTarget);
        treeSelector.children.Add(skillUsage);
        //treeSelector.children.Add(returnToSpawnPoint);

        root = treeSelector;

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
