using GW.Control;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static GW.Statistics.Progression;

namespace GW.Statistics
{
    public class AttributesPanel : MonoBehaviour
    {
        [SerializeField] private Text m_ClassNameText, m_AttributePointsAvailableText;
        [SerializeField] private TMP_Dropdown m_PrimaryProfessionDropdown;
        [SerializeField] private TMP_Dropdown m_SecondaryProfessionDropdown;
        [SerializeField] private Text[] m_TraitsTexts;
        [SerializeField] private Text[] m_AttributesTexts;
        [SerializeField] private Button[] m_DecrementButtons;
        [SerializeField] private Button[] m_IncrementButtons;

        private UI_Skills m_UISkills;
        [SerializeField] SkillManager m_SkillManager;

        private BaseStats m_Stats;
        private GameObject m_Player;

        private Professions m_CurrentProfession;
        private Professions m_CurrentSecondaryProfession;
        Dictionary<ProfessionsAttributes, int> m_AttributesTable = new Dictionary<ProfessionsAttributes, int>();

        void Awake()
        {
            m_Player = GameObject.Find("Player");
            m_Stats = m_Player.GetComponent<BaseStats>();
            m_UISkills = GetComponent<UI_Skills>();
        }

        // Start is called before the first frame update
        void Start()
        {
            gameObject.SetActive(false);
        }
        
        // Update is called once per frame
        void Update()
        {
            if (m_CurrentProfession != m_Stats.m_Profession)
            {
                UpdateTraitsUI();
                SetUpButtons();
                SetUpPrimaryProfessionDropdown();
                SetUpSecondaryProfessionDropdown();
            }

            if (m_CurrentSecondaryProfession != m_Stats.m_SecondaryProfession)
            {
                UpdateTraitsUI();
                SetUpButtons();
                SetUpPrimaryProfessionDropdown();
                SetUpSecondaryProfessionDropdown();
            }
        }

        public void UpdateTraitsUI()
        {
            m_CurrentProfession = m_Stats.m_Profession;
            m_CurrentSecondaryProfession = m_Stats.m_SecondaryProfession;
            m_AttributesTable = m_Stats.GetAttributesData();

            m_AttributePointsAvailableText.text = m_Stats.GetAttributePointsAvailable().ToString();

            int i = 0;
            foreach (var attribute in m_AttributesTable)
            {
                m_TraitsTexts[i].text = attribute.Key.ToString();
                m_AttributesTexts[i].text = attribute.Value.ToString();
                i++;
            }
            m_UISkills.UpdateSkillDisplay(m_SkillManager.GetAllAvailableSkills(), m_Stats);
        }

        private void OnProfessionValueChanged(int value)
        {
            m_Stats.ResetAttributePoints();
            m_Stats.m_Profession = (Professions)value;
            SetUpSecondaryProfessionDropdown();
        }

        private void OnSecondaryProfessionValueChanged(int value)
        {
            m_Stats.ResetAttributePoints();

            if (System.Enum.TryParse(m_SecondaryProfessionDropdown.options[value].text, out Professions secondaryProfession))
            {
                m_Stats.m_SecondaryProfession = secondaryProfession;
            }
        }

        private void SetUpSecondaryProfessionDropdown()
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (Professions profession in System.Enum.GetValues(typeof(Professions)))
            {
                if (profession == m_Stats.m_Profession) continue;
                TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(profession.ToString());
                options.Add(option);
            }
            m_SecondaryProfessionDropdown.options = options;

            // Find the index of the secondary profession by name
            int index = options.FindIndex(option => option.text == m_Stats.m_SecondaryProfession.ToString());
            if (index >= 0)
            {
                m_SecondaryProfessionDropdown.value = index;
            }
            else
            {
                m_SecondaryProfessionDropdown.value = 0;
                OnSecondaryProfessionValueChanged(0);
                UpdateTraitsUI();
                SetUpButtons();
            }

            m_SecondaryProfessionDropdown.RefreshShownValue();
            m_SecondaryProfessionDropdown.onValueChanged.AddListener(OnSecondaryProfessionValueChanged);
        }

        private void SetUpPrimaryProfessionDropdown()
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (Professions characterClass in System.Enum.GetValues(typeof(Professions)))
            {
                TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(characterClass.ToString());
                options.Add(option);
            }
            m_PrimaryProfessionDropdown.options = options;
            m_PrimaryProfessionDropdown.value = (int)m_Stats.m_Profession;
            m_PrimaryProfessionDropdown.RefreshShownValue();
            m_PrimaryProfessionDropdown.onValueChanged.AddListener(OnProfessionValueChanged);
        }

        private void SetUpButtons()
        {
            int i = 0;
            foreach (var trait in m_AttributesTable)
            {
                m_DecrementButtons[i].GetComponent<TraitButtonSetup>().SetAssignedTrait(trait.Key);
                m_IncrementButtons[i].GetComponent<TraitButtonSetup>().SetAssignedTrait(trait.Key);
                i++;
            }

            foreach (var button in m_DecrementButtons)
            {
                ProfessionsAttributes attribute = button.GetComponent<TraitButtonSetup>().GetAssignedAttribute();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => DecrementAttribute(attribute));
            }

            foreach (var button in m_IncrementButtons)
            {
                ProfessionsAttributes attribute = button.GetComponent<TraitButtonSetup>().GetAssignedAttribute();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => IncrementAttribute(attribute));
            }
        }

        public void DecrementAttribute(ProfessionsAttributes attribute)
        {
            int currentValue = m_AttributesTable[attribute];
            if (currentValue > 0)
            {
                int newValue = currentValue - 1;
                m_AttributesTable[attribute] = newValue;
                m_Stats.SetAttributePointsAvailable(1);
                m_Stats.SetAttributeLevel(attribute, newValue);
                UpdateTraitsUI();
            }
        }

        public void IncrementAttribute(ProfessionsAttributes attribute)
        {
            int currentValue = m_AttributesTable[attribute];
            int availablePoints = m_Stats.GetAttributePointsAvailable();
            if (currentValue < m_Stats.GetMaxAttributeValue() && availablePoints > 0)
            {
                int newValue = currentValue + 1;
                m_AttributesTable[attribute] = newValue;
                m_Stats.SetAttributePointsAvailable(-1);
                m_Stats.SetAttributeLevel(attribute, newValue);
                UpdateTraitsUI();
            }
        }

        public void SetAttributePointsAvailableText(string text)
        {
            m_AttributePointsAvailableText.text = text;
        }

        public void UpdatePanel()
        {
            UpdateTraitsUI();
            SetUpButtons();
            SetUpPrimaryProfessionDropdown();
            SetUpSecondaryProfessionDropdown();
        }
    }
}