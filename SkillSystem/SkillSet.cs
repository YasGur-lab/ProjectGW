using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill Set", menuName = "Skill System/Skill Set")]
public class SkillSet : ScriptableObject
{
    public List<Skill> skills = new List<Skill>();

    public SkillSet(SkillSet mSkillSet)
    {
        skills = mSkillSet.skills;
    }

    public List<Skill> GetSkills => skills;
}
