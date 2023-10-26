using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GW.Statistics
{
    public class LevelDisplay : MonoBehaviour
    {
        private BaseStats m_Stats;
        // Start is called before the first frame update
        void Awake()
        {
            m_Stats = GameObject.FindWithTag("Player").GetComponent<BaseStats>();
        }

        // Update is called once per frame
        void Update()
        {
            GetComponent<Text>().text = m_Stats.GetLevel().ToString();
        }
    }
}