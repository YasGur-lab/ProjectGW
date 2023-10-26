using System;
using System.Collections;
using System.Collections.Generic;
using GW.Statistics;
using TMPro;
using Unity.Loading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UI_Tooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_EffectDuration;
    [SerializeField] private TextMeshProUGUI m_HeaderTextBox;
    [SerializeField] private TextMeshProUGUI m_SkillTypeTextBox;
    [SerializeField] private TextMeshProUGUI m_SkillAttributeTextBox;
    [SerializeField] private TextMeshProUGUI m_CostTextBox;
    [SerializeField] private TextMeshProUGUI m_CDTextBox;
    [SerializeField] private TextMeshProUGUI m_CastTimeTextBox;
    [SerializeField] private TextMeshProUGUI m_BodyTextBox;
    [SerializeField] private LayoutElement m_LayoutElement;
    [SerializeField] private RectTransform m_RectTransform;
    [SerializeField] private int m_CharacterWrapLimit;
    private UI_SkillDisplay m_SkillDisplay;
    [SerializeField] private BaseStats m_BaseStats;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        m_LayoutElement.enabled = Math.Max(m_HeaderTextBox.preferredWidth, m_BodyTextBox.preferredWidth) >= m_LayoutElement.preferredWidth;

        Vector2 position = Input.mousePosition;

        float x = position.x / Screen.width;
        float y = position.y / Screen.height;

        if (x <= y && x <= 1 - y) m_RectTransform.pivot = new Vector2(-0.15f, y);
        else if (x >= y && x <= 1 - y) m_RectTransform.pivot = new Vector2(x, -0.1f);
        else if (x >= y && x >= 1 - y) m_RectTransform.pivot = new Vector2(1.1f, y);
        else if (x <= y && x >= 1 - y) m_RectTransform.pivot = new Vector2(x, 1.3f);
        transform.position = position;
    }

    public void OnShow(Skill skill)
    {
        if (skill != null)
        {
            float damage = m_BaseStats.GetSkillBaseDamage(skill.m_Skill, skill.GetAttribute(), Stat.Damage);
            m_EffectDuration.text ="";
            m_HeaderTextBox.text = skill.name;
            m_SkillTypeTextBox.text = skill.GetSkillType().ToString();
            m_SkillAttributeTextBox.text = new string("Level: ") + m_BaseStats.GetAttributeLevel(skill.GetAttribute()) + " " + skill.GetAttribute();
            m_BodyTextBox.text = string.Format(skill.GetDescription(), damage);
            m_CostTextBox.text = new string("Cost: ") + skill.GetEnergyCost() + new string(" energy");
            m_CDTextBox.text = new string("Cooldown: ") + skill.GetCooldownTime() + new string("s");
            m_CastTimeTextBox.text = new string("Cast time: ") + skill.GetCastTime() + new string("s");

            m_LayoutElement.enabled = Math.Max(m_HeaderTextBox.preferredWidth, m_BodyTextBox.preferredWidth) >= m_LayoutElement.preferredWidth;
            Vector2 position = Input.mousePosition;

            float x = position.x / Screen.width;
            float y = position.y / Screen.height;

            if (x <= y && x <= 1 - y) m_RectTransform.pivot = new Vector2(-0.15f, y);
            else if (x >= y && x <= 1 - y) m_RectTransform.pivot = new Vector2(x, -0.1f);
            else if (x >= y && x >= 1 - y) m_RectTransform.pivot = new Vector2(1.1f, y);
            else if (x <= y && x >= 1 - y) m_RectTransform.pivot = new Vector2(x, 1.3f);
            transform.position = position;
        }
    }

    public void OnShow(Skill skill, Effect effect)
    {
        if (effect != null && skill != null)
        {
            float damage = m_BaseStats.GetSkillBaseDamage(skill.m_Skill, skill.GetAttribute(), Stat.Damage);
            m_EffectDuration.text = new string("Duration: ") + effect.GetRemainingDuration();
            m_HeaderTextBox.text = skill.name;
            m_SkillTypeTextBox.text = skill.GetSkillType().ToString();
            m_SkillAttributeTextBox.text = new string("Level: ") + m_BaseStats.GetAttributeLevel(skill.GetAttribute()) + " " + skill.GetAttribute();
            m_BodyTextBox.text = string.Format(skill.GetDescription(), damage);
            m_CostTextBox.text = new string("Cost: ") + skill.GetEnergyCost() + new string(" energy");
            m_CDTextBox.text = new string("Cooldown: ") + skill.GetCooldownTime() + new string("s");
            m_CastTimeTextBox.text = new string("Cast time: ") + skill.GetCastTime() + new string("s");

            m_LayoutElement.enabled = Math.Max(m_HeaderTextBox.preferredWidth, m_BodyTextBox.preferredWidth) >= m_LayoutElement.preferredWidth;
            Vector2 position = Input.mousePosition;

            float x = position.x / Screen.width;
            float y = position.y / Screen.height;

            if (x <= y && x <= 1 - y) m_RectTransform.pivot = new Vector2(-0.15f, y);
            else if (x >= y && x <= 1 - y) m_RectTransform.pivot = new Vector2(x, -0.1f);
            else if (x >= y && x >= 1 - y) m_RectTransform.pivot = new Vector2(1.1f, y);
            else if (x <= y && x >= 1 - y) m_RectTransform.pivot = new Vector2(x, 1.3f);
            transform.position = position;
        }
    }
}
