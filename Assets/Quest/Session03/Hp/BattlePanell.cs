using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePanel : MonoBehaviour
{
    public static BattlePanel Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public static event Action OnVictory;

    [Header("Head UI")]
    [SerializeField] private Canvas overlayCanvas;      // Screen Space Overlay 캔버스
    [SerializeField] private GameObject headUiPrefab;   // EnemyHeadUI가 달린 프리팹(자식에 Text 포함)

    [Header("Hit Settings")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string floorTag = "Floor";
    [SerializeField] private string bulletTag = "Bullet"; // 빈 문자열이면 '총알 제한' 없음
    [SerializeField] private float collisionDamageToEnemy = 1f;
    [SerializeField] private float collisionDamageToPlayer = 1f;
    [SerializeField] private float hitCooldownEnemy = 0.05f;
    [SerializeField] private float hitCooldownPlayer = 0.25f;

    [Header("HP System")]
    [SerializeField] private Hp_Subject hp_Subject = null;
    [SerializeField] private MyHp_Observer myHp_Observer = null;
    [SerializeField] private EnemyHp_Observer enemyHp_Observer = null;

    [Header("UI")]
    [SerializeField] private Text nextButton = null;
    [SerializeField] private Text myHpText = null;
    [SerializeField] private Text enemyHpText = null;

    private float originMyHp = 10f;
    private float originEnemyHp = 10f;
    private float currentMyHp = 0f;
    private float currentEnemyHp = 0f;

    // 전투 종료 플래그
    private bool isFinished = false;

    // ------ 승리 처리 ------
    public void Win()
    {
        if (isFinished) return; // 중복 방지
        isFinished = true;

        currentEnemyHp = Mathf.Max(0f, currentEnemyHp);
        hp_Subject.Changed(currentMyHp / originMyHp, currentEnemyHp / originEnemyHp);
        UpdateHpUI();

        if (nextButton) nextButton.text = "나의 승리";
        Debug.Log("--- 나의 승리 ---");

        DespawnAllEnemies();
        OnVictory?.Invoke();
    }

    // ------ 적 피격 ------
    public void EnemyHit(float damage = 1f)
    {
        if (isFinished) return;

        currentEnemyHp = Mathf.Max(0f, currentEnemyHp - damage);
        hp_Subject.Changed(currentMyHp / originMyHp, currentEnemyHp / originEnemyHp);
        UpdateHpUI();

        if (currentEnemyHp <= 0f) Win();
        else if (nextButton) nextButton.text = "다음 턴";
    }

    // ------ 플레이어 피격 ------
    public void PlayerHit(float damage = 1f)
    {
        if (isFinished) return;

        currentMyHp = Mathf.Max(0f, currentMyHp - damage);
        hp_Subject.Changed(currentMyHp / originMyHp, currentEnemyHp / originEnemyHp);
        UpdateHpUI();

        if (currentMyHp <= 0f)
        {
            isFinished = true;
            if (nextButton) nextButton.text = "상대 승리";
            Debug.Log("--- 상대 승리 ---");
        }
        else if (nextButton) nextButton.text = "다음 턴";
    }

    private void Start()
    {
        myHp_Observer.Init(hp_Subject);
        enemyHp_Observer.Init(hp_Subject);

        currentMyHp = originMyHp = 10f;
        currentEnemyHp = originEnemyHp = 10f;

        hp_Subject.RegisterObserver(myHp_Observer);
        hp_Subject.RegisterObserver(enemyHp_Observer);

        hp_Subject.Changed(currentMyHp / originMyHp, currentEnemyHp / originEnemyHp);
        UpdateHpUI();

        // --- Enemy relay attach (root/child 태그/콜라이더 모두 대응) ---
        var allTransforms = GameObject.FindObjectsOfType<Transform>(true);
        var enemyRoots = new HashSet<GameObject>();

        // (1) Enemy 태그가 루트/자식 어디에 있든 "루트 GameObject"를 수집
        foreach (var tr in allTransforms)
        {
            if (!tr.CompareTag(enemyTag)) continue;
            var root = tr.root ? tr.root.gameObject : tr.gameObject;
            enemyRoots.Add(root);
        }

        // (2) 각 루트 아래의 모든 Collider에 릴레이 부착
        foreach (var root in enemyRoots)
        {
            var colliders = root.GetComponentsInChildren<Collider>(true);
            foreach (var col in colliders)
            {
                var go = col.gameObject;
                var relay = go.GetComponent<_EnemyHitRelay>();
                if (relay == null) relay = go.AddComponent<_EnemyHitRelay>();
                relay.Init(this, floorTag, bulletTag, collisionDamageToEnemy, hitCooldownEnemy, enemyTag);
            }
        }

        // === Enemy Head UI 생성 === (기준점: HeadupUI -> Head -> head -> 루트)
        if (overlayCanvas != null && headUiPrefab != null)
        {
            foreach (var root in enemyRoots)
            {
                Transform anchor = FindAnchorTransform(root.transform,
                    new[] { "HeadupUI", "Head", "head" });

                var uiGO = Instantiate(headUiPrefab, overlayCanvas.transform);
                var follow = uiGO.GetComponent<EnemyHeadUI>();
                follow.target = anchor;       // 기준 트랜스폼을 따라감
                // HeadupUI를 정확한 위치에 두었다면 보통 worldOffset은 (0,0,0)이 적절
                // 필요 시 follow.worldOffset으로 미세 조정

                // 초기 문구
                follow.SetText($"Enemy HP {currentEnemyHp:0}/{originEnemyHp:0}\n패배조건 ≤ {0:0}");
            }
        }

        // --- Player relay attach (Enemy와 부딪히면 PlayerHit) ---
        foreach (var go in GameObject.FindGameObjectsWithTag(playerTag))
        {
            if (go.GetComponent<CharacterController>() != null)
            {
                var ccrelay = go.GetComponent<_PlayerCCRelay>();
                if (ccrelay == null) ccrelay = go.AddComponent<_PlayerCCRelay>();
                ccrelay.Init(this, enemyTag, collisionDamageToPlayer, hitCooldownPlayer);
            }
            else
            {
                var relay = go.GetComponent<_PlayerHitRelay>();
                if (relay == null) relay = go.AddComponent<_PlayerHitRelay>();
                relay.Init(this, enemyTag, collisionDamageToPlayer, hitCooldownPlayer);
            }
        }
    }

    public void Next()
    {
        if (isFinished) { Debug.Log("--- 전투 종료 ---"); return; }

        int attackIndex = UnityEngine.Random.Range(0, 2);
        if (attackIndex == 0) currentEnemyHp = Mathf.Max(0f, currentEnemyHp - 1f);
        else currentMyHp = Mathf.Max(0f, currentMyHp - 1f);

        hp_Subject.Changed(currentMyHp / originMyHp, currentEnemyHp / originEnemyHp);
        UpdateHpUI();

        if (currentMyHp <= 0f)
        {
            isFinished = true;
            if (nextButton) nextButton.text = "상대 승리";
            Debug.Log("--- 상대 승리 ---");
        }
        else if (currentEnemyHp <= 0f)
        {
            Win();
        }
        else if (nextButton) nextButton.text = "다음 턴";
    }

    private void UpdateHpUI()
    {
        if (myHpText) myHpText.text = $"패배조건  : {currentMyHp:0}/{originMyHp:0}";
        if (enemyHpText) enemyHpText.text = $"승리조건 : {currentEnemyHp:0}/{originEnemyHp:0}";
    }

    // --- 적 전부 제거(루트/자식 태그/릴레이 모두 커버) ---
    private void DespawnAllEnemies()
    {
        var toDestroy = new HashSet<GameObject>();

        // (A) 루트에 Enemy 태그
        foreach (var go in GameObject.FindGameObjectsWithTag(enemyTag))
            toDestroy.Add(go);

        // (B) 자식에만 Enemy 태그 → 루트 제거
        var allTransforms = GameObject.FindObjectsOfType<Transform>(true);
        foreach (var tr in allTransforms)
        {
            if (tr.CompareTag(enemyTag))
            {
                var root = tr.root ? tr.root.gameObject : tr.gameObject;
                toDestroy.Add(root);
            }
        }

        // (C) 자식 콜라이더에 붙인 릴레이의 루트까지 제거
        var relays = FindObjectsOfType<_EnemyHitRelay>(true);
        foreach (var r in relays)
        {
            var root = r.transform.root ? r.transform.root.gameObject : r.gameObject;
            toDestroy.Add(root);
        }

        foreach (var go in toDestroy)
            if (go) Destroy(go);
    }

    // === 기준 트랜스폼 탐색: 이름 순서대로 시도, 없으면 루트 반환 ===
    private static Transform FindAnchorTransform(Transform root, string[] namesToTry)
    {
        foreach (var n in namesToTry)
        {
            var t = root.Find(n);
            if (t != null) return t;
        }
        return root; // 폴백
    }

    // ----------------- 내부 클래스들 -----------------

    // 적: 총알 등 피격 릴레이 (자식 콜라이더 포함)
    private class _EnemyHitRelay : MonoBehaviour
    {
        private BattlePanel panel;
        private string floorTag, bulletTag, enemyTag;
        private float damagePerHit, cooldown, lastHitTime = -999f;

        private const bool DEBUG_HIT = false;

        public void Init(BattlePanel panel, string floorTag, string bulletTag, float damage, float cooldown, string enemyTag)
        {
            this.panel = panel;
            this.floorTag = floorTag;
            this.bulletTag = bulletTag; // 비거나 null이면 '총알 제한' 없음
            this.damagePerHit = damage;
            this.cooldown = cooldown;
            this.enemyTag = enemyTag;
        }

        private void TryHit(GameObject otherGO)
        {
            if (panel == null) return;

            // (A) 바닥 무시
            if (!string.IsNullOrEmpty(floorTag) && otherGO.CompareTag(floorTag))
            {
                if (DEBUG_HIT) Debug.Log($"[EnemyRelay] Ignore floor: {otherGO.name}");
                return;
            }

            // (B) bulletTag가 지정되어 있으면 그 태그만 인정
            if (!string.IsNullOrEmpty(bulletTag))
            {
                var root = otherGO.transform.root ? otherGO.transform.root.gameObject : otherGO;
                if (!otherGO.CompareTag(bulletTag) && !root.CompareTag(bulletTag))
                {
                    if (DEBUG_HIT) Debug.Log($"[EnemyRelay] Not bullet tag: {otherGO.name}/{root.tag}");
                    return;
                }
            }
            // bulletTag가 비어 있으면 모든 비바닥 충돌을 피격으로 처리

            // (C) 쿨다운
            if (Time.time - lastHitTime < cooldown)
            {
                if (DEBUG_HIT) Debug.Log($"[EnemyRelay] Cooldown: {gameObject.name}");
                return;
            }
            lastHitTime = Time.time;

            if (DEBUG_HIT) Debug.Log($"[EnemyRelay] HIT by {otherGO.name} at {gameObject.name}");
            panel.EnemyHit(damagePerHit);
        }

        private void OnCollisionEnter(Collision c) => TryHit(c.collider.gameObject);
        private void OnTriggerEnter(Collider o) => TryHit(o.gameObject);
    }

    // 플레이어: 일반 물리 충돌/트리거 릴레이
    private class _PlayerHitRelay : MonoBehaviour
    {
        private BattlePanel panel;
        private string enemyTag;
        private float damagePerHit, cooldown, lastHitTime = -999f;

        public void Init(BattlePanel panel, string enemyTag, float damage, float cooldown)
        {
            this.panel = panel; this.enemyTag = enemyTag;
            this.damagePerHit = damage; this.cooldown = cooldown;
        }

        private void TryHit(GameObject otherGO)
        {
            if (panel == null) return;

            var root = otherGO.transform.root ? otherGO.transform.root.gameObject : otherGO;
            if (!otherGO.CompareTag(enemyTag) && !root.CompareTag(enemyTag)) return;

            if (Time.time - lastHitTime < cooldown) return;
            lastHitTime = Time.time;

            panel.PlayerHit(damagePerHit);
        }

        private void OnCollisionEnter(Collision c) => TryHit(c.collider.gameObject);
        private void OnTriggerEnter(Collider o) => TryHit(o.gameObject);
    }

    // 플레이어: CharacterController 전용 릴레이
    private class _PlayerCCRelay : MonoBehaviour
    {
        private BattlePanel panel;
        private string enemyTag;
        private float damagePerHit, cooldown, lastHitTime = -999f;

        public void Init(BattlePanel panel, string enemyTag, float damage, float cooldown)
        {
            this.panel = panel; this.enemyTag = enemyTag;
            this.damagePerHit = damage; this.cooldown = cooldown;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (panel == null) return;

            var other = hit.collider.gameObject;
            var root = other.transform.root ? other.transform.root.gameObject : other;

            if (!other.CompareTag(enemyTag) && !root.CompareTag(enemyTag)) return;
            if (Time.time - lastHitTime < cooldown) return;

            lastHitTime = Time.time;
            panel.PlayerHit(damagePerHit);
        }
    }
}
