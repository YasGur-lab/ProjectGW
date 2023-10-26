using GW.Saving;
using System;
using System.Collections.Generic;
using GW.Combat;
using UnityEngine;
using Random = UnityEngine.Random;
using GW.Attributes;

namespace GW.Statistics
{
    public class BaseStats : MonoBehaviour, ISaveable
    {
        [Range(1, 20)] [SerializeField] private int m_StartingLevel = 1;
        [SerializeField] public Professions m_Profession;
        [SerializeField] public Professions m_SecondaryProfession;
        [SerializeField] private Progression m_ProgressionPrefab;
        [SerializeField] Progression m_Progression = null;
        [SerializeField] private GameObject m_LevelUpEffect = null;
        [SerializeField] private int m_AttributePointsAvailable;
        [SerializeField] private bool m_ShouldUseModifiers = false;
        private int[] m_AttributesLevels = new int[4];
        ProfessionsAttributes[] m_Attributes = new ProfessionsAttributes[4];

        public event Action OnLevelUp; // event we can subscribe of off.

        [SerializeField] private UIController m_UIController;

        //private LazyValue<int> m_CurrentLevel;
        private int m_CurrentLevel;
        private Experience m_Experience;
        private int m_MaxAttributePointsPerTraitLine = 12;


        Dictionary<Professions,
            Dictionary<ProfessionsAttributes, Dictionary<SkillList, Dictionary<Stat, float[]>>>> m_SkillsDictionnary = new Dictionary<Professions,
            Dictionary<ProfessionsAttributes, Dictionary<SkillList, Dictionary<Stat, float[]>>>>();

        [SerializeField] int m_ArmorLevel;
        private void Awake()
        {
            m_Progression = Instantiate(m_ProgressionPrefab, transform);
        }

        private void Start()
        {
            if (m_CurrentLevel < 1) m_CurrentLevel = m_StartingLevel;

        }

        private void OnEnable()
        {
            if (m_Experience != null)
            {
                m_Experience.m_OnExperienceGained += UpdateLevel;
            }
        }

        private void OnDisable()
        {
            if (m_Experience != null)
            {
                m_Experience.m_OnExperienceGained -= UpdateLevel;
            }
        }

        private void Update()
        {
            if (gameObject.tag != "Player") return;

            if (Input.GetKeyDown(KeyCode.P))
            {
                Experience experience = GetComponent<Experience>();
                experience.AddExperience(1000);
            }
        }

        //////////////////////
        //                  //
        //      BASICS      //
        //                  //
        //////////////////////
        public float GetCommonStat(Stat stat)
        {
            return m_Progression.GetCommonStat(stat, GetLevel());
        }

        public float GetCommonStatByLevel(Stat stat, int level)
        {
            return m_Progression.GetCommonStat(stat, level);
        }

        public float GetSpecificStat(Stat stat)
        {
            return m_Progression.GetSpecificStat(m_Profession, stat, GetLevel());
        }

        ///////////////////////
        //                   //
        //      WEAPONS      //
        //                   //
        ///////////////////////

        public float GetWeaponStat(Weapon weapon, GameObject target)
        {
            int attributeLevel = GetAttributeLevel(weapon.m_Attribute);
            int armorLevel = target.GetComponent<BaseStats>().m_ArmorLevel;
            float exponent = Mathf.Max((5f * attributeLevel - armorLevel) / 40f, 0);

            float minDamage = weapon.m_MinimumDamage;
            minDamage *= (1 + (weapon.m_PercentageBonus / 100));
            minDamage *= Mathf.Pow(2, exponent);
            minDamage = Mathf.FloorToInt(minDamage);

            float maxDamage = weapon.m_MaximumDamage;
            maxDamage *= (1 + (weapon.m_PercentageBonus / 100));
            maxDamage *= Mathf.Pow(2, exponent);
            maxDamage = Mathf.FloorToInt(maxDamage);


            int finalDamage = Mathf.RoundToInt(Random.Range(minDamage, maxDamage));
            return finalDamage;
        }

        //////////////////////
        //                  //
        //      SKILLS      //
        //                  //
        //////////////////////
        public float GetSkillStat(SkillList skill, Stat stat, ProfessionsAttributes attribute, GameObject target)
        {
            var baseDamage = GetSkillBaseDamage(skill, attribute, stat);
            int armorLevel = target.GetComponent<BaseStats>().m_ArmorLevel;
            float exponent = Mathf.Max((3f * m_CurrentLevel - armorLevel) / 40f, 0);
            float finalDamage = baseDamage * Mathf.Pow(2, exponent);
            finalDamage = Mathf.FloorToInt(finalDamage);
            return finalDamage;
        }

        public float GetSkillBaseDamage(SkillList skill, ProfessionsAttributes attribute, Stat stat)
        {
            float baseDamage = m_Progression.GetBaseSkillStat(m_Profession, m_SecondaryProfession, attribute, skill, stat);
            return baseDamage;
        }

        public Progression.SkillClassData GetSkill(SkillList skill)
        {
            return m_Progression.GetSkill(m_Profession, skill);
        }

        //////////////////////////
        //                      //
        //      ATTRIBUTES      //
        //                      //
        //////////////////////////
        private void GetAttributePoints()
        {
            m_AttributePointsAvailable += (int)GetCommonStat(Stat.AttributePoints);
        }

        public void SetAttributeLevel(ProfessionsAttributes attribute, float amount)
        {
            m_Progression.SetAttributeLevels(attribute, amount);
        }

        public int GetAttributeLevel(ProfessionsAttributes attribute)
        {
            return m_Progression.GetAttributeLevel(attribute);
        }

        public ProfessionsAttributes[] GetAttributes()
        {
            return m_Progression.GetAttributes(m_Profession, m_SecondaryProfession);
        }
        public int GetAttributePointsAvailable() { return m_AttributePointsAvailable; }

        public void SetAttributePointsAvailable(int amount) { m_AttributePointsAvailable += amount; }

        public void ResetAttributePoints()
        {
            ProfessionsAttributes[] attributes = m_Progression.GetAllAttributes();

            foreach (var attribute in attributes)
            {
                m_AttributePointsAvailable += GetAttributeLevel(attribute);
                SetAttributeLevel(attribute, 0);
            }
        }

        public Dictionary<ProfessionsAttributes, int> GetAttributesData()
        {
            Dictionary<ProfessionsAttributes, int> data = new Dictionary<ProfessionsAttributes, int>();
            ProfessionsAttributes[] traits = GetAttributes();
            foreach (var trait in traits)
            {
                data[trait] = GetAttributeLevel(trait);
            }
            return data;
        }

        public int GetMaxAttributeValue()
        {
            return m_MaxAttributePointsPerTraitLine;
        }

        //////////////////////
        //                  //
        //      LEVELS      //
        //                  //
        //////////////////////
        public void UpdateLevel()
        {
            int newLevel = CalculateLevel();
            if (newLevel > m_CurrentLevel)
            {
                m_CurrentLevel = newLevel;
                LevelUpEffect();
                GetAttributePoints();
                GetComponent<Health>().SetHealth(GetCommonStatByLevel(Stat.Health, m_CurrentLevel));
                GetComponent<Energy>().SetEnergy(GetSpecificStat(Stat.Energy));
                //m_UIController.GetComponent<AttributesPanel>().SetAttributePointsAvailableText(GetAttributePointsAvailable().ToString());
                //OnLevelUp();
            }
        }

        private void LevelUpEffect()
        {
            if (m_LevelUpEffect != null)
                Instantiate(m_LevelUpEffect, transform);
        }

        public int GetLevel() { return m_CurrentLevel; }

        public int GetStartingLevel() { return m_StartingLevel; }
        public void SetStartingLevel(int level) { m_StartingLevel = level; }
        public int CalculateLevel()
        {
            Experience experience = GetComponent<Experience>();
            if (experience == null) return m_StartingLevel;

            float currentExp = experience.GetExperience();
            int maxLevel = m_Progression.GetCommonLevels(Stat.ExperienceToLevelUp);
            for (int level = 1; level < maxLevel; level++)
            {
                float expToLevelUp = m_Progression.GetCommonStat(Stat.ExperienceToLevelUp, level);
                if (expToLevelUp > currentExp)
                {
                    return level - 1;
                }
            }
            return maxLevel;
        }
        public int GetCurrentMaxExperience()
        {
            return (int)m_Progression.GetCommonStat(Stat.ExperienceToLevelUp, m_CurrentLevel + 1);
        }

        /////////////////////////
        //                     //
        //       MODIFIERS     //
        //                     //
        /////////////////////////
        private float GetAdditiveModifier(Stat stat)
        {
            if (!m_ShouldUseModifiers) return 0;
            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifiers in provider.GetAdditiveModifiers(stat))
                {
                    total += modifiers;
                }
            }
            return total;
        }
        private float GetPercentageModifier(Stat stat)
        {
            if (!m_ShouldUseModifiers) return 0;
            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifiers in provider.GetPercentageModifiers(stat))
                {
                    total += modifiers;
                }
            }
            return total;
        }

        ////////////////////////
        //                    //
        //       SAVING       //
        //                    //
        ////////////////////////
        public object CaptureState()
        {
            m_SkillsDictionnary = m_Progression.GetSkiDictionary();

            for (int i = 0; i < GetAttributes().Length; i++)
            {
                m_Attributes[i] = GetAttributes()[i];
                m_AttributesLevels[i] = GetAttributeLevel(GetAttributes()[i]);
            }

            return new SaveData()
            {
                m_CurrentLevel = this.m_CurrentLevel,
                m_AttributePointsAvailable = this.m_AttributePointsAvailable,
                m_Profession = this.m_Profession,
                m_SecondaryProfession = this.m_SecondaryProfession,
                m_AttributesLevels = this.m_AttributesLevels,
                m_Attributes = this.m_Attributes
            };
        }

        public void RestoreState(object state)
        {
            SaveData saveData = (SaveData)state;
            m_CurrentLevel = saveData.m_CurrentLevel;
            SetStartingLevel(m_CurrentLevel);
            m_AttributePointsAvailable = saveData.m_AttributePointsAvailable;
            m_Profession = saveData.m_Profession;
            m_SecondaryProfession = saveData.m_SecondaryProfession;
            m_AttributesLevels = saveData.m_AttributesLevels;
            m_Attributes = saveData.m_Attributes;

            for (int i = 0; i < m_AttributesLevels.Length; i++)
            {
                SetAttributeLevel(m_Attributes[i], m_AttributesLevels[i]);
            }

            //m_AttributesPanel.GetComponent<AttributesPanel>().UpdatePanel();
        }

        [Serializable]
        private struct SaveData
        {
            
            public int m_CurrentLevel;
            public int m_AttributePointsAvailable;
            public Professions m_Profession;
            public Professions m_SecondaryProfession;
            public ProfessionsAttributes[] m_Attributes;
            public int[] m_AttributesLevels;
        }
    }
}
