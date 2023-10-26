using System.Collections;
using UnityEngine;

namespace GW.SceneManagement
{

    public class Fader : MonoBehaviour
    {
        private CanvasGroup m_Canvas;

        private void Awake()
        {
            m_Canvas = GetComponent<CanvasGroup>();
        }

        public void FadeOutImmediate()
        {
            m_Canvas.alpha = 1;
        }

        public IEnumerator FadeOut(float time)
        {
            while (m_Canvas.alpha < 1) // alpha isnt 1
            {
                m_Canvas.alpha += Time.deltaTime / time;
                yield return null;
            }
        }

        public IEnumerator FadeIn(float time)
        {
            while (m_Canvas.alpha > 0) // alpha isnt 1
            {
                m_Canvas.alpha -= Time.deltaTime / time;
                yield return null;
            }
        }
    }
}
