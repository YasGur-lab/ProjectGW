using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Damage : MonoBehaviour
{
    [SerializeField] private TextMeshPro m_Text;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        transform.position += new Vector3(0.0f, 2.5f, 0.0f) * Time.deltaTime;
    }

    public void InitPopup(int damage)
    {
        m_Text.SetText(damage.ToString());
    }
}
