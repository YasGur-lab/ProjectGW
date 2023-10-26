using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using GW.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace GW.Core
{

    public class Respawn : MonoBehaviour
    {
        [SerializeField] private Transform m_RespawnPoint;
        [SerializeField] public int m_SceneToLoad = 1;
        [SerializeField] private float fadeWaitTime = 0.5f;
        [SerializeField] private float fadeInTime = 2.0f;
        [SerializeField] private float fadeOutTime = 1.0f;

        private GameObject m_Player;

        void Start()
        {
            if (GameObject.FindWithTag("Outpost"))
                m_SceneToLoad = SceneManager.GetActiveScene().buildIndex;
        }

        // Update is called once per frame
        void Update()
        {
            m_Player = GameObject.FindWithTag("Player");
            if (m_Player.GetComponent<Health>().IsDead())
                RespawnPlayer();
        }

        private void RespawnPlayer()
        {
            StartCoroutine(LoadScene());
        }

        private IEnumerator LoadScene()
        {
            if (m_SceneToLoad < 0)
            {
                Debug.LogError("Scene to load not set.");
                yield break;
            }
            
            Fader fader = GameObject.FindObjectOfType<Fader>();

            yield return fader.FadeOut(fadeOutTime);

            SavingWrapper wrapper = FindObjectOfType<SavingWrapper>();

            wrapper.Save();

            yield return SceneManager.LoadSceneAsync(0); //m_SceneToLoad

            wrapper.Load();

            //UpdatePlayer(m_SpawnPoint);
            m_Player = GameObject.FindWithTag("Player");
            m_Player.GetComponent<Health>().ResetGameObject();
            m_RespawnPoint = GameObject.Find("RespawnPoint").transform;
            UpdatePlayer(m_RespawnPoint);

            yield return new WaitForSeconds(fadeWaitTime);
            yield return fader.FadeIn(fadeInTime);
        }

        //public object CaptureState()
        //{
        //    //Dictionary<string, object> data = new Dictionary<string, object>();
        //    //data["SceneToLoad"] = 1;
        //    //return data;
        //    //Dictionary<string, object> state = new Dictionary<string, object>();
        //    ////state["SceneToLoad"] = m_SceneToLoad;
        //    //return state;

        //}

        //public void RestoreState(object state)
        //{
        //    //Dictionary<string, object> data = (Dictionary<string, object>)state;

        //    //if (data.ContainsKey("SceneToLoad"))
        //    //{
        //    //    m_SceneToLoad = (int)data["SceneToLoad"];
        //    //}
        //}

        private void UpdatePlayer(Transform SpawnPoint)
        {
            m_Player.transform.position = SpawnPoint.position;
            //m_Player.GetComponent<NavMeshAgent>().Warp(SpawnPoint.position);
            m_Player.transform.rotation = SpawnPoint.rotation;
        }
    }
}
