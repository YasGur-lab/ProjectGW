using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class SupportiveSkillSelection_Task : Task
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


    public SupportiveSkillSelection_Task(BehaviorTree tree)
    {
        this.tree = tree;
    }

    public override NodeResult Execute()
    {
        if (m_Transform.GetComponent<Health>().IsDead())
        {
            return NodeResult.STOP;
        }

        //if (m_Fighter.IsCasting()) { return NodeResult.FAILURE; }

        m_Target = (GameObject)tree.GetValue("ClosestAllyInNeed");
        if (m_Target == null)
        {
            //tree.ResetBehabviorTree("Select_Target_Task", 0);
            tree.SetValue("m_Requested", false);
            return NodeResult.FAILURE;
        }

        if (!m_Target.GetComponent<Health>().IsDead())
        {
            if (m_SkillBar.IsSkillCompleted())
            {

                Skill skillToActivate = (Skill)tree.GetValue("CurrentSkillInUse");
                if (skillToActivate == null && m_SkillBar.GetCurrentSkillIndex() == -1)
                {
                    skillToActivate = GetRandomSkill();
                    if (skillToActivate != null)
                    {
                        int cost = skillToActivate.GetEnergyCost();
                        if (!m_Energy.HasEnoughEnergy(cost))
                        {
                            return NodeResult.FAILURE;
                        }

                        if (!m_SkillBar.GetEquipedSkills().Contains(skillToActivate))
                        {
                            return NodeResult.FAILURE;
                        }

                        tree.SetValue("CurrentSkillInUse", skillToActivate);
                        if (!IsInRangeFromSkill(skillToActivate))
                        {
                            //get in range
                            children[0].Execute();
                        }
                        else
                        {
                            children[1].Execute();
                        }

                    }

                    return NodeResult.FAILURE;
                }
                if (skillToActivate)
                {
                    if (!IsInRangeFromSkill(skillToActivate))
                    {
                        //get in range

                        children[0].Execute();
                    }
                    else
                    {
                        //Debug.Log("Using: " + skillToActivate.name);
                        children[1].Execute();
                    }
                }
                return NodeResult.FAILURE;
            }
        }

        tree.SetValue("m_Requested", false);

        return NodeResult.FAILURE;
    }

    public Skill GetRandomSkill()
    {
        List<Skill> readySkills = new List<Skill>();
        m_SkillStates = m_SkillBar.GetSkillStates();
        m_Skills = m_SkillBar.GetEquipedSkills();
        HealthThresholds targetHealthThreshold = m_Target.GetComponent<Health>().HealthThreshold;
        List<Effect> effects = m_Target.GetComponent<EffectComponent>().GetActiveEffects();

        HashSet<Skill> skillsUnderEffect = new HashSet<Skill>();
        foreach (var effect in effects)
        {
            if (effect.FromSkill != null)
            {
                skillsUnderEffect.Add(effect.FromSkill);
            }
        }
        
        bool isSelfCasting = m_Target == m_Transform.gameObject;

        foreach (var skill in m_Skills)
        {
            if (skill == null) continue;

            if (m_SkillStates[skill] == SkillBar_V2.SkillState.ready && skill.IsASupportiveSkill &&
                (skill.HealthThreshold <= targetHealthThreshold ||
                 skill.HealthThreshold == HealthThresholds.LowAlert) &&
                (!isSelfCasting || skill.IsSelfTarget) &&
                !skillsUnderEffect.Contains(skill))
            {
                readySkills.Add(skill);
            }
        }

        if (readySkills.Count > 0)
        {
            int index = Random.Range(0, readySkills.Count);
            return readySkills[index];
        }

        return null;
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
    private bool IsInRangeFromSkill(Skill skill)
    {
        float distance = Vector3.Distance(m_Target.transform.position, m_Transform.position);
        distance = Mathf.Round(distance * 10.0f) / 10.0f;
        if (distance <= skill.GetRange())
        {
            return true;

        }
        return false;
    }
}
