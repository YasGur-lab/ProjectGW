using System;
using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetHealthDisplay : MonoBehaviour
{
    private TargetSystem m_TargetSystem;
    [SerializeField] private Slider m_Slider;
    [SerializeField] private TextMeshProUGUI m_Label;

    [SerializeField] private Health m_Target;

    private float m_CurrentHealth;
    // Start is called before the first frame update
    void Awake()
    {
        m_TargetSystem = GameObject.FindWithTag("Player").GetComponent<TargetSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_TargetSystem.GetTarget() == null) return;
        if (m_TargetSystem.GetTarget().GetComponent<Health>() == null) return;
        //if (GetComponent<Text>().text == m_TargetSystem.GetTarget().name) return;

        if (m_Target != m_TargetSystem.GetTarget().GetComponent<Health>() || m_Target == null)
            m_Target = m_TargetSystem.GetTarget().GetComponent<Health>();

        UpdateHealthBar(m_Target.GetHealth(), m_Target.GetOriginalHealth());
    }

    public void UpdateHealthBar(float currentHealth, float originalHealth)
    {
        m_Slider.value = currentHealth / originalHealth;
        int currentHealthToInt = (int)currentHealth;
        m_Label.text = currentHealthToInt.ToString();
    }
}
