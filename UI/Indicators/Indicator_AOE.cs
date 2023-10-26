using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator_AOE : MonoBehaviour
{
    [SerializeField] private LineRenderer m_LR;
    [SerializeField] private float circleSegments = 36;
    [SerializeField] private float m_Radius = 5.0f;
    [SerializeField] private LayerMask m_LayerMask;
    private void Start()
    {
        GenerateCircle();

    }

    private void GenerateCircle()
    {
        //m_LR.positionCount = (int)circleSegments + 1;

        //for (int i = 0; i <= circleSegments; i++)
        //{
        //    float angle = 2 * Mathf.PI * i / circleSegments;
        //    float x = Mathf.Sin(angle) * m_Radius;
        //    float z = Mathf.Cos(angle) * m_Radius;
        //    m_LR.SetPosition(i, new Vector3(x, 0, z));
        //}

        m_LR.positionCount = (int)circleSegments + 1;

        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = 2 * Mathf.PI * i / circleSegments;
            float x = Mathf.Sin(angle) * m_Radius;
            float z = Mathf.Cos(angle) * m_Radius;

            // Calculate the position of the circle point relative to the indicator's position
            Vector3 circlePoint = transform.position + new Vector3(x, 0.0f, z);

            RaycastHit hit;
            if (Physics.Raycast(circlePoint + Vector3.up * 100, Vector3.down, out hit, Mathf.Infinity, m_LayerMask))
            {
                circlePoint.y = hit.point.y + 0.01f;
            }
            
            m_LR.SetPosition(i, circlePoint);
        }
    }

    public void SetRadius(float mRadius)
    {
        m_Radius = mRadius;
    }

    public void SetLayerMask(LayerMask layerMask)
    {
        m_LayerMask = layerMask;
    }
}