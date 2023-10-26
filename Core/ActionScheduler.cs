using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GW.Core
{
    public class ActionScheduler : MonoBehaviour
    {
        private IAction m_CurrentAction;

        public void StartAction(IAction action)
        {
            if (m_CurrentAction == action) return;
            if (m_CurrentAction != null)
            {
                //print("Action cancelled: " + m_CurrentAction);
                m_CurrentAction.Cancel();
            }

            m_CurrentAction = action;
        }

        public void CancelCurrentAction()
        {
            StartAction(null);
        }
    }
}
