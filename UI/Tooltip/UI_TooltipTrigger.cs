using UnityEngine;
using UnityEngine.EventSystems;

public class UI_TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static LTDescr m_Delay;
    [Multiline()]
    private UI_SkillDisplay m_SkillDisplay;
    private UI_EffectDisplay m_EffectDisplay;

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_Delay = LeanTween.delayedCall(0.25f, SettingSkillTooltip);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CancelTooltip();
        UI_TooltipSystem.Hide();
    }

    public void CancelTooltip()
    {
        LeanTween.cancel(m_Delay.uniqueId);
    }

    void SettingSkillTooltip()
    {
        m_SkillDisplay = GetComponent<UI_SkillDisplay>();
        if (m_SkillDisplay != null)
        {
            Skill skill = m_SkillDisplay.GetSkillFromIcon();
            if (skill != null)
            {
                UI_TooltipSystem.Show(skill);
            }

            return;
        }
        m_EffectDisplay = GetComponent<UI_EffectDisplay>();
        if (m_EffectDisplay != null)
        {
            Effect effect = m_EffectDisplay.GetEffectFromIcon();
            Skill skill = m_EffectDisplay.GetSkillFromIcon();
            if (effect != null)
            {
                UI_TooltipSystem.Show(skill, effect);
            }
            return;
        }
    }
}
