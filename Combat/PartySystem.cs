using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using UnityEngine;
using UnityEngine.AI;

public class PartySystem : MonoBehaviour
{
    private List<GameObject> m_PartyPrefab = new List<GameObject>();
    private GameObject m_Instigator;
    private List<GameObject> m_Children = new List<GameObject>();
    private GameObject m_Leader;
    private NavMeshAgent m_LeaderNavMesh;
    public bool m_RespawnParty;
    private Vector3 m_CurrentLeaderDestination = Vector3.zero;
    bool m_LeaderHasMoved = false;
    [SerializeField] private List<Transform> m_PartyMembersPositions;
    [SerializeField] UI_Party m_PartyUI;
    void Start()
    {
        transform.GetChild(0).transform.position = transform.GetChild(1).transform.position;
        transform.GetChild(0).transform.parent = transform.GetChild(1);

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
            {
                m_Children.Add(transform.GetChild(i).gameObject);
                m_PartyPrefab.Add(transform.GetChild(i).gameObject);
                transform.GetChild(i).transform.position = m_PartyMembersPositions[i].position;
            }
        }

        if (m_Children.Count > 0)
        {
            m_Leader = m_Children[0];
            m_LeaderNavMesh = m_Leader.GetComponent<NavMeshAgent>();
        }

        if (m_PartyUI && m_Leader.CompareTag("Player"))
        {
            foreach (var partyMember in m_Children)
            {
                if (partyMember == m_Leader) continue;
                m_PartyUI.AddMemberToUI(partyMember);
            }
        }

        if(m_PartyUI) m_PartyUI.HideRemoveButton();


        float cooldownUpdateInterval = 5.0f; // Update every 0.1 seconds (adjust as needed)
        InvokeRepeating("PartyRespawn", cooldownUpdateInterval, cooldownUpdateInterval);
    }

    public void AddMemberToParty()
    {
        GameObject memberToAdd = m_Leader.GetComponent<Fighter>().m_AllyTarget.gameObject;

        if (m_Children.Contains(memberToAdd)) return;

        if(memberToAdd.GetComponentInParent<PartySystem>())
            memberToAdd.GetComponentInParent<PartySystem>().RemoveMemberFromParty(memberToAdd);
        memberToAdd.transform.parent = transform;
        m_Children.Add(memberToAdd);
        m_PartyUI.AddMemberToUI(memberToAdd);
    }

    public void RemoveMemberFromParty(GameObject memberToRemove)
    {
        m_Children.Remove(memberToRemove);
        memberToRemove.transform.parent = null;
        
        if (transform.childCount <= 1)
        {
            Destroy(gameObject);
        }
    }

    public void RemoveMemberFromParty()
    {
        GameObject memberToRemove = GameObject.FindWithTag("Player").GetComponent<Fighter>().m_AllyTarget.gameObject;
        m_PartyUI.RemoveMemberFromUI(memberToRemove, GetPartMemberIndexPosition(memberToRemove));
        m_Children.Remove(memberToRemove);
        memberToRemove.transform.parent = null;
    }

    private void PartyRespawn()
    {
        if (m_RespawnParty)
        {
            //Debug.Log("PartyRespawn");
            foreach (var child in m_Children)
            {
                Health health = child.GetComponent<Health>();
                if (health.IsDead())
                {
                    health.ResetGameObject();
                }
            }

            m_RespawnParty = false;
        }
    }

    public void SetChildrenHasTakenDamage(bool hasTakenDamage, GameObject instigator)
    {
        foreach (GameObject child in m_Children)
        {
            if (child == null) continue;
            Health health = child.GetComponent<Health>();
            //Fighter health = child.GetComponent<Health>();
            health.SetTookDamage(hasTakenDamage);
            health.SetInstigator(instigator);
        }
    }

    public List<GameObject> GetPartyMembers() => m_Children;

    public int GetPartMemberIndexPosition(GameObject go)
    {
        return m_Children.IndexOf(go);
    }

    public GameObject GetLeader()
    {
        return m_Leader;
    }

    public Vector3 GetGameObjectPartyPosition(Transform child)
    {
        for (int i = 1; i < transform.childCount; i++)
        {
            if (transform.GetChild(i) == child)
            {

                return m_PartyMembersPositions[i - 1].position;
            }
        }
        return Vector3.zero;
    }
    public Vector3 SetLeaderDestination(Vector3 destination) => m_CurrentLeaderDestination = destination;
    public void SetLeaderHasMoved(bool b) => m_LeaderHasMoved = b;
    public bool HasLeaderMoved() => m_LeaderHasMoved;

    public int GetNumberOfPartyMembers() => m_Children.Count;
}
