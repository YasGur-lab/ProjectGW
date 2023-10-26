using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace GW.Combat
{
    public class WeaponPickup : MonoBehaviour
    {
        [SerializeField] private Weapon m_Weapon = null;
        // Start is called before the first frame update
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                other.GetComponent<Fighter>().EquipWeapon(m_Weapon);
                //Destroy(gameObject);
            }
        }
    }
}
