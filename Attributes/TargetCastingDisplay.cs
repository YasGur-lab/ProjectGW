using System;
using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using GW.Control;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetCastingDisplay : MonoBehaviour
{
    [SerializeField] private Slider m_Slider;
    [SerializeField] private TextMeshProUGUI m_Label;
    private TargetSystem m_TargetSystem;
    [SerializeField] private SkillBar_V2 m_Target;
    private float m_CurrentCastingTime;
    private bool m_HasBeenReset;
    private float m_SkillCastTime;
    private string m_Name;
    private Skill m_Skill;
    public event SkillEventArgs.SkillActivatedEventHandler SkillActivated;
    bool m_SkillActivated;

    void Awake()
    {
        m_TargetSystem = GameObject.FindGameObjectWithTag("Player").GetComponent<TargetSystem>();


    }

    void Start()
    {
        //float cooldownUpdateInterval = 0.05f; // Update every 0.1 seconds (adjust as needed)
        //InvokeRepeating("UpdateCooldowns", cooldownUpdateInterval, cooldownUpdateInterval);
        m_TargetSystem.TargetChangedEvent += OnTargetChanged;
    }

    public void UpdateCastingBar(float currentCastingTime, float skillCastTime, string name)
    {
        m_Slider.value = currentCastingTime / skillCastTime;
        m_Label.text = name;
        m_HasBeenReset = false;
    }

    //public void Update()
    //{
    //    UpdateCooldowns();
    //}

    public void ResetCastingBar()
    {
        m_HasBeenReset = true;
        m_Slider.value = 0;
        m_Label.text = "";
        m_Name = null;
        m_SkillCastTime = -1;
        m_CurrentCastingTime = -1;
        m_SkillActivated = false;
    }

    //public void UpdateCooldowns()
    //{
    //    if (!m_Target) return;

    //    if (!m_Target.IsActivatingSkill())
    //    {
    //        if (!m_HasBeenReset) ResetCastingBar();
    //        return;
    //    }
    //    if (m_Skill == null) m_Skill = m_Target.GetCurrentSkill();
    //    if (m_CurrentCastingTime < 0) m_CurrentCastingTime = 0.0f;
    //    if (m_SkillCastTime < 0) m_SkillCastTime = m_Skill.GetCastTime();
    //    if (m_Name == null) m_Name = m_Skill.name;
    //    UpdateCastingBar(m_Target.GetCurrentCastingTime(), m_SkillCastTime, m_Name);
    //}

    //private void OnEnable()
    //{
    //    m_TargetSystem.TargetChangedEvent += OnTargetChanged;
    //}
    
    //private void OnDisable()
    //{
    //    m_TargetSystem.TargetChangedEvent -= OnTargetChanged;
    //}

    private void OnTargetChanged(object sender, TargetChangedEventArgs e)
    {
        // Update the target.
        if (e.NewTarget == null) return;
        m_Target = e.NewTarget.GetComponent<SkillBar_V2>();
        //m_Skill = m_Target.GetCurrentSkill();
        //m_Target.SkillActivated += OnSkillActivated;
        // You may want to check if m_Target is null or not to handle cases when the new target doesn't have SkillBar_V2 component.

        // Handle other updates if needed...
    }
}
