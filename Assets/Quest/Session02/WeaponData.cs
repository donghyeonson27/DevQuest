using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    [CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        public string weaponName;
        public int damage;
        public float fireRate;
        public int maxAmmo;
    }
