using UnityEngine;

public class UI_TooltipSystem : MonoBehaviour
{
    private static UI_TooltipSystem current;

    [SerializeField] private UI_Tooltip m_Tooltip;
    // Start is called before the first frame update
    void Awake()
    {
        current = this;
    }

    public static void Show(Skill skill)
    {
        current.m_Tooltip.OnShow(skill);
        current.m_Tooltip.gameObject.SetActive(true);
    }

    public static void Show(Skill skill, Effect effect)
    {
        current.m_Tooltip.OnShow(skill, effect);
        current.m_Tooltip.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        current.m_Tooltip.gameObject.SetActive(false);
    }

}
