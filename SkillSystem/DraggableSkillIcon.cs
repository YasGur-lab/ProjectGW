using System.Collections;
using System.Collections.Generic;
using GW.Combat;
using GW.Control;
using GW.Core;
using StarterAssets;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableSkillIcon : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IInitializePotentialDragHandler
{
    [SerializeField] public GameObject m_Prefab; 
    private RectTransform m_RectTransform;
    private Canvas m_Canvas;
    private GameObject m_DraggedSkillIcon;
    private CanvasGroup m_DraggedCanvasGroup;
    private ThirdPersonController m_ThirdPersonController;
    public Canvas m_DraggedCanvas;
    public Transform m_SkillSlotTranform;
    public Transform m_OldSlotTransform;
    public bool m_Copy;
    public bool m_FromSkillBook;
    private Fighter m_Fighter;

    private void Awake()
    {
        m_RectTransform = GetComponent<RectTransform>();
        m_Canvas = GetComponentInParent<Canvas>();
        m_ThirdPersonController = FindAnyObjectByType<ThirdPersonController>();
        m_Fighter = m_ThirdPersonController.GetComponent<Fighter>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (m_Fighter.IsInCombat() || m_Fighter.IsCasting()) return;

        if (!m_Copy)
        {
            m_DraggedSkillIcon = Instantiate(m_Prefab, m_Canvas.transform);
            m_DraggedSkillIcon.GetComponent<DraggableSkillIcon>().m_Copy = true;
            m_DraggedSkillIcon.GetComponent<DraggableSkillIcon>().m_FromSkillBook = true;
            Image skillIconImage = m_DraggedSkillIcon.GetComponent<Image>();
            skillIconImage.sprite = GetComponent<Image>().sprite;
            skillIconImage.rectTransform.sizeDelta = m_RectTransform.sizeDelta;

            m_DraggedCanvasGroup = m_DraggedSkillIcon.GetComponent<CanvasGroup>();
            m_DraggedCanvasGroup.blocksRaycasts = false;

            gameObject.GetComponent<CanvasGroup>().alpha = 0.3f;

            m_DraggedSkillIcon.transform.SetParent(m_DraggedCanvas.transform);
            m_DraggedSkillIcon.transform.SetAsLastSibling();
        }
        else
        {

            Image skillIconImage = GetComponent<Image>();
            skillIconImage.rectTransform.sizeDelta = m_RectTransform.sizeDelta;
            m_FromSkillBook = false;
            m_DraggedCanvasGroup = GetComponent<CanvasGroup>();
            m_DraggedCanvasGroup.blocksRaycasts = false;

            gameObject.GetComponent<CanvasGroup>().alpha = 0.3f;
            if(m_SkillSlotTranform == null) m_SkillSlotTranform = transform.parent;
            m_OldSlotTransform = transform.parent;
            transform.SetParent(m_DraggedCanvas.transform);
            transform.SetAsLastSibling();
        }

        m_ThirdPersonController.FreezeCamera(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (m_Fighter.IsInCombat() || m_Fighter.IsCasting()) return;
        //Debug.Log("Draggable skill On Drop");
        Vector2 mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_DraggedCanvas.transform as RectTransform,
            eventData.position, m_DraggedCanvas.worldCamera, out mousePosition);
        if (!m_Copy)
        {
            m_DraggedSkillIcon.transform.position = m_DraggedCanvas.transform.TransformPoint(mousePosition);
        }
        else
        {
            transform.position = m_DraggedCanvas.transform.TransformPoint(mousePosition);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (m_Fighter.IsInCombat() || m_Fighter.IsCasting()) return;
        GetComponent<UI_TooltipTrigger>().CancelTooltip();
        if (!m_Copy)
        {
            m_DraggedSkillIcon.GetComponent<CanvasGroup>().blocksRaycasts = true;
            if (eventData.pointerEnter != null)
            {
                SkillSlot skillSlot = eventData.pointerEnter.GetComponentInParent<SkillSlot>();
                if (skillSlot != null) m_DraggedSkillIcon.transform.SetParent(skillSlot.transform.childCount == 0 ? skillSlot.transform : m_OldSlotTransform);
                else Destroy(m_DraggedSkillIcon);
            }
            else Destroy(m_DraggedSkillIcon);
        }
        else
        {
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            if (eventData.pointerEnter != null)
            {
                SkillSlot skillSlot = eventData.pointerEnter.GetComponentInParent<SkillSlot>();
                if (skillSlot != null) transform.SetParent(skillSlot.transform.childCount == 0 ? skillSlot.transform : m_OldSlotTransform);
                else Destroy(gameObject);
            }
            else Destroy(gameObject);
        }

        m_ThirdPersonController.FreezeCamera(false);
        gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;

        FindAnyObjectByType<UI_SkillBar>().Coroutine_UpdateSkillBarSkills();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Implement any actions you want when the skill icon is clicked
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    public bool IsDraggedSkillIconCopy() => m_DraggedSkillIcon.GetComponent<DraggableSkillIcon>().m_Copy;
    public bool IsDraggedSkillIconFromSkillBook()
    {
        if (m_DraggedSkillIcon)
        {
            return m_DraggedSkillIcon.GetComponent<DraggableSkillIcon>().m_FromSkillBook;
        }
        else
        {
            return m_FromSkillBook;
        }

    }
    public bool IsACopy() => m_Copy;
}
