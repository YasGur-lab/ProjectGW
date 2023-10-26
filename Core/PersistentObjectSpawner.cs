using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GW.Core
{
    public class PersistentObjectSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject m_PersistentObjectPrefab;

        private static bool m_hasSpawned = false;

        // Start is called before the first frame update
        void Awake()
        {
            if (m_hasSpawned) return;
            SpawnPersistentObjects();

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            m_hasSpawned = true;
        }

        private void SpawnPersistentObjects()
        {
            GameObject persistentObjects = Instantiate(m_PersistentObjectPrefab);
            DontDestroyOnLoad(persistentObjects);
        }
    }
}
