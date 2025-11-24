using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class GunXR : MonoBehaviour
{
    [Header("Ammo")]
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private int currentAmmo;
    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo;


    [Header("Bullet")]
    [SerializeField] private ObjectPool bulletPool;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletLifeTime = 2f;
    [SerializeField] private Transform firePoint;     // Gun 하위 FirePoint

    [Header("Haptics")]
    [Range(0f, 1f)]
    public float fireAmplitude = 0.7f;
    public float fireDuration = 0.08f;

    [Header("Recoil (Rotation)")]
    [SerializeField] private float recoilAngle = 8f;        // 위로 튀는 각도(도)
    [SerializeField] private float recoilReturnSpeed = 15f; // 원위치 복귀 속도

    [Header("Debug / Editor Test")]
    [SerializeField] private bool useMouseTest = true;      // 에디터에서 마우스로 테스트
    [SerializeField] private KeyCode testKey = KeyCode.Mouse0;

    private XRGrabInteractable grab;

    // 회전 반동 상태
    private Quaternion defaultLocalRot;
    private float currentRecoilAngle = 0f;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        defaultLocalRot = transform.localRotation; // Gun의 기본 로컬 회전 저장
        currentAmmo = maxAmmo;

    }

    private void OnEnable()
    {
        if (grab != null)
            grab.activated.AddListener(OnActivated);
    }

    private void OnDisable()
    {
        if (grab != null)
            grab.activated.RemoveListener(OnActivated);
    }

    private void Update()
    {
        // 에디터 / 노트북 환경에서 테스트용: 마우스 왼쪽 클릭으로도 발사 + 반동
        if (useMouseTest && Input.GetKeyDown(testKey))
        {
            TestShootAndRecoil();
        }

        // 반동 각도를 서서히 0으로 줄이면서 원래 회전으로 복귀
        if (Mathf.Abs(currentRecoilAngle) > 0.001f)
        {
            currentRecoilAngle = Mathf.Lerp(
                currentRecoilAngle,
                0f,
                recoilReturnSpeed * Time.deltaTime
            );

            // x축 기준으로 총구가 살짝 위로 들리는 회전
            transform.localRotation =
                defaultLocalRot * Quaternion.Euler(-currentRecoilAngle, 0f, 0f);
        }
    }

    // XR 트리거(Activate)로 들어올 때
    private void OnActivated(ActivateEventArgs args)
    {
        if (Shoot())
        {
            ApplyRecoil();
            SendHaptics(args);
        }
    }

    // 에디터에서 마우스로 테스트할 때
    private void TestShootAndRecoil()
    {
        if (Shoot())
        {
            ApplyRecoil();
        }
    }

    private bool Shoot()
    {
        // 탄약 없으면 발사 안 함
        if (currentAmmo <= 0)
        {
            Debug.Log("[GunXR] 탄약이 없습니다.");
            return false;
        }

        if (bulletPool == null || firePoint == null)
        {
            Debug.LogWarning("[GunXR] bulletPool 또는 firePoint가 설정되지 않았습니다.", this);
            return false;
        }

        GameObject bullet = bulletPool.GetFromPool();
        if (bullet == null)
        {
            Debug.LogWarning("[GunXR] 풀에서 bullet을 가져오지 못했습니다.", this);
            return false;
        }

        bullet.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);

        if (!bullet.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            Debug.LogWarning("[GunXR] bullet에 Rigidbody가 없습니다. 풀로 되돌립니다.", bullet);
            bulletPool.ReturnToPool(bullet);
            return false;
        }

        rb.linearVelocity = firePoint.forward * bulletSpeed;

        // 실제로 발사된 경우에만 탄약 1 감소
        currentAmmo--;

        StartCoroutine(DeactivateBullet(bullet));
        return true;
    }


    private IEnumerator DeactivateBullet(GameObject bullet)
    {
        yield return new WaitForSeconds(bulletLifeTime);
        bulletPool.ReturnToPool(bullet);
    }

    private void ApplyRecoil()
    {
        currentRecoilAngle += recoilAngle;
    }

    private void SendHaptics(ActivateEventArgs args)
    {
        if (args.interactorObject is XRBaseInputInteractor controllerInteractor)
        {
            var xrController = controllerInteractor.xrController;
            if (xrController != null)
            {
                xrController.SendHapticImpulse(fireAmplitude, fireDuration);
            }
        }
    }
}
