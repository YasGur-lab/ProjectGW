using System;
using System.Collections;
using System.Collections.Generic;
using GW.Statistics;
using UnityEngine;

public class UI_Effects : MonoBehaviour
{
    [SerializeField] private GameObject m_SkillUIPrefab;
    [SerializeField] private Transform m_EffectsBar;
    [SerializeField] private Canvas m_EffectsBarUI;

    private List<Effect> m_ActiveEffects = new List<Effect>();
    private Dictionary<Effect, GameObject> m_EffectUIObjects = new Dictionary<Effect, GameObject>();

    public void AddEffectToDisplay(Effect effect, Skill skill)
    {
        GameObject effectUIObject = Instantiate(m_SkillUIPrefab, m_EffectsBar);

        UI_EffectDisplay effectUIScript = effectUIObject.GetComponent<UI_EffectDisplay>();
        effectUIScript.SetEffectIcon(skill, effect);
        effectUIObject.transform.localScale = new Vector3(0.81f, 0.81f, 0.81f);

        effectUIObject.AddComponent<UI_TooltipTrigger>();

        m_ActiveEffects.Add(effect);
        m_EffectUIObjects.Add(effect, effectUIObject);
    }

    public void RemoveEffectToDisplay(Effect effect)
    {
        if (m_ActiveEffects.Contains(effect))
        {
            UI_TooltipSystem.Hide();
            if (m_EffectUIObjects.TryGetValue(effect, out GameObject effectUIObject))
            {
                m_EffectUIObjects.Remove(effect);
                Destroy(effectUIObject);
            }
            m_ActiveEffects.Remove(effect);
        }
    }

    public void UpdateEffectDuration(Effect effect)
    {
        if (m_ActiveEffects.Contains(effect))
        {
            if (m_EffectUIObjects.TryGetValue(effect, out GameObject effectUIObject))
            {
                UI_EffectDisplay uiEffectDisplay = effectUIObject.GetComponent<UI_EffectDisplay>();
                float remainingDuration = effect.GetRemainingDuration();
                float originalDuration = effect.OriginalDuration;
                uiEffectDisplay.UpdateDuration(remainingDuration, originalDuration);
            }
        }
    }

    public GameObject GetSkillUIPrefab() => m_SkillUIPrefab;
    public Transform GetSkillbookUI() => m_EffectsBar;
    public Canvas GetUISkillbar() => m_EffectsBarUI;
    public List<Effect> GetEffectsList() => m_ActiveEffects;
}
