using System;
using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetEnergyDisplay : MonoBehaviour
{
    private TargetSystem m_TargetSystem;
    [SerializeField] private Slider m_Slider;
    [SerializeField] private TextMeshProUGUI m_Label;

    [SerializeField] private Energy m_Target;

    private float m_CurrentEnergy;
    // Start is called before the first frame update
    void Awake()
    {
        m_TargetSystem = GameObject.FindWithTag("Player").GetComponent<TargetSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_TargetSystem.GetTarget() == null) return;
        if (m_TargetSystem.GetTarget().GetComponent<Energy>() == null) return;

        if (m_Target != m_TargetSystem.GetTarget().GetComponent<Energy>() || m_Target == null)
            m_Target = m_TargetSystem.GetTarget().GetComponent<Energy>();

        UpdateEnergyBar(m_Target.GetEnergy(), m_Target.GetInitialEnergy());
    }

    public void UpdateEnergyBar(float currentEnergy, float originalEnergy)
    {
        m_Slider.value = currentEnergy / originalEnergy;
        int currentHealthToInt = (int)currentEnergy;
        m_Label.text = currentHealthToInt.ToString();
    }
}
