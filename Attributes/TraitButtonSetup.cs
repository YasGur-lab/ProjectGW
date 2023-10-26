using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GW.Statistics
{
    public class TraitButtonSetup : MonoBehaviour
    {
        ProfessionsAttributes m_AssignedAttribute;
        public ProfessionsAttributes GetAssignedAttribute() { return m_AssignedAttribute; }
        public void SetAssignedTrait(ProfessionsAttributes trait) { m_AssignedAttribute = trait; }
    }
}