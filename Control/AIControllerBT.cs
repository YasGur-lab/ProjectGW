using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using GW.Core;
using GW.Movement;
using GW.Statistics;
using UnityEngine;
using UnityEngine.AI;


public class AIControllerBT : BehaviorTree
{
    [Header("FIGHTER")] 
    private Fighter m_Fighter;
    private Health m_Health;

    [Header("SkillBar")]
    private static SkillBar_V2 m_SkillBar;

    [Header("TARGET")]
    public List<GameObject> m_PotentialTargets = new List<GameObject>();
    public List<GameObject> m_PotentialAllyTargets = new List<GameObject>();
    public List<GameObject> m_AlliesInNeed = new List<GameObject>();
    private GameObject m_ClosestTarget;
    private GameObject m_ClosestAllyInNeed;
    private float m_TimeSinceLastTargetSelection;

    [Header("FOES LAYER")]
    private LayerMask m_TargetMask;
    private LayerMask m_TargetAllyMask;
    private LayerMask m_DetectionLayer;

    [SerializeField] public List<string> m_EnemyTags = new List<string>();
    [SerializeField] public List<string> m_AllyTags = new List<string>();

    [Header("AI SETTINGS")]
    private bool m_CanOverlapSphere;
    private bool m_CanDetectTargets;
    private float m_ChaseDistance;
    private float m_MinimumAggroRange;
    private float m_TimeSinceLastChasingRequested;
    private bool m_ChasingRequested;
    private Transform m_LeftHandTransform;
    private Transform m_RightHandTransform;
    private float m_SkillUsageProbability;
    private float m_ChaseDuration;
    private SkillAnimationController m_SkillAnimationController;
    private float m_RoamingRadius;
    private Vector3 m_RoamingDestination = new Vector3();
    private Mover m_Mover;
    private float m_TimeBetweenNewDestination;
    private bool m_StationaryUnit;

    [Header("WEAPON SETTINGS")]
    private Weapon m_DefaultWeapon = null;
    private Weapon m_CurrentWeapon;
    
    void Awake()
    {
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        
        m_Fighter = transform.GetComponent<Fighter>();
        m_SkillBar = transform.GetComponent<SkillBar_V2>();
        m_SkillAnimationController = transform.GetComponent<SkillAnimationController>();
        m_Health = GetComponent<Health>();
        m_CurrentWeapon = m_Fighter.GetWeapon();
        m_Mover = GetComponent<Mover>();
        m_CanOverlapSphere = true;
        m_CanDetectTargets = true;

        m_LeftHandTransform = m_Fighter.m_LeftHandTransform;
        m_RightHandTransform = m_Fighter.m_RightHandTransform;

        m_ChaseDistance = m_Fighter.m_ChaseDistance;
        m_MinimumAggroRange = m_Fighter.m_DangerZoneRadius;

        //m_TargetMask = m_Fighter.m_TargetMask;
        //m_TargetAllyMask = m_Fighter.m_AllyTargetMask;
        m_DetectionLayer = m_Fighter.m_DetectionLayer;

        m_EnemyTags = m_Fighter.m_EnemyTags;
        m_AllyTags = m_Fighter.m_AllyTags;

        Sequence treeSelector = new Sequence();

        Remove_Target_Task removeTarget = new Remove_Target_Task();
        removeTarget.m_Transform = transform;
        removeTarget.m_MaxDistance = m_ChaseDistance;
        removeTarget.m_LayerMask = m_TargetMask;
        removeTarget.m_AllyLayerMask = m_TargetAllyMask;
        removeTarget.m_DetectionLayer = m_DetectionLayer;
        removeTarget.m_EnemyTags = m_EnemyTags;
        removeTarget.m_AllyTags = m_AllyTags;

        IsInCombat_Task isInCombat = new IsInCombat_Task();
        isInCombat.m_Transform = transform;
        isInCombat.m_Health = m_Health;
        isInCombat.m_Fighter = m_Fighter;

        Detect_Targets_Task detectTarget = new Detect_Targets_Task();
        detectTarget.m_Transform = transform;
        detectTarget.m_LayerMask = m_TargetMask;
        detectTarget.m_MinimumAggroRange = m_MinimumAggroRange;
        detectTarget.m_Fighter = m_Fighter;
        detectTarget.m_AllyLayerMask = m_TargetAllyMask;
        detectTarget.m_DetectionLayer = m_DetectionLayer;
        detectTarget.m_EnemyTags = m_EnemyTags;
        detectTarget.m_AllyTags = m_AllyTags;


        Split_TargetRoaming_Task splitTargetRoamingTarget = new Split_TargetRoaming_Task(this);
        splitTargetRoamingTarget.m_Skills = m_SkillBar.GetEquipedSkills();
        splitTargetRoamingTarget.m_SkillBar = m_SkillBar;
        splitTargetRoamingTarget.m_Fighter = m_Fighter;
        splitTargetRoamingTarget.m_StationaryUnit = m_StationaryUnit;

        Split_SelectHealingDamage_Task splitSelectHealingDamage = new Split_SelectHealingDamage_Task(this);
        splitSelectHealingDamage.m_Fighter = m_Fighter;
        splitSelectHealingDamage.m_SkillAnimationController = m_SkillAnimationController;
        splitSelectHealingDamage.m_SkillBar = m_SkillBar;

        Select_Target_Task selectTarget = new Select_Target_Task();
        selectTarget.m_Transform = transform;
        selectTarget.m_Fighter = m_Fighter;

        Select_TargetToSupport_Task selectTargetToSupport = new Select_TargetToSupport_Task();
        selectTargetToSupport.m_Transform = transform;
        selectTargetToSupport.m_Fighter = m_Fighter;

        Split_Task splitTask = new Split_Task(this);
        splitTask.skillUsageProbability = m_SkillUsageProbability;

        WeaponAttack_Task weaponAttack = new WeaponAttack_Task(this);
        weaponAttack.m_Transform = transform;
        weaponAttack.m_Weapon = m_CurrentWeapon;
        weaponAttack.m_Stats = GetComponent<BaseStats>();
        weaponAttack.m_LeftHandTransform = m_LeftHandTransform;
        weaponAttack.m_RightHandTransform = m_RightHandTransform;
        weaponAttack.m_NavMeshAgent = GetComponent<NavMeshAgent>();
        weaponAttack.m_Fighter = m_Fighter;
        weaponAttack.m_Scheduler = GetComponent<ActionScheduler>();
        weaponAttack.m_SkillAnimationController = m_SkillAnimationController;
        weaponAttack.m_SkillBar = m_SkillBar;

        //SELECT SKILL
        SkillSelection_Task skillSelection = new SkillSelection_Task(this);
        skillSelection.m_SkillBar = m_SkillBar;
        skillSelection.m_Transform = transform;
        skillSelection.m_Weapon = m_CurrentWeapon;
        skillSelection.m_Fighter = m_Fighter;
        skillSelection.m_Energy = GetComponent<Energy>();
        skillSelection.m_Skills = m_SkillBar.GetEquipedSkills();

        //USE SKILL
        SkillUsage_Task skillUsage = new SkillUsage_Task(this);
        skillUsage.m_SkillBar = m_SkillBar;
        skillUsage.m_Transform = transform;
        skillUsage.m_Weapon = m_CurrentWeapon;
        skillUsage.m_Fighter = m_Fighter;
        skillUsage.m_Energy = GetComponent<Energy>();
        skillUsage.m_Skills = m_SkillBar.GetEquipedSkills();

        SupportiveSkillSelection_Task supportiveSkillSelection = new SupportiveSkillSelection_Task(this);
        supportiveSkillSelection.m_SkillBar = m_SkillBar;
        supportiveSkillSelection.m_Transform = transform;
        supportiveSkillSelection.m_Weapon = m_CurrentWeapon;
        supportiveSkillSelection.m_Fighter = m_Fighter;
        supportiveSkillSelection.m_Energy = GetComponent<Energy>();
        supportiveSkillSelection.m_Skills = m_SkillBar.GetEquipedSkills();


        SupportiveSkillUsage_Task supportiveSkillUsage = new SupportiveSkillUsage_Task(this);
        supportiveSkillUsage.m_SkillBar = m_SkillBar;
        supportiveSkillUsage.m_Transform = transform;
        supportiveSkillUsage.m_Weapon = m_CurrentWeapon;
        supportiveSkillUsage.m_Fighter = m_Fighter;
        supportiveSkillUsage.m_Energy = GetComponent<Energy>();
        supportiveSkillUsage.m_Skills = m_SkillBar.GetEquipedSkills();


        MoveTo_Target_Task moveToTarget = new MoveTo_Target_Task();
        moveToTarget.m_Transform = transform;
        moveToTarget.m_Mover = GetComponent<Mover>();
        moveToTarget.m_MoverV2 = GetComponent<Mover_V2>();
        moveToTarget.m_WeaponRange = m_CurrentWeapon.GetRange();
        moveToTarget.m_MinimumAggroRange = m_MinimumAggroRange;
        moveToTarget.m_ChaseDistance = m_ChaseDistance;

        MoveTo_TargetToSupport_Task moveToTargetToSupport = new MoveTo_TargetToSupport_Task();
        moveToTargetToSupport.m_Transform = transform;
        moveToTargetToSupport.m_Mover = GetComponent<Mover>();
        moveToTargetToSupport.m_MoverV2 = GetComponent<Mover_V2>();
        moveToTargetToSupport.m_WeaponRange = m_CurrentWeapon.GetRange();
        moveToTargetToSupport.m_MinimumAggroRange = m_MinimumAggroRange;

        Roaming_Task roamingTask = new Roaming_Task(this);
        roamingTask.m_Transform = transform;
        roamingTask.m_Mover = GetComponent<Mover>();
        roamingTask.m_MoverV2 = GetComponent<Mover_V2>();
        roamingTask.m_RoamingRadius = m_RoamingRadius;
        roamingTask.m_Scheduler = GetComponent<ActionScheduler>();
        roamingTask.m_OriginalPosition = GetComponent<Mover_V2>().GetInitialPosition();
        roamingTask.m_Fighter = m_Fighter;
        roamingTask.m_TimeBetweenNewDestination = m_TimeBetweenNewDestination;
        //roamingTask.m_Party = m_PartySystem;

        Select_RoamingFollowLeader_Task splitRoamingFollowLeader = new Select_RoamingFollowLeader_Task();
        splitRoamingFollowLeader.m_Transform = transform;
        //splitRoamingFollowLeader.m_Party = m_PartySystem;

        FollowLeader_Task followLeaderTask = new FollowLeader_Task(this);
        followLeaderTask.m_Transform = transform;
        //followLeaderTask.m_Party = m_PartySystem;
        followLeaderTask.m_Mover = GetComponent<Mover>();
        followLeaderTask.m_MoverV2 = GetComponent<Mover_V2>();
        followLeaderTask.m_RoamingRadius = m_RoamingRadius;
        followLeaderTask.m_Scheduler = GetComponent<ActionScheduler>();
        followLeaderTask.m_OriginalPosition = GetComponent<Mover_V2>().GetInitialPosition();
        followLeaderTask.m_Fighter = m_Fighter;
        followLeaderTask.m_TimeBetweenNewDestination = m_TimeBetweenNewDestination;

        isInCombat.children.Add(removeTarget);
        isInCombat.children.Add(selectTarget);

        removeTarget.children.Add(detectTarget);

        detectTarget.children.Add(splitTargetRoamingTarget);

        splitTargetRoamingTarget.children.Add(splitSelectHealingDamage);
        splitTargetRoamingTarget.children.Add(splitRoamingFollowLeader);

        splitSelectHealingDamage.children.Add(selectTarget);
        splitSelectHealingDamage.children.Add(selectTargetToSupport);
        splitSelectHealingDamage.children.Add(splitRoamingFollowLeader);

        splitRoamingFollowLeader.children.Add(roamingTask);
        splitRoamingFollowLeader.children.Add(followLeaderTask);

        skillSelection.children.Add(moveToTarget);
        skillSelection.children.Add(skillUsage);

        supportiveSkillSelection.children.Add(moveToTargetToSupport);
        supportiveSkillSelection.children.Add(supportiveSkillUsage);

        //skill selection into get in range into skillusage. we need to divide skill usage in 2
        selectTarget.children.Add(skillSelection);

        selectTargetToSupport.children.Add(supportiveSkillSelection);

        moveToTarget.children.Add(skillUsage);
        //moveToTarget.children.Add(weaponAttack);

        //supportiveSkillUsage.children.Add(skillUsage);

        moveToTargetToSupport.children.Add(supportiveSkillUsage);


        //weaponAttack.children.Add(moveToTarget);
        //splitTask.children.Add(skillSelection);
        //splitTask.children.Add(moveToTarget);

        SetValue("ChasingRequested", false);
        SetValue("PotentialTargets", m_PotentialTargets);
        SetValue("PotentialAllyTargets", m_PotentialAllyTargets);
        SetValue("AlliesInNeed", m_AlliesInNeed);
        SetValue("ClosestTarget", m_ClosestTarget);
        SetValue("ClosestAllyInNeed", m_ClosestAllyInNeed);
        SetValue("m_ChaseDistance", m_ChaseDistance);
        SetValue("m_LayerMasks", m_TargetMask);
        SetValue("DetectionLMask", m_DetectionLayer);
        SetValue("MaxChasingDuration", m_ChaseDuration);
        SetValue("RoamingDestination", m_RoamingDestination);
        SetValue("m_Requested", false);
        SetValue("CanOverlapSphere", m_CanOverlapSphere);
        SetValue("CanDetectTargets", m_CanDetectTargets);

        treeSelector.tree = this;
        removeTarget.tree = this;
        isInCombat.tree = this;
        detectTarget.tree = this;
        splitTargetRoamingTarget.tree = this;
        splitSelectHealingDamage.tree = this;
        selectTarget.tree = this;
        selectTargetToSupport.tree = this;
        moveToTarget.tree = this;
        moveToTargetToSupport.tree = this;
        //splitTask.tree = this;

        treeSelector.children.Add(isInCombat);
        treeSelector.children.Add(removeTarget);
        //treeSelector.children.Add(isInCombat);
        treeSelector.children.Add(detectTarget);
        treeSelector.children.Add(splitTargetRoamingTarget);
        //treeSelector.children.Add(splitSelectHealingDamage);
        //treeSelector.children.Add(selectTarget);
        //treeSelector.children.Add(selectTargetToSupport);
        //treeSelector.children.Add(moveToTarget);
        //treeSelector.children.Add(moveToTargetToSupport);
        //treeSelector.children.Add(splitTask);

        root = treeSelector;

        float cooldownUpdateInterval = 20.0f; // Update every 0.1 seconds (adjust as needed)
        InvokeRepeating("UpdateRemoveTask", cooldownUpdateInterval, cooldownUpdateInterval);
        cooldownUpdateInterval = 2.0f; // Update every 0.1 seconds (adjust as needed)
        InvokeRepeating("UpdateDetectTask", cooldownUpdateInterval, cooldownUpdateInterval);
    }

    public void UpdateRemoveTask()
    {
        //Debug.Log("UpdateRemoveTask");
        SetValue("CanOverlapSphere", true);
    }

    public void UpdateDetectTask()
    {
        //Debug.Log("UpdateDetectTask");
        SetValue("CanDetectTargets", true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_ChaseDistance);
        Gizmos.color = Color.green;
        if (m_CurrentWeapon)
            Gizmos.DrawWireSphere(transform.position, m_CurrentWeapon.GetRange());
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, m_MinimumAggroRange);
        Gizmos.color = Color.red;
        if (m_Fighter.m_Target)
            Gizmos.DrawSphere(m_Fighter.m_Target.transform.position + Vector3.up, 0.2f);
        Gizmos.color = Color.green;
        if (m_Fighter.m_AllyTarget)
            Gizmos.DrawSphere(m_Fighter.m_AllyTarget.transform.position + Vector3.up, 0.2f);
    }

    public override void ResetBehabviorTree(string taskIdentifier, int depth = 0)
    {
        switch (taskIdentifier)
        {
            case "Remove_Target_Task":
                ResetRemoveTargetTaskVariables(depth);
                break;
            case "Select_Target_Task":
                ResetSelectTargetTaskVariables(depth);
                break;
            case "MoveTo_Target_Task":
                ResetMoveTargetTaskVariables(depth);
                break;
            case "WeaponAttack_Task":
                ResetAutoAttackTargetTaskVariables(depth);
                break;
            // Add more cases for other tasks if needed
            // ...
            default:
                // If no specific task identifier matches, reset common variables
                ResetCommonVariables();
                break;
        }
    }

    private void ResetRemoveTargetTaskVariables(int depth = 0)
    {
        if (depth == 0 || depth == 2)
        {
            //Debug.Log("ResetRemoveTargetTaskVariables");
            //m_SkillAnimationController.StopSkillAnimations();
            m_SkillAnimationController.StopAttack();
            m_Fighter.m_Target = null;
            SetValue("ClosestTarget", null);
        }

        if(depth != 2)
            SetValue("m_Requested", false);
    }

    private void ResetSelectTargetTaskVariables(int depth = 0)
    {
        if (depth == 0)
        {
            m_SkillAnimationController.StopAttack();
        }
    }


    private void ResetMoveTargetTaskVariables(int depth = 0)
    {
        if (depth == 0)
        {
            List<GameObject> targets = (List<GameObject>)GetValue("PotentialTargets");
            targets.Clear();
            SetValue("PotentialTargets", targets);
            SetValue("m_Requested", false);
            SetValue("ClosestTarget", null);
            m_Fighter.m_Target = null;
        }
    }

    private void ResetAutoAttackTargetTaskVariables(int depth = 0)
    {
        if (depth == 1)
        {
            SetValue("ClosestTarget", null);
            m_Health.SetTookDamage(false);
            m_Health.SetInstigator(null);
            m_Fighter.SetHasHitTarget(false);
            m_Fighter.m_Target = null;
            //Debug.Log("ResetAutoAttackTargetTaskVariables");
            m_SkillAnimationController.StopAttack();
            //m_SkillAnimationController.StopSkillAnimations();
        }
        SetValue("m_Requested", false);
    }
    
    private void ResetCommonVariables()
    {
        List<GameObject> targets = (List<GameObject>)GetValue("PotentialTargets");
        targets.Clear();
        SetValue("PotentialTargets", targets);
        List<GameObject> potentialAllyTargets = (List<GameObject>)GetValue("PotentialAllyTargets");
        potentialAllyTargets.Clear();
        SetValue("PotentialAllyTargets", potentialAllyTargets);
        //List<GameObject> alliesInNeed = (List<GameObject>)GetValue("AlliesInNeed");
        //alliesInNeed.Clear();
        //SetValue("AlliesInNeed", alliesInNeed);
        SetValue("ClosestTarget", null);
        SetValue("ClosestAllyInNeed", m_ClosestAllyInNeed);

        SetValue("m_Requested", false);

        m_Health.SetTookDamage(false);
        m_Health.SetInstigator(null);

        m_Fighter.AttackRequested = false;
        m_Fighter.SetHasHitTarget(false);
        m_Fighter.m_Target = null;
        m_Fighter.m_AllyTarget = null;

        m_SkillAnimationController.StopAttack();
        m_SkillAnimationController.ResetCharacterAfterDeath();

        m_HasStopped = false;
    }

    //public override void ResetBehabviorTree()
    //{
    //    SetValue("m_Requested", false);
    //    SetValue("ClosestTarget", null);
    //    List<GameObject> targets = (List<GameObject>)GetValue("PotentialTargets");
    //    targets.Clear();
    //    SetValue("PotentialTargets", m_PotentialTargets);

    //    m_Health.SetTookDamage(false);
    //    m_Health.SetInstigator(null);

    //    m_Fighter.AttackRequested = false;
    //    m_Fighter.SetHasHitTarget(false);
    //    m_Fighter.m_Target = null;

    //    m_SkillAnimationController.StopAttack();
    //    m_SkillAnimationController.ResetCharacterAfterDeath();

    //    m_HasStopped = false;
    //}

    public void SetSkillUsageProbability(float f) { m_SkillUsageProbability = f; }
    public void SetChaseDuration(float f) { m_ChaseDuration = f; }

    public void SetRoamingRadius(float f) { m_RoamingRadius = f; }

    public void SetTimeBetweenNewDestination(float f) { m_TimeBetweenNewDestination = f; }

    public void SetIsStationaryUnit(bool stationaryUnit)
    {
        m_StationaryUnit = stationaryUnit;
    }
}
