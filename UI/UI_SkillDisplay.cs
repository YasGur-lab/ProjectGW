using System.Collections;
using System.Collections.Generic;
using GW.Control;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SkillDisplay : MonoBehaviour, IPointerClickHandler
{
    private Image skillIconImage;
    [SerializeField] private Skill m_Skill;
    private SkillBar_Player m_SkillBar;
    void Start()
    {
        skillIconImage = GetComponent<Image>();
    }

    // Public method to set the skill icon for this UI
    public void SetSkillIcon(Skill skill)
    {
        m_Skill = skill;
        GetComponent<Image>().sprite = skill.m_Icon;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        m_SkillBar.ActivateSkill(transform.parent.GetSiblingIndex());
    }

    public Skill GetSkillFromIcon() => m_Skill;

    public void SetSkillBar(SkillBar_Player skillBar)
    {
        m_SkillBar = skillBar;
    }
}
