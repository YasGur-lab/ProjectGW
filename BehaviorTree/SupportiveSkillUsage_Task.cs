using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class SupportiveSkillUsage_Task : Task
{
    public SkillBar_V2 m_SkillBar;
    private Dictionary<Skill, SkillBar_V2.SkillState> m_SkillStates = new Dictionary<Skill, SkillBar_V2.SkillState>();
    public List<Skill> m_Skills = new List<Skill>();
    public Transform m_Transform;
    public Weapon m_Weapon;
    private GameObject m_Target;
    private int m_SkillIndex;
    public Fighter m_Fighter;
    private new BehaviorTree tree;
    public Energy m_Energy;


    public SupportiveSkillUsage_Task(BehaviorTree tree)
    {
        this.tree = tree;
    }

    public override NodeResult Execute()
    {
        if (m_Transform.GetComponent<Health>().IsDead())
        {
            return NodeResult.STOP;
        }
        
        m_Target = (GameObject)tree.GetValue("ClosestAllyInNeed");
        if (m_Target == null)
        {
            tree.SetValue("m_Requested", false);
            return NodeResult.FAILURE;
        }

        if (!m_Target.GetComponent<Health>().IsDead())
        {
            Skill skillToActivate = (Skill)tree.GetValue("CurrentSkillInUse");
            if (skillToActivate != null)
            {
                int cost = skillToActivate.GetEnergyCost();
                if (!m_Energy.HasEnoughEnergy(cost))
                {
                    NodeResult result = children[0].Execute();
                    return result;
                }

                if (!m_SkillBar.GetEquipedSkills().Contains(skillToActivate)) { return NodeResult.FAILURE; }
                LookAtTarget(m_Target.transform.position);
                m_SkillBar.ActivateSkill(skillToActivate);
                tree.SetValue("CurrentSkillInUse", null);
                return NodeResult.SUCCESS;
            }
            return NodeResult.FAILURE;
        }

        tree.SetValue("m_Requested", false);

        return NodeResult.FAILURE;
    }
    
    private void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - m_Transform.position;
        direction.y = 0.0f;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            m_Transform.rotation = targetRotation;
        }
    }
}
