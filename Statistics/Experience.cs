using System;
using System.Collections;
using System.Collections.Generic;
using GW.Saving;
using UnityEngine;

namespace GW.Statistics
{
    public class Experience : MonoBehaviour, ISaveable
    {
        [SerializeField] private float m_ExperiencePoints = 0.0f;
        [SerializeField] private float m_MaxExperiencePoints = 10.0f;

        private int m_NumberOfAlliesInGroup = 1;

        public event Action m_OnExperienceGained;

        PartySystem m_PartySystem;
        
        ExperienceDisplay m_Display;

        private void Start()
        {
            if (m_PartySystem == null) m_PartySystem = GetComponentInParent<PartySystem>();
            if (m_Display == null) m_Display = FindAnyObjectByType<ExperienceDisplay>();

            m_MaxExperiencePoints = GetComponent<BaseStats>().GetCurrentMaxExperience();

            m_Display.UpdateData(m_ExperiencePoints, m_MaxExperiencePoints);
        }

        public void GainExperience(GameObject instigator, int characterLevel)
        {
            int levelDifference = instigator.GetComponent<BaseStats>().GetStartingLevel() - characterLevel;

            levelDifference += 7;

            m_ExperiencePoints += GetComponent<BaseStats>().GetCommonStatByLevel(Stat.ExperienceReward, levelDifference) / m_PartySystem.GetNumberOfPartyMembers();
            GetComponent<BaseStats>().UpdateLevel();

            if (m_OnExperienceGained != null) 
                m_OnExperienceGained();

            m_Display.UpdateData(m_ExperiencePoints, m_MaxExperiencePoints);
        }

        public void AddExperience(float experienceToAdd)
        {
            m_ExperiencePoints += experienceToAdd;
            GetComponent<BaseStats>().UpdateLevel();

            if (m_OnExperienceGained != null) 
                m_OnExperienceGained();
        }

        public object CaptureState()
        {
            return m_ExperiencePoints;
        }

        public void RestoreState(object state)
        {
            m_ExperiencePoints = (float)state;
            GetComponent<BaseStats>().UpdateLevel();
        }

        public float GetExperience()
        {
            return m_ExperiencePoints;
        }

        public float GetOriginalExperience()
        {
            return m_MaxExperiencePoints;
        }
    }
}
