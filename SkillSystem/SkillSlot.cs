using System.Collections;
using System.Collections.Generic;
using GW.Combat;
using GW.Control;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class SkillSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private SkillBar_Player m_SkillBar;
    [SerializeField] private UI_SkillBar m_SkillBarUI;

    public void OnDrop(PointerEventData eventData)
    {
        Fighter m_Fighter = m_SkillBar.gameObject.GetComponent<Fighter>();
        if (m_Fighter.IsInCombat() || m_Fighter.IsCasting()) return;

        GameObject dropped = eventData.pointerDrag;

        DraggableSkillIcon draggableSkill = dropped.GetComponent<DraggableSkillIcon>();
        if (!draggableSkill) return;

        Skill skill = dropped.GetComponent<UI_SkillDisplay>().GetSkillFromIcon();
        if (skill == null) return;

        if (m_SkillBar.DoesContains(skill))
        {
            m_SkillBarUI.RemoveSkillFromSkillBar(skill);
        }

        if (transform.childCount == 0)
        {
            draggableSkill.m_SkillSlotTranform = transform;
            m_SkillBarUI.AddSkillToSkillBar(skill, transform.GetSiblingIndex());
        }
        else
        {
            Skill existingSkill = transform.GetChild(0).GetComponent<UI_SkillDisplay>().GetSkillFromIcon();
            m_SkillBarUI.RemoveSkillFromSkillBar(existingSkill);

            if (draggableSkill.IsDraggedSkillIconFromSkillBook())
            {
                Skill existingDraggedSkill = draggableSkill.GetComponent<UI_SkillDisplay>().GetSkillFromIcon();
                draggableSkill.m_SkillSlotTranform = transform;
                m_SkillBarUI.CreateIconAndAddSkillToSkillBar(existingDraggedSkill, transform.GetSiblingIndex());
            }
            else
            {
                Skill existingDraggedSkill = draggableSkill.GetComponent<UI_SkillDisplay>().GetSkillFromIcon();
                draggableSkill.m_SkillSlotTranform = transform;
                m_SkillBarUI.CreateIconAndAddSkillToSkillBar(existingSkill, draggableSkill.m_OldSlotTransform.GetSiblingIndex());

                draggableSkill.m_OldSlotTransform = transform;
                m_SkillBarUI.AddSkillToSkillBar(existingDraggedSkill, transform.GetSiblingIndex());
            }
        }
    }
}

