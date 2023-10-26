using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using UnityEngine;

public class Split_SelectHealingDamage_Task : Node
{
    private Node currentChild;
    private new BehaviorTree tree;
    //private GameObject m_Target;
    public SkillAnimationController m_SkillAnimationController;
    public Fighter m_Fighter;
    public SkillBar_V2 m_SkillBar;

    public Split_SelectHealingDamage_Task(BehaviorTree tree)
    {
        this.tree = tree;
    }

    public override NodeResult Execute()
    {
        List<GameObject> potentialTargets = (List<GameObject>)tree.GetValue("PotentialTargets");
        List<GameObject> alliesInNeed = (List<GameObject>)tree.GetValue("AlliesInNeed");
        GameObject go = (GameObject)tree.GetValue("ClosestTarget");

        //if (go && go.GetComponent<Health>().IsDead())
        //{
        //    List<GameObject> targets = (List<GameObject>)tree.GetValue("PotentialTargets");
        //    targets.Remove(go);
        //    tree.SetValue("PotentialTargets", targets);

        //    tree.SetValue("ClosestTarget", null);
        //    m_Fighter.SetHasHitTarget(false);
        //    m_Fighter.m_Target = null;
        //    m_SkillAnimationController.StopAttack();
        //    m_SkillAnimationController.StopSkillAnimations();
        //    tree.SetValue("m_Requested", false);

        //    return NodeResult.FAILURE;
        //}

        //if (!m_SkillBar.IsSkillCompleted()) return NodeResult.SUCCESS;

        //select target to heal
        if (alliesInNeed.Count > 0)
        {
            //make currentchild equals the healing task
            //Debug.Log("Allies in need for group: " + m_Fighter.name + " :" + alliesInNeed.Count);
            currentChild = children[1];
        }
        //select target to damage
        else if (go || potentialTargets.Count > 0)
        {
            //make currentchild equals the attack task
            currentChild = children[0];
        }
        else
        {
            currentChild = children[2];
        }

        NodeResult result;
        if (currentChild != null)
            result = currentChild.Execute();
        else
        {
            result = NodeResult.FAILURE;
        }

        return result;
    }
}
