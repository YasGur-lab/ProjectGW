using System.Collections;
using System.Collections.Generic;
using GW.Combat;
using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    public GameObject m_Target;
    
    //[SerializeField] private Camera m_Camera;
    [SerializeField] private GameObject m_PreviousTarget;
    private Color m_TargetOriginalColor;


    //--HIGHLIGHT--
    //[SerializeField] private Material outlineMaterial;
    //[SerializeField] private float outlineScaleFactor;
    //[SerializeField] private Color outlineColor;
    //private Renderer outlineRenderer;


    // Start is called before the first frame update
    void Start()
    {
        //outlineRenderer = CreateOutline(outlineMaterial, outlineScaleFactor, outlineColor);
        //outlineRenderer.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
            foreach (RaycastHit hit in hits)
            {
                CombatTarget target = hit.transform.GetComponent<CombatTarget>();
                if (!GetComponent<Fighter>().CanAttack(target.gameObject)) continue;

                if (m_Target != null)
                        m_PreviousTarget = m_Target;
                m_Target = hit.collider.gameObject;
            }
        }
        //EnemyDeselected();
    }

    void EnemySelected()
    {
        if (m_Target != null)
            m_Target.GetComponent<Renderer>().material.color = Color.red;
        if(m_PreviousTarget != null)
            m_PreviousTarget.GetComponent<Renderer>().material.color = m_TargetOriginalColor;
    }

    void EnemyDeselected()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
            if (hits.Length == 1)
            {
                if (hits[0].collider.tag == "Terrain")
                {
                    m_Target = null;
                    m_PreviousTarget = null;
                }
            }
        }
    }

    //Renderer CreateOutline(Material outlineMat, float scaleFactor, Color color)
    //{
    //    Renderer rend = m_Target.GetComponent<Renderer>();

    //    rend.material = outlineMat;
    //    rend.material.SetColor("_OutlineColor", color);
    //    rend.material.SetFloat("_Scale", scaleFactor);
    //    rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    //    m_Target.GetComponent<Collider>().enabled = false;

    //    rend.enabled = false;
    //    return rend;
    //}

    private static Ray GetMouseRay()
    {
        return Camera.main.ScreenPointToRay(Input.mousePosition);
    }
}
