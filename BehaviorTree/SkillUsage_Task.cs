using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using GW.Movement;
using UnityEngine;

public class SkillUsage_Task : Task
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
    public Mover m_Mover;

    public SkillUsage_Task(BehaviorTree tree)
    {
        this.tree = tree;
    }

    public override NodeResult Execute()
    {
        if (m_Transform.GetComponent<Health>().IsDead())
        {
            return NodeResult.STOP;
        }

        if (m_SkillBar.IsSkillCompleted())
        {

            m_Target = (GameObject)tree.GetValue("ClosestTarget");
            if (m_Target == null)
            {
                tree.ResetBehabviorTree("Select_Target_Task", 0);
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
                        return NodeResult.FAILURE;
                    }
                    
                    LookAtTarget(m_Target.transform.position);
                    m_SkillBar.ActivateSkill(skillToActivate);
                    tree.SetValue("CurrentSkillInUse", null);
                    return NodeResult.SUCCESS;
                }

                return NodeResult.FAILURE;
            }
        }

        tree.SetValue("m_Requested", false);
        return NodeResult.FAILURE;
    }
    
    private void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - m_Transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            m_Transform.rotation = targetRotation;
        }
    }
}
