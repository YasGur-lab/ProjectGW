using System.Collections.Generic;
using UnityEngine;


namespace GW.Statistics
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Statistics/New Progression", order = 0)]
    public class Progression : ScriptableObject
    {
        [SerializeField] private ProgressionStat[] m_CommonStats;

        [SerializeField] private ProgressionCharacterClass[] m_ProfessionsStats;

        Dictionary<Professions, Dictionary<Stat, float[]>> m_LookUpSpecificStatTable = null;

        Dictionary<Stat, float[]> m_LookUpCommonStatTable = null;

        private Dictionary<Professions, Dictionary<ProfessionsAttributes, Dictionary<SkillList, Dictionary<Stat, float[]>>>> m_SkillsDictionnary = null;

        private Dictionary<WeaponTypes, Dictionary<ProfessionsAttributes, Dictionary<WeaponDamageThresholds, float>>> m_WeaponsDictionnary = null;

        //////////////////////
        //                  //
        //      BASICS      //
        //                  //
        //////////////////////

        public float GetCommonStat(Stat stat, int level)
        {
            BuildLookUpCommonStat();
            float[] levels = m_LookUpCommonStatTable[stat];
            if (levels.Length < level) return levels[levels.Length - 1];
            if(level - 1 < 0) return levels[level];
            return levels[level - 1];
        }

        public float GetSpecificStat(Professions profession, Stat stat, int level)
        {
            BuildLookUpSpecificStat();

            float[] levels = m_LookUpSpecificStatTable[profession][stat];
            if (levels.Length < level) return levels[levels.Length - 1];
            else if (level == 0) return levels[level];
            return levels[level - 1];
        }

        public int GetCommonLevels(Stat stat)
        {
            BuildLookUpCommonStat();
            float[] levels = m_LookUpCommonStatTable[stat];
            return levels.Length;
        }

        //////////////////////
        //                  //
        //      SKILLS      //
        //                  //
        //////////////////////
        public SkillClassData GetSkill(Professions characterClass, SkillList skill)
        {
            BuildSkillsDictionnary();

            if (!m_SkillsDictionnary.ContainsKey(characterClass))
            {
                Debug.LogError("Character class " + characterClass + " not found in skills dictionary.");
                return null;
            }

            foreach (ProgressionCharacterClass progressionClass in m_ProfessionsStats)
            {
                if (progressionClass.m_Professions == characterClass)
                {
                    foreach (var attribute in progressionClass.m_ClassTraits)
                    {
                        foreach (var potentialSkill in attribute.m_Skills)
                        {
                            if (potentialSkill.m_Skill == skill)
                            {
                                return potentialSkill;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public int GetBaseSkillStat(Professions profession, Professions secondaryProfession, ProfessionsAttributes attribute, SkillList skill, Stat stat)
        {
             BuildSkillsDictionnary();

            if (!m_SkillsDictionnary.ContainsKey(profession))
            {
                Debug.LogError("Character class " + profession + " not found in skills dictionary.");
                return 0;
            }

            foreach (ProgressionCharacterClass progressionClass in m_ProfessionsStats)
            {
                if (progressionClass.m_Professions == profession || progressionClass.m_Professions == secondaryProfession)
                {
                    foreach (var potentialAttribute in progressionClass.m_ClassTraits)
                    {
                        //Debug.Log("Potential attribute : " + potentialAttribute.m_Attributes);
                        foreach (var potentialSkill in potentialAttribute.m_Skills)
                        {
                            //Debug.Log("Looking for skill: " + skill);
                            //Debug.Log("Potential skill : " + potentialSkill.m_Skill);
                            if (potentialSkill.m_Skill == skill)
                            {
                                foreach (var potentialSkillStat in potentialSkill.m_Stats)
                                {
                                    if (potentialSkillStat.stat == stat)
                                    {
                                        int value = (int)potentialSkillStat.m_Levels[
                                            GetAttributeLevel(profession, secondaryProfession, attribute)];
                                        //Debug.Log("Damage for: " + potentialSkill.m_Skill + " :" + value);
                                        return value;
                                    }
                                }
                                //if (potentialSkill.statLevels.ContainsKey(stat))
                                //{
                                //    int attributeLevel = GetAttributeLevel(profession, secondaryProfession, attribute);
                                //    LevelData[] levelDataArray = potentialSkill.statLevels[stat];
                                //    if (attributeLevel >= 0 && attributeLevel < levelDataArray.Length)
                                //    {
                                //        int value = (int)levelDataArray[attributeLevel].value;
                                //        return value;
                                //    }
                                //}
                            }
                        }
                    }
                }
            }
            return 0;
        }

        //////////////////////////
        //                      //
        //      ATTRIBUTES      //
        //                      //
        //////////////////////////

        public void SetAttributeLevels(ProfessionsAttributes attribute, float amount)
        {
            BuildSkillsDictionnary();

            foreach (ProgressionCharacterClass progressionClass in m_ProfessionsStats)
            {
                foreach (ProfessionSpecificStat professionSpecStat in progressionClass.m_ClassTraits)
                {
                    if (professionSpecStat.m_Attributes != attribute) continue;

                    professionSpecStat.m_Level = (int)amount;
                    return;
                }
            }
        }

        public int GetAttributeLevel(Professions profession, Professions secondaryProfession, ProfessionsAttributes attribute)
        {
            BuildSkillsDictionnary();

            foreach (ProgressionCharacterClass progressionClass in m_ProfessionsStats)
            {
                if (progressionClass.m_Professions == profession || progressionClass.m_Professions == secondaryProfession)
                {
                    foreach (ProfessionSpecificStat professionSpecStat in progressionClass.m_ClassTraits)
                    {
                        if (professionSpecStat.m_Attributes == attribute)
                        {
                            return professionSpecStat.m_Level;
                        }
                    }
                }
            }

            Debug.LogWarning("Character class " + profession + " not found in character class list.");
            return 0;
        }

        public int GetAttributeLevel(ProfessionsAttributes attribute)
        {
            BuildSkillsDictionnary();

            foreach (ProgressionCharacterClass progressionClass in m_ProfessionsStats)
            {
                foreach (ProfessionSpecificStat professionSpecStat in progressionClass.m_ClassTraits)
                {
                    if (professionSpecStat.m_Attributes == attribute)
                    {
                        return professionSpecStat.m_Level;
                    }
                }
            }
            return 0;
        }


        public ProfessionsAttributes[] GetAttributes(Professions profession, Professions secondaryProfession)
        {
            BuildSkillsDictionnary();

            List<ProfessionsAttributes> attributesList = new List<ProfessionsAttributes>();

            if (!m_SkillsDictionnary.ContainsKey(secondaryProfession))
            {
                Debug.LogError("Profession " + profession + " not found in skills dictionary.");
                return null;
            }

            foreach (ProgressionCharacterClass progressionClass in m_ProfessionsStats)
            {
                if (progressionClass.m_Professions == profession)
                {
                    foreach (ProfessionSpecificStat professionStats in progressionClass.m_ClassTraits)
                    {
                        attributesList.Add(professionStats.m_Attributes);
                    }
                }
            }

            if (!m_SkillsDictionnary.ContainsKey(secondaryProfession))
            {
                Debug.LogError("Secondary Profession " + secondaryProfession + " not found in skills dictionary.");
                return null;
            }

            foreach (ProgressionCharacterClass progressionClass in m_ProfessionsStats)
            {
                if (progressionClass.m_Professions == secondaryProfession)
                {
                    foreach (ProfessionSpecificStat professionStats in progressionClass.m_ClassTraits)
                    {
                        attributesList.Add(professionStats.m_Attributes);
                    }
                }
            }

            return attributesList.ToArray();
        }
        public ProfessionsAttributes[] GetAllAttributes()
        {
            BuildSkillsDictionnary();

            List<ProfessionsAttributes> attributesList = new List<ProfessionsAttributes>();


            foreach (ProgressionCharacterClass progressionClass in m_ProfessionsStats)
            {
                foreach (ProfessionSpecificStat professionStats in progressionClass.m_ClassTraits)
                {
                    attributesList.Add(professionStats.m_Attributes);
                }
            }

            return attributesList.ToArray();
        }

        //////////////////////
        //                  //
        //      TABLES      //
        //                  //
        //////////////////////

        private void BuildLookUpCommonStat()
        {
            if (m_LookUpCommonStatTable != null) return;

            m_LookUpCommonStatTable = new Dictionary<Stat, float[]>();
            var statLookUpTable = new Dictionary<Stat, float[]>();
            foreach (ProgressionStat progressionStat in m_CommonStats)
            {
                statLookUpTable[progressionStat.stat] = progressionStat.m_Levels;
            }
            m_LookUpCommonStatTable = statLookUpTable;
        }
        
        private void BuildLookUpSpecificStat()
        {
            if (m_LookUpSpecificStatTable != null) return;

            m_LookUpSpecificStatTable = new Dictionary<Professions, Dictionary<Stat, float[]>>();
            foreach (ProgressionCharacterClass progressionClass in m_ProfessionsStats)
            {
                var statLookUpTable = new Dictionary<Stat, float[]>();

                foreach (ProgressionStat progressionStat in progressionClass.m_SpecificStats)
                {
                    statLookUpTable[progressionStat.stat] = progressionStat.m_Levels;
                }

                m_LookUpSpecificStatTable[progressionClass.m_Professions] = statLookUpTable;
            }
        }

        public void BuildSkillsDictionnary()
        {
            if (m_SkillsDictionnary != null) return;

            m_SkillsDictionnary = new Dictionary<Professions, Dictionary<ProfessionsAttributes, Dictionary<SkillList, Dictionary<Stat, float[]>>>>();
            foreach (ProgressionCharacterClass progressionClass in m_ProfessionsStats)
            {
                var attributesDictionary = new Dictionary<ProfessionsAttributes, Dictionary<SkillList, Dictionary<Stat, float[]>>>();

                foreach (ProfessionSpecificStat classSpecStat in progressionClass.m_ClassTraits)
                {
                    var skillDictionary = new Dictionary<SkillList, Dictionary<Stat, float[]>>();

                    foreach (SkillClassData skillData in classSpecStat.m_Skills)
                    {
                        if (!skillDictionary.ContainsKey(skillData.m_Skill))
                        {
                            skillDictionary[skillData.m_Skill] = new Dictionary<Stat, float[]>();
                        }

                        foreach (StatData statData in skillData.m_Stats)
                        {
                            skillDictionary[skillData.m_Skill][statData.stat] = statData.m_Levels;
                        }
                        //skillDictionary[skillData.m_Skill][skillData.stat] = skillData.m_Levels;
                    }

                    attributesDictionary[classSpecStat.m_Attributes] = skillDictionary;
                }

                m_SkillsDictionnary[progressionClass.m_Professions] = attributesDictionary;
            }
        }

        public void SetSkillsDictionary(Dictionary<Professions, Dictionary<ProfessionsAttributes, Dictionary<SkillList, Dictionary<Stat, float[]>>>> skillsDictionary)
        {
            m_SkillsDictionnary = skillsDictionary;
        }

        public Dictionary<Professions,
            Dictionary<ProfessionsAttributes, Dictionary<SkillList, Dictionary<Stat, float[]>>>> GetSkiDictionary()
        {
            return m_SkillsDictionnary;
        }

        /////////////////////
        //                 //
        //      SAVES      //
        //                 //
        /////////////////////

        [System.Serializable]
        class ProgressionCharacterClass
        {
            public Professions m_Professions;
            public ProgressionStat[] m_SpecificStats;
            public ProfessionSpecificStat[] m_ClassTraits;
        }

        [System.Serializable]
        public class ProgressionStat
        {
            public Stat stat;
            public float[] m_Levels;
        }

        //[System.Serializable]
        //public class WeaponsStats
        //{
        //    public WeaponTypes m_Type;
        //    public WeaponTypeStats m_Stats;
        //}

        //[System.Serializable]
        //public class WeaponTypeStats
        //{
        //    public ProfessionsAttributes m_Attribute;
        //    public WeaponDamageTypes m_DamageType;
        //    public WeaponDamageStats[] m_DamageStats;
        //}

        //[System.Serializable]
        //public class WeaponDamageStats
        //{
        //    public WeaponDamageThresholds m_WeaponDamageThresholds;
        //    public float m_Values;
        //}

        [System.Serializable]
        public class ProfessionSpecificStat
        {
            public ProfessionsAttributes m_Attributes;
            public int m_Level;
            public SkillClassData[] m_Skills;
        }

        //[System.Serializable]
        //public class SkillClassData
        //{
        //    public SkillList m_Skill;
        //    public Stat stat;
        //    public float[] m_Levels;
        //}

        [System.Serializable]
        public class SkillClassData
        {
            public SkillList m_Skill;
            public StatData[] m_Stats;
        }

        [System.Serializable]
        public class StatData
        {
            public Stat stat;
            public float[] m_Levels;
        }
    }

}