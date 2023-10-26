using System;
using System.Collections;
using System.Collections.Generic;
using GW.Control;
using GW.Statistics;
using Unity.VisualScripting;
using UnityEngine;

public class UI_Skills : MonoBehaviour
{
    [SerializeField] private GameObject m_SkillUIPrefab;
    [SerializeField] private Transform m_SkillbookUI;
    [SerializeField] private Canvas m_SkillbarUI;

    public void UpdateSkillDisplay(List<Skill> availableSkills, BaseStats stats)
    {
        ProfessionsAttributes[] attributes =  stats.GetAttributes();

        foreach (Transform child in m_SkillbookUI)
        {
            Destroy(child.gameObject);
        }
         
        foreach (Skill skill in availableSkills)
        {
            if(skill == null) continue;

            if (Array.Exists(attributes, element => element == skill.m_Attribute))
            {
                GameObject skillUIObject = Instantiate(m_SkillUIPrefab, m_SkillbookUI);

                UI_SkillDisplay skillUIScript = skillUIObject.GetComponent<UI_SkillDisplay>();
                skillUIScript.SetSkillIcon(skill);

                DraggableSkillIcon draggableIcon = skillUIObject.AddComponent<DraggableSkillIcon>();
                draggableIcon.m_Prefab = skillUIObject;
                draggableIcon.m_DraggedCanvas = m_SkillbarUI;

                skillUIObject.AddComponent<UI_TooltipTrigger>();
            }
        }
    }

    public GameObject GetSkillUIPrefab() => m_SkillUIPrefab;
    public Transform GetSkillbookUI() => m_SkillbookUI;
    public Canvas GetUISkillbar() => m_SkillbarUI;
}
