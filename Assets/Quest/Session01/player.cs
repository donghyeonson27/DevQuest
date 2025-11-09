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

    private PlayerActions input;
    private Vector2 moveInput;
    private float yVelocity;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        input = new PlayerActions();
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
        Vector3 camForward = cameraTransform ? cameraTransform.forward : Vector3.forward;
        Vector3 camRight = cameraTransform ? cameraTransform.right : Vector3.right;
        camForward.y = 0f; camRight.y = 0f;
        camForward.Normalize(); camRight.Normalize();

        Vector3 move = camForward * moveInput.y + camRight * moveInput.x;
        if (move.sqrMagnitude > 1f) move.Normalize();

       //  Shift + W(전진)일 때만 move배율 적용
        Keyboard kb = Keyboard.current;
        bool shiftHeld = kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);
        bool movingForward = moveInput.y > 0.1f; // 미세 노이즈 방지용 문턱
               if (shiftHeld && movingForward)
        move *= sprintMultiplier; 

        if (controller.isGrounded && yVelocity < 0f) yVelocity = -2f; // 접지 클램프
        yVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed + Vector3.up * yVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
}
