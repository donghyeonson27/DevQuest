using UnityEngine;
using UnityEngine.UI;

public class EnemyHeadUI : MonoBehaviour
{
    [Header("Target & UI")]
    public Transform target;                 // 따라갈 기준(권장: Enemy 프리팹의 'HeadupUI')
    public Vector3 worldOffset = new Vector3(0, 1.8f, 0);
    public Text label;                       // TMP 쓰면 TMP_Text로 교체

    [Header("Auto Anchor (no target)")]
    public bool useAutoAnchorIfNoTarget = true;   // target 미지정 시 자동 머리 추정
    [Range(0f, 1f)]
    public float boundsHeadOffsetRatio = 0.15f;   // 모델 키의 몇 %만큼 위로 더 띄울지

    private Camera cam;
    private RectTransform rt;
    private Renderer[] _renderers;                // 자동 머리 추정용(모델 렌더러들 캐시)
    private Transform _root;                      // 자동시 기준이 될 루트

    private void Awake()
    {
        cam = Camera.main;
        rt = GetComponent<RectTransform>();

        // target이 없고 자동 사용이면, 루트/렌더러를 캐시
        if (target == null && useAutoAnchorIfNoTarget)
        {
            // 가장 가까운 상위의 Enemy(또는 그냥 최상단) 기준으로 렌더러 수집
            _root = transform.root;
            _renderers = _root.GetComponentsInChildren<Renderer>(true);
        }
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        Vector3 worldPos;

        if (target != null)
        {
            // 명시된 기준(HeadupUI 등)을 그대로 사용
            worldPos = target.position + worldOffset;
        }
        else if (useAutoAnchorIfNoTarget && _renderers != null && _renderers.Length > 0)
        {
            // 모든 렌더러의 바운드를 합쳐 "몸통 바운드" 계산
            Bounds b = _renderers[0].bounds;
            for (int i = 1; i < _renderers.Length; i++)
                b.Encapsulate(_renderers[i].bounds);

            // 바운드 상단 + 약간의 여유 높이
            float extra = b.size.y * boundsHeadOffsetRatio;
            worldPos = new Vector3(b.center.x, b.max.y + extra, b.center.z);
        }
        else
        {
            // 최소 폴백: 그냥 자기 자신 + 오프셋
            worldPos = transform.position + worldOffset;
        }

        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // 카메라 뒤면 숨김
        bool behind = screenPos.z < 0f;
        if (label) label.enabled = !behind;
        if (behind) return;

        rt.position = screenPos;
    }

    public void SetText(string text)
    {
        if (label) label.text = text;
    }
}
