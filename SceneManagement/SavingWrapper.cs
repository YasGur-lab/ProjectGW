using System.Collections;
using System.Collections.Generic;
using GW.Saving;
using UnityEngine;

namespace GW.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        private const string m_DefaultSaveFile = "save";
        [SerializeField] private float fadeWaitTime = 1.0f;
        [SerializeField] private float fadeInTime = 1.0f;

        private void Awake()
        {
            StartCoroutine(LoadLastScene());
        }

        IEnumerator LoadLastScene()
        {
            yield return GetComponent<SavingSystem>().LoadLastScene(m_DefaultSaveFile);
            Fader fader = FindObjectOfType<Fader>();
            fader.FadeOutImmediate();
            yield return new WaitForSeconds(fadeWaitTime);
            yield return fader.FadeIn(fadeInTime);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                Save();
            }
        }

        public void Save()
        {
            GetComponent<SavingSystem>().Save(m_DefaultSaveFile);
        }

        public void Load()
        {
            GetComponent<SavingSystem>().Load(m_DefaultSaveFile);
        }
    }
}
