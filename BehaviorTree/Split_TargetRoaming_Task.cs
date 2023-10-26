using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using UnityEngine;

public class Split_TargetRoaming_Task : Node
{
    private Node currentChild;
    private new BehaviorTree tree;

    public List<Skill> m_Skills = new List<Skill>();
    public SkillBar_V2 m_SkillBar;
    public Fighter m_Fighter;
    public bool m_StationaryUnit;

    public Split_TargetRoaming_Task(BehaviorTree tree)
    {
        this.tree = tree;
    }

    public override NodeResult Execute()
    {
        List<GameObject> potentialTargets = (List<GameObject>)tree.GetValue("PotentialTargets");
        GameObject target = (GameObject)tree.GetValue("ClosestTarget");
        NodeResult result;

        if (HasASupportiveSkillReady())
        {
            List<GameObject> potentialAllyTargets = (List<GameObject>)tree.GetValue("PotentialAllyTargets");
            List<GameObject> AllyTargetsInNeeds = new List<GameObject>();
            foreach (var potentialAllyTarget in potentialAllyTargets)
            {
                Health health = potentialAllyTarget.GetComponent<Health>();

                float maxHealth = health.GetOriginalHealth();
                float currentHealth = health.GetHealth();
                float healthThreshold = 0.75f * maxHealth; // 30% of max health

                if (currentHealth <= healthThreshold)
                {
                    AllyTargetsInNeeds.Add(potentialAllyTarget);
                }
            }

            tree.SetValue("AlliesInNeed", AllyTargetsInNeeds);
            //Debug.Log("alliesInNeed: " + AllyTargetsInNeeds.Count);
            if (target || potentialTargets.Count > 0 || AllyTargetsInNeeds.Count > 0)
            {
                //make currentchild equals the target task
                //currentChild = children[0];
                 result = children[0].Execute();
                return result;
            }
            else if (!m_StationaryUnit)
            {
                //make currentchild equals the roaming task
                result = children[1].Execute();
                return result;
            }
        }

        if (target || potentialTargets.Count > 0)
        {
            //make currentchild equals the target task
            result = children[0].Execute();
            return result;
        }
        else if(!m_StationaryUnit)
        {
            //make currentchild equals the roaming task
            result = children[1].Execute();
            return result;
        }
        else
        {
            return NodeResult.SUCCESS;
        }


        result = currentChild.Execute();
        return result;
    }

    private bool HasASupportiveSkillReady()
    {
        foreach (var skill in m_Skills)
        {
            if (skill == null) continue;
            if (m_SkillBar.GetSkillStates()[skill] == SkillBar_V2.SkillState.ready && skill.IsASupportiveSkill)
            {
                return true;
            }
        }

        return false;
    }
}
