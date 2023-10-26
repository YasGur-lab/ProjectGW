using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace GW.SceneManagement
{
    public class Portal : MonoBehaviour
    {
        enum PortalID
        {
            MapOneStart, MapTwoStart, MapThreeStart, MapOneSecondPortal, MapTwoSecondPortal, MapThreeSecondPortal
        }

        enum PortalToGoTo
        {
            MapOneStart, MapTwoStart, MapThreeStart, MapOneSecondPortal, MapTwoSecondPortal, MapThreeSecondPortal
        }

        [SerializeField] private int m_SceneToLoad = -1;
        [SerializeField] private Transform m_SpawnPoint;
        [SerializeField] private float fadeWaitTime = 0.5f;
        [SerializeField] private float fadeInTime = 2.0f;
        [SerializeField] private float fadeOutTime = 1.0f;
        [SerializeField] PortalID m_PortalID;
        [SerializeField] PortalToGoTo m_PortalToGoTo;


        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                StartCoroutine(Transition());
            }
        }

        private IEnumerator Transition()
        {
            if (m_SceneToLoad < 0)
            {
                Debug.LogError("Scene to load not set.");
                yield break;
            }

            DontDestroyOnLoad(gameObject);

            Fader fader = FindObjectOfType<Fader>();

            yield return fader.FadeOut(fadeOutTime);

            SavingWrapper wrapper = FindObjectOfType<SavingWrapper>();

            wrapper.Save();

            fader.FadeOutImmediate();

            yield return SceneManager.LoadSceneAsync(m_SceneToLoad);

            fader.FadeOutImmediate();

            wrapper.Load();

            Portal otherPortal = GetOtherPortal();
            UpdatePlayer(otherPortal);

            yield return  new WaitForSeconds( fadeWaitTime);
            yield return fader.FadeIn(fadeInTime);

            Destroy(gameObject, 1f);
        }

        private Portal GetOtherPortal()
        {
            foreach (Portal portal in FindObjectsOfType<Portal>())
            {
                if (portal == this) continue;
                if (portal.m_PortalID == (PortalID)m_PortalToGoTo)
                    return portal;
            }
            return null;
        }

        private void UpdatePlayer(Portal otherPortal)
        {
            GameObject player = GameObject.FindWithTag("Player");

            player.transform.position = otherPortal.m_SpawnPoint.position;

            //player.GetComponent<NavMeshAgent>().Warp(otherPortal.m_SpawnPoint.position);
            player.transform.rotation = otherPortal.m_SpawnPoint.rotation;
        }
    }
}
