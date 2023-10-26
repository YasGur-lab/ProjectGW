using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GW.Core
{
    public class DestroyAfterEffect : MonoBehaviour
    {
        [SerializeField] GameObject m_TargetToDestroy = null;

        // Update is called once per frame
        void Update()
        {
            if (!GetComponentInChildren<ParticleSystem>().IsAlive())
            {
                if (m_TargetToDestroy != null)
                    Destroy(m_TargetToDestroy);
                else
                    Destroy(gameObject);
            }
        }
    }
}
