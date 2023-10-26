using System.Collections;
using System.Collections.Generic;
using GW.Control;
using TMPro;
using UnityEngine;
using static SkillEventArgs;

public class UI_SkillBar : MonoBehaviour
{
    public SkillCooldownUpdateEventHandler SkillsCooldownUpdated;
    public SkillCooldownUpdateEventHandler SkillsCooldownReset;

    //--SKILL BAR--
    [SerializeField] private SkillBar_Player m_SkillBar;
    [SerializeField] private GameObject m_UI_SkillIcon;
    [SerializeField] private UI_Skills m_UI_Skill;
    [SerializeField] Transform[] m_SkillSlotTransforms;
    [SerializeField] private TextMeshProUGUI[] m_SkillTexts;

    void Start()
    {
        SkillsCooldownUpdated += UpdateSkillCooldownUI;
        SkillsCooldownReset += ResetSkillCooldownUI;
    }

    public void AddSkillToSkillBar(Skill skillToAdd, int skillBarIndex)
    {
        if (skillBarIndex >= 0 && skillBarIndex <= m_SkillBar.GetEquipedSkills().Count)
        {
            m_SkillBar.InitSkill(skillToAdd, skillBarIndex);
        }
    }

    public void CreateIconAndAddSkillToSkillBar(Skill skillToAdd, int skillBarIndex)
    {
        m_SkillBar.InitSkill(skillToAdd, skillBarIndex);

        GameObject skillUIObject = Instantiate(m_UI_SkillIcon, m_SkillSlotTransforms[skillBarIndex].transform);
        skillUIObject.transform.localScale = new Vector3(0.81f, 0.81f, 0.81f);

        UI_SkillDisplay skillUIScript = skillUIObject.GetComponent<UI_SkillDisplay>();
        skillUIScript.SetSkillIcon(skillToAdd);
        skillUIScript.SetSkillBar(m_SkillBar);

        DraggableSkillIcon draggableIcon = skillUIObject.AddComponent<DraggableSkillIcon>();
        draggableIcon.m_Prefab = skillUIObject;
        draggableIcon.m_DraggedCanvas = m_UI_Skill.GetUISkillbar();
        draggableIcon.m_SkillSlotTranform = m_SkillSlotTransforms[skillBarIndex].transform;
        draggableIcon.m_Copy = true;

        skillUIObject.AddComponent<UI_TooltipTrigger>();
    }

    public void RemoveSkillFromSkillBar(Skill skill)
    {
        int skillIndex = m_SkillBar.FindSkillIndex(skill);
        if (skillIndex == -1) return;
        if (skillIndex >= 0 && skillIndex < m_SkillBar.GetEquipedSkills().Count)
        {
            Transform skillSlotTransform = m_SkillSlotTransforms[skillIndex].transform;
            for (int i = skillSlotTransform.childCount - 1; i >= 0; i--)
            {
                Destroy(skillSlotTransform.GetChild(i).gameObject);
            }
            //Debug.Log("Skill " + m_Skills[skillIndex].name + " has been removed at index " + skillIndex);
            m_SkillBar.GetEquipedSkills()[skillIndex] = null;
        }
        else
        {
            Debug.LogError("Invalid skill index: " + skillIndex);
        }
    }

    public void UpdateSkillCooldownUI(object sender, SkillEventArgs args)
    {
        Skill skill = args.skill;
        var cooldown = args.cooldown;
        var index = m_SkillBar.FindSkillIndex(skill);
        
        m_SkillTexts[index].text = $"{Mathf.Ceil(cooldown)}";
    }

    private void ResetSkillCooldownUI(object sender, SkillEventArgs args)
    {
        Skill skill = args.skill;
        var index = m_SkillBar.FindSkillIndex(skill);

        m_SkillTexts[index].text = "";
    }

    public void Coroutine_UpdateSkillBarSkills()
    {
        StartCoroutine(UpdateSkillBarSkills());
    }

    public IEnumerator UpdateSkillBarSkills()
    {
        yield return new WaitForSeconds(0.1f);

        m_SkillBar.GetEquipedSkills().Clear();

        foreach (Transform slotTransform in m_SkillSlotTransforms)
        {
            if (slotTransform.childCount > 0)
            {
                Skill skill = slotTransform.GetChild(0).gameObject.GetComponent<UI_SkillDisplay>()
                    .GetSkillFromIcon();
                m_SkillBar.GetEquipedSkills().Add(skill);
            }
            else
            {
                m_SkillBar.GetEquipedSkills().Add(null);
            }
        }
    }
}
