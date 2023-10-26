using GW.Attributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GW.Combat
{
    [RequireComponent(typeof(Health))]
    public class CombatTarget : MonoBehaviour, IRaycastable
    {
        private Dictionary<GameObject, int> initialLayers = new Dictionary<GameObject, int>();
        private Fighter m_Player;
        private bool m_IsHovering;
        TargetSystem m_TargetSystem;

        private GameObject m_CurrentTarget;
        private bool m_IsAnEnemy;
        private bool m_IsAnAlly;

        private Outline m_Outline;
        private CursorSettings m_CursorSettings;
        private LayerMask m_LayerMask;
        private UI_Party m_UIParty;

        void Start()
        {
            initialLayers.Clear();

            foreach (var tag in GetComponent<Fighter>().m_AllyTags.Concat(GetComponent<Fighter>().m_EnemyTags))
            {
                GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);
                foreach (var go in gos)
                {
                    initialLayers[go] = go.layer;
                }
            }

            m_UIParty = FindAnyObjectByType<UI_Party>();
            m_Player = GameObject.Find("Player").GetComponent<Fighter>();
            m_CursorSettings = m_Player.GetComponent<CursorSettings>();
            m_TargetSystem = m_Player.GetComponent<TargetSystem>();
            m_LayerMask = m_Player.m_DetectionLayer;
            m_Outline = gameObject.AddComponent<Outline>();
            m_Outline.OutlineMode = Outline.Mode.OutlineVisible;
            m_Outline.OutlineWidth = 3.0f;

            if (m_TargetSystem.IsGameObjectInTagList(gameObject, m_Player.m_AllyTags))
                m_Outline.OutlineColor = Color.green;
            else if (m_TargetSystem.IsGameObjectInTagList(gameObject, m_Player.m_EnemyTags))
            {
                m_Outline.OutlineColor = Color.red;
            }

            m_Outline.enabled = false;
        }

        private void OnMouseEnter()
        {
            if (IsHoveringOverValidTarget())
            {
                m_IsHovering = true;
                m_Outline.enabled = true;
            }
            else
            {
                m_CursorSettings.ResetCursor();
                //ResetTargetLayer();
            }
            // Call your code here that needs to run when hovering starts
        }

        private void OnMouseExit()
        {

            m_Outline.enabled = false;

            m_IsHovering = false;

            if (!IsHoveringOverValidTarget())
            {
                m_CursorSettings.ResetCursor();
                
                //ResetTargetLayer();
            }
            // Call your code here that needs to run when hovering ends
        }

        private void Update()
        {
            if (m_IsHovering)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (m_IsAnEnemy)
                    {
                        m_TargetSystem.SetCombatTarget(gameObject);
                        m_Player.SetTarget(gameObject);
                        m_Player.SetAllyTarget(null);
                        m_CurrentTarget = gameObject;
                        m_Player.Attack();
                    }
                    else if (m_IsAnAlly)
                    {
                        m_TargetSystem.SetAllyTarget(gameObject);
                        m_Player.SetAllyTarget(gameObject);
                        m_Player.SetTarget(null);
                        m_CurrentTarget = gameObject;

                        if (!m_Player.IsInCombat())
                        {
                            List<GameObject> partyMembers = m_Player.GetComponentInParent<PartySystem>().GetPartyMembers();

                            if (!partyMembers.Contains(gameObject)) m_UIParty.ShowAddButton();
                            else if (partyMembers.Contains(gameObject)) m_UIParty.ShowRemoveButton();
                        }
                        else
                        {
                            m_UIParty.HideAddButton();
                            m_UIParty.HideRemoveButton();
                        }
                    }
                }
            }
        }

        private bool IsHoveringOverValidTarget()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay(), 20, m_LayerMask);
            foreach (RaycastHit hit in hits)
            {
                IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();

                foreach (var raycastable in raycastables)
                {
                    if (raycastable.HandleRaycast())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
        //    if (null == obj)
        //    {
        //        return;
        //    }

        //    obj.layer = newLayer;

        //    foreach (Transform child in obj.transform)
        //    {
        //        if (null == child)
        //        {
        //            continue;
        //        }
        //        SetLayerRecursively(child.gameObject, newLayer);
        //    }
        }

        private void ResetTargetLayer()
        {
            //foreach (var kvp in initialLayers)
            //{
            //    GameObject npc = kvp.Key;
            //    int initialLayer = kvp.Value;

            //    if (npc.layer == 8)
            //    {
            //        SetLayerRecursively(npc, initialLayer);
            //    }
            //}
        }

        public bool HandleRaycast()
        {
           if (gameObject.GetComponent<Health>().IsDead())
           {
                return false;
           }

           m_IsAnEnemy = m_TargetSystem.IsGameObjectInTagList(gameObject, m_Player.m_EnemyTags);
           m_IsAnAlly = m_TargetSystem.IsGameObjectInTagList(gameObject, m_Player.m_AllyTags);

           //ResetTargetLayer();
           if (m_IsAnEnemy)
           {
               m_CursorSettings.SetCursor(CursorSettings.CursorType.Enemy);
               return true;

           }
           else if (m_IsAnAlly)
           {
               m_CursorSettings.SetCursor(CursorSettings.CursorType.Ally);
               return true;
           }
            
           return false;
        }
    }
}
