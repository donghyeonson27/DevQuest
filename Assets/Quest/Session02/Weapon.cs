using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData data;

    void Start()
    {

        Debug.Log($"{data.weaponName}의 공격력은 {data.damage} 입니다!");

    }

}