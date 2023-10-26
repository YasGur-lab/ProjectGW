using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GW.Statistics
{
    public class ExperienceDisplay : MonoBehaviour
    {
        public void UpdateData(float experience, float maxExperience)
        {
            GetComponent<Text>().text = Mathf.Ceil(experience) + "/" + maxExperience;
        }
    }
}