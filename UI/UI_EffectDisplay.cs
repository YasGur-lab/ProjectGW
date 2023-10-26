using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_EffectDisplay : MonoBehaviour
{
    private Image skillIconImage;
    [SerializeField] private Effect m_Effect;
    [SerializeField] private Skill m_Skill;
    [SerializeField] private Slider m_Slider;

    void Start()
    {
        skillIconImage = GetComponent<Image>();
        m_Slider = GetComponentInChildren<Slider>();
    }

    public void UpdateDuration(float currentDuration, float OriginalDuration)
    {
        m_Slider.value = currentDuration / OriginalDuration;
        //Debug.Log("UpdateDuration" + m_Slider.value + ": " + currentDuration + "/" + OriginalDuration);
    }

    // Public method to set the skill icon for this UI
    public void SetEffectIcon(Skill skill, Effect effect)
    {
        m_Skill = skill;
        m_Effect = effect;
        GetComponent<Image>().sprite = skill.m_Icon;
    }

    public Skill GetSkillFromIcon() => m_Skill;
    public Effect GetEffectFromIcon() => m_Effect;
}
