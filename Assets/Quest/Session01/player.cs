using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform cameraTransform;

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float sprintMultiplier = 1.8f;

    [Header("Game Over Settings")]
    [SerializeField] private int maxHits = 10;            // 적과 몇 번 부딪히면 게임 오버인지
    [SerializeField] private GameObject gameOverPanel;    // 게임 오버 UI (Assignment 씬에서 연결)
    [SerializeField] private string enemyTag = "Enemy";   // 적 태그 이름

    private PlayerActions input;
    private Vector2 moveInput;
    private float yVelocity;

    private int currentHits = 0;      // 현재 부딪힌 횟수
    private bool isGameOver = false;  // 게임 오버 상태 여부

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        input = new PlayerActions();

        // 시작할 때 게임 오버 UI는 감추기
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        input.Players.Enable();

        input.Players.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Players.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnDisable()
    {
        input.Players.Move.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Players.Move.canceled -= ctx => moveInput = Vector2.zero;

        input.Players.Disable();
    }

    private void Update()
    {
        // 게임 오버 상태면 이동 막기
        if (isGameOver)
            return;

        Vector3 camForward = cameraTransform ? cameraTransform.forward : Vector3.forward;
        Vector3 camRight = cameraTransform ? cameraTransform.right : Vector3.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camForward * moveInput.y + camRight * moveInput.x;
        if (move.sqrMagnitude > 1f)
            move.Normalize();

        //  Shift + W(전진)일 때만 move배율 적용
        Keyboard kb = Keyboard.current;
        bool shiftHeld = kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);
        bool movingForward = moveInput.y > 0.1f; // 미세 노이즈 방지용 문턱

        if (shiftHeld && movingForward)
            move *= sprintMultiplier;

        if (controller.isGrounded && yVelocity < 0f)
            yVelocity = -2f; // 접지 클램프

        yVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed + Vector3.up * yVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    // Trigger 콜라이더로 적과 부딪힌 걸 체크
    private void OnTriggerEnter(Collider other)
    {
        if (isGameOver)
            return;

        // 충돌한 상대가 Enemy 태그면 카운트
        if (other.CompareTag(enemyTag))
        {
            currentHits++;
            Debug.Log($"[Player] Enemy와 충돌: {currentHits}/{maxHits}", other);

            if (currentHits >= maxHits)
            {
                GameOver();
            }
        }
    }

    private void GameOver()
    {
        isGameOver = true;

        Debug.Log("[Player] GAME OVER");

        // Game Over UI 띄우기
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        else
            Debug.LogWarning("[Player] gameOverPanel이 설정되지 않았습니다.", this);

        // 필요하면 여기서 시간 멈추기 등 추가 가능
        // Time.timeScale = 0f;
    }
}
