using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_FPS : MonoBehaviour
{
    private float m_FPS;
    [SerializeField] TextMeshProUGUI m_Text;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(GetFPS), 1, 1);
    }
    
    void GetFPS()
    {
        m_FPS = (int)(1f / Time.unscaledDeltaTime);
        m_Text.text = "FPS: " + m_FPS;
    }
}
