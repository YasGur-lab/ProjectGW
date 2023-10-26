using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Skill Bar Inputs", menuName = "Skill System/Skill Bar Inputs")]
public class SkillBarInputs : ScriptableObject
{
    [SerializeField] private List<KeyCode> m_KeyCodesSetup;
    public List<KeyCode> GetInputs => m_KeyCodesSetup;
}
