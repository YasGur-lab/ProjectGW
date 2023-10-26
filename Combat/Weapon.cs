using System.Collections.Generic;
using GW.Attributes;
using GW.Statistics;
using UnityEngine;

namespace GW.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Make New Weapon", order = 0)]
    public class Weapon : ScriptableObject
    {
        [SerializeField] public WeaponTypes m_WeaponType;
        [SerializeField] public WeaponDamageTypes m_WeaponDamageType;
        [SerializeField] public ProfessionsAttributes m_Attribute;
        [SerializeField] public Skill m_Skill;
        [SerializeField] private float m_WeaponRange = 2.0f;
        [SerializeField] public float m_TimeBetweenAttacks = 1.0f;
        [SerializeField] public float m_MinimumDamage = 5.0f;
        [SerializeField] public float m_MaximumDamage = 5.0f;
        [SerializeField] public float m_PercentageBonus = 0.0f;
        [SerializeField] private bool m_IsRightHanded = true;
        [SerializeField] private Projectile m_Projectile = null;
        [SerializeField] private GameObject m_EquipedPrefab = null;

        private const string m_WeaponName = "Weapon";
        public void Spawn(Transform rightHand, Transform leftHand, Animator animator, Fighter fighter)
        {
            DestroyOldWeapon(rightHand, leftHand);

            if (m_EquipedPrefab != null)
            {
                GameObject weapon = Instantiate(m_EquipedPrefab, GetTransform(rightHand, leftHand).transform);
                weapon.name = m_WeaponName;
            }

            if(m_WeaponType == WeaponTypes.Sword)
                m_WeaponRange = fighter.GetMeleeRange();
            else
            {
                m_WeaponRange = fighter.GetCastingRange();
            }
        }

        private void DestroyOldWeapon(Transform rightHand, Transform leftHand)
        {
            Transform oldWeapon = rightHand.Find(m_WeaponName);

            if (oldWeapon == null) oldWeapon = leftHand.Find(m_WeaponName);
            if (oldWeapon == null) return;
            oldWeapon.name = "Destroying";
            Destroy(oldWeapon.gameObject);
        }

        private Transform GetTransform(Transform rightHand, Transform leftHand)
        {
            Transform handTransform;
            if (m_IsRightHanded) handTransform = rightHand;
            else handTransform = leftHand;
            return handTransform;
        }

        public bool HasProjectile()
        {
            return m_Projectile != null;

        }

        public void LaunchProjectile(Transform rightHand, Transform leftHand, Health target, GameObject instigator, float calculatedDamage, List<string> tags)
        {
            Projectile projectileInstance = Instantiate(m_Projectile, GetTransform(rightHand, leftHand).position,
                Quaternion.identity);
            projectileInstance.Init(instigator , calculatedDamage, tags, target);
        }

        public float GetTimeBetweenAttacks()
        {
            return m_TimeBetweenAttacks;
        }

        public float GetRange()
        {
            return m_WeaponRange;
        }

        public Skill GetSkill() => m_Skill;
    }
}