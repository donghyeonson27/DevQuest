using System.Collections;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private ObjectPool bulletPool;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletLifeTime = 2f;

    [Header("Fire Settings")]
    [SerializeField] private Transform firePoint;

    private void Update()
    {
        // 테스트용: 마우스 왼쪽 버튼 클릭 시 발사
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        // 필수 레퍼런스 체크
        if (bulletPool == null || firePoint == null) return;

        GameObject bullet = bulletPool.GetFromPool();

        bullet.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);

        if (bullet.TryGetComponent(out Rigidbody rb))
        {
            
            rb.linearVelocity = firePoint.forward * bulletSpeed;
        }

        StartCoroutine(DeactivateBullet(bullet));
    }

    private IEnumerator DeactivateBullet(GameObject bullet)
    {
        yield return new WaitForSeconds(bulletLifeTime);
        bulletPool.ReturnToPool(bullet);
    }
}
