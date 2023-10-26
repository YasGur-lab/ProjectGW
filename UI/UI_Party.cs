using System.Collections.Generic;
using GW.Attributes;
using GW.Movement;
using TMPro;
using UnityEngine;

public class UI_Party : MonoBehaviour
{
    [SerializeField] private GameObject m_PartyMemberDataPrefab;
    private List<GameObject> m_PartyMembersData = new List<GameObject>();
    [SerializeField] GameObject m_AddButton;
    [SerializeField] GameObject m_RemoveButton;

    public void RemoveMemberFromUI(GameObject member, int memberDataIndex)
    {
        m_PartyMembersData[memberDataIndex - 1].GetComponent<HealthDisplay>();
        member.GetComponent<Health>().RemoveHealthDisplay();
        member.GetComponent<Energy>().RemoveEnergyDisplay();
        member.GetComponent<EffectComponent>().RemoveUIEffects();
        m_PartyMembersData.RemoveAt(memberDataIndex - 1);
        Destroy(transform.GetChild(1).GetChild(memberDataIndex - 1).gameObject);
        HideRemoveButton();
    }

    public void AddMemberToUI(GameObject member)
    {
        GameObject dataTile = Instantiate(m_PartyMemberDataPrefab, transform.GetChild(1));
        m_PartyMembersData.Add(dataTile);

        Transform parentTransform = transform.GetChild(1);
        int lastIndex = parentTransform.childCount - 1;

        if (lastIndex >= 1)
        {
            Transform lastChild = parentTransform.GetChild(lastIndex);
            Transform secondLastChild = parentTransform.GetChild(lastIndex - 1);

            int tempIndex = lastChild.GetSiblingIndex();
            lastChild.SetSiblingIndex(secondLastChild.GetSiblingIndex());
            secondLastChild.SetSiblingIndex(tempIndex);
        }

        dataTile.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = member.name;
        member.GetComponent<Health>().SetHealthDisplay(dataTile.GetComponent<HealthDisplay>());
        member.GetComponent<Energy>().SetEnergyDisplay(dataTile.GetComponent<EnergyDisplay>());
        member.GetComponent<EffectComponent>().SetUIEffects(dataTile.GetComponent<UI_Effects>());
        UI_Effects uiEffects = dataTile.GetComponent<UI_Effects>();
        foreach (var effect in member.GetComponent<EffectComponent>().GetActiveEffects())
        {
            uiEffects.AddEffectToDisplay(effect, effect.FromSkill);
        }

        dataTile.SetActive(true);
        ShowRemoveButton();
    }

    public void HideAddButton()
    {
        m_AddButton.SetActive(false);
    }

    public void ShowAddButton()
    {
        HideRemoveButton();
        m_AddButton.SetActive(true);
    }

    public void ShowRemoveButton()
    {
        HideAddButton();
        m_RemoveButton.SetActive(true);
    }
    public void HideRemoveButton()
    {
        m_RemoveButton.SetActive(false);
    }

    public List<GameObject> GetPartyMemberData() => m_PartyMembersData;
}
