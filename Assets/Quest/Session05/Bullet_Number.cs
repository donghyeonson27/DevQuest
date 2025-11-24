using UnityEngine;
using UnityEngine.UI;

public class Bullet_Number : MonoBehaviour
{
    [SerializeField] private GunXR gun;   // 씬에 있는 Gun 오브젝트
    [SerializeField] private Text ammoText;

    private void Update()
    {
        if (gun == null || ammoText == null)
            return;

        // "현재 / 최대" 형식으로 표시
        ammoText.text = $"{gun.CurrentAmmo} / {gun.MaxAmmo}";
    }
}

