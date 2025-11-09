using System.Collections;
using UnityEngine;
using System.Collections;


public class gameManager : MonoBehaviour
{
    public ObjectPool bulletPool;
    public float bulletSpeed = 10f;

    [SerializeField]
    private Transform firePoint;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {       
        GameObject bullet = bulletPool.GetFromPool();

        bullet.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);

        if (bullet.TryGetComponent(out Rigidbody rb))
            rb.velocity = firePoint.forward * bulletSpeed;

        StartCoroutine(DeactivateBullet(bullet));
    }


    IEnumerator DeactivateBullet(GameObject bullet)
    {
        yield return new WaitForSeconds(2f);
        bulletPool.ReturnToPool(bullet);
    }
}
