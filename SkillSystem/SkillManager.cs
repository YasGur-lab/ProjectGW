using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SkillManager : ScriptableObject
{
    [SerializeField] private List<Skill> m_AvailableSkills;

    public List<Skill> GetAllAvailableSkills()
    {
        return m_AvailableSkills;
    }
}
