using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using GW.Control;
using UnityEngine;

public class SkillSelection_Task : Task
{
    public SkillBar_V2 m_SkillBar;
    private Dictionary<Skill, SkillBar_V2.SkillState> m_SkillStates = new Dictionary<Skill, SkillBar_V2.SkillState>();
    private Dictionary<Skill, SkillBar_V2.SkillState> m_WeaponSkillState = new Dictionary<Skill, SkillBar_V2.SkillState>();
    public List<Skill> m_Skills = new List<Skill>();
    public Transform m_Transform;
    public Weapon m_Weapon;
    private GameObject m_Target;
    private int m_SkillIndex;
    public Fighter m_Fighter;
    private new BehaviorTree tree;
    public Energy m_Energy;
    private Skill m_WeaponSkill;

    public SkillSelection_Task(BehaviorTree tree)
    {
        this.tree = tree;
    }

    public override NodeResult Execute()
    {
        if (m_Transform.GetComponent<Health>().IsDead())
        {
            return NodeResult.STOP;
        }

        if (m_WeaponSkill == null) m_WeaponSkill = m_SkillBar.GetWeaponSkill();

        if (m_SkillBar.IsSkillCompleted())
        {

            m_Target = (GameObject)tree.GetValue("ClosestTarget");
            if (m_Target == null)
            {
                tree.ResetBehabviorTree("Select_Target_Task", 0);
                tree.SetValue("m_Requested", false);
                return NodeResult.FAILURE;
            }

            //float distance = Vector3.Distance(m_Transform.position, m_Target.transform.position);
            //if (distance > (float)tree.GetValue("m_ChaseDistance") || distance > m_Weapon.GetRange())
            //{
            //    return NodeResult.FAILURE;
            //}

            Skill CurrentSkillInUse = (Skill)tree.GetValue("CurrentSkillInUse");
            if (CurrentSkillInUse != null)
            {
                if (CurrentSkillInUse.GetRange() > 0 && !IsInRangeFromSkill(CurrentSkillInUse))
                {
                    //get in range
                    children[0].Execute();
                }
                else
                {
                    //activate skill
                    children[1].Execute();
                }
            }

            if (!m_Target.GetComponent<Health>().IsDead())
            {
                Skill skillToActivate = GetRandomSkill();
                if (skillToActivate != null)
                {
                    int cost = skillToActivate.GetEnergyCost();
                    if (!m_Energy.HasEnoughEnergy(cost))
                    {
                        return NodeResult.FAILURE;
                    }

                    if (!m_SkillBar.GetEquipedSkills().Contains(skillToActivate) && skillToActivate != m_WeaponSkill)
                    {
                        return NodeResult.FAILURE;
                    }

                    PartySystem party = m_Transform.GetComponentInParent<PartySystem>();
                    if (party)
                        party.SetChildrenHasTakenDamage(true, m_Target);

                    tree.SetValue("ClosestTarget", m_Target);
                    tree.SetValue("CurrentSkillInUse", skillToActivate);
                    if (skillToActivate.GetRange() > 0 && !IsInRangeFromSkill(skillToActivate))
                    {
                        //get in range
                        children[0].Execute();
                    }
                    else
                    {
                        //activate skill
                        children[1].Execute();
                    }
                }

                return NodeResult.FAILURE;
            }
        }

        //tree.ResetBehabviorTree("WeaponAttack_Task", 1);
        tree.SetValue("m_Requested", false);
        return NodeResult.FAILURE;
    }

    public Skill GetRandomSkill()
    {
        List<Skill> readySkills = new List<Skill>();
        m_SkillStates = m_SkillBar.GetSkillStates();
        m_WeaponSkillState = m_SkillBar.GetWeaponSkillState();
        m_Skills = m_SkillBar.GetEquipedSkills();
        List<Effect> effects = m_Target.GetComponent<EffectComponent>().GetActiveEffects();
        HashSet<Skill> skillsUnderEffect = new HashSet<Skill>();

        foreach (var effect in effects)
        {
            if (effect.FromSkill != null)
            {
                skillsUnderEffect.Add(effect.FromSkill);
            }
        }

        foreach (var skill in m_Skills)
        {
            if (skill == null) continue;

            if (m_SkillStates[skill] == SkillBar_V2.SkillState.ready && !skill.IsASupportiveSkill &&
                !skillsUnderEffect.Contains(skill))
            {
                readySkills.Add(skill);
            }
        }

        if (m_WeaponSkill && m_WeaponSkillState[m_WeaponSkill] == SkillBar_V2.SkillState.ready && !m_WeaponSkill.IsASupportiveSkill)
        {
            readySkills.Add(m_WeaponSkill);
        }

        if (readySkills.Count > 0)
        {
            int index = Random.Range(0, readySkills.Count);
            Debug.Log(readySkills[index]);
            return readySkills[index];
        }

        //if (m_WeaponSkill && m_WeaponSkillState[m_WeaponSkill] == SkillBar_V2.SkillState.ready && !m_WeaponSkill.IsASupportiveSkill)
        //{
        //    return m_WeaponSkill;
        //}

        return null;
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
