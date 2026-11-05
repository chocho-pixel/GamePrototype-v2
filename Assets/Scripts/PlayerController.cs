using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 4.5f, dashSpeed = 11f, dashDuration = .16f, dashCooldown = .8f;
    Rigidbody2D body; Health health; Vector2 input, dashDirection;
    float dashEnd, nextDash;
    public Vector2 LastDirection { get; private set; } = Vector2.down;
    public bool IsDashing => health != null && health.DashInvulnerable;
    public bool IsMoving => input.sqrMagnitude > .001f;
    void Awake() { body = GetComponent<Rigidbody2D>(); health = GetComponent<Health>(); }
    void Update()
    {
        if (health.IsDead) { input = Vector2.zero; health.DashInvulnerable = false; return; }
        var k = Keyboard.current;
        input = k == null ? Vector2.zero : new Vector2((k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0), (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0)).normalized;
        if (input.sqrMagnitude > 0) LastDirection = input;
        if (health.DashInvulnerable && Time.time >= dashEnd) health.DashInvulnerable = false;
        if (k != null && (k.leftShiftKey.wasPressedThisFrame || k.rightShiftKey.wasPressedThisFrame) && Time.time >= nextDash && !health.DashInvulnerable)
        {
            dashDirection = input.sqrMagnitude > 0 ? input : LastDirection;
            dashEnd = Time.time + dashDuration; nextDash = Time.time + dashCooldown;
            health.DashInvulnerable = true;
        }
    }
    void FixedUpdate()
    {
        if (health.IsDead) { body.linearVelocity = Vector2.zero; return; }
        body.linearVelocity = health.DashInvulnerable ? dashDirection * dashSpeed : input * moveSpeed;
    }
    void OnDisable() { if (health != null) health.DashInvulnerable = false; }
}
