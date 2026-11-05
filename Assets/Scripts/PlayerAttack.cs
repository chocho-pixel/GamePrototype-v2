using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Health))]
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] float radius = 1.2f, angle = 100f, activeTime = .14f, cooldown = .35f;
    [SerializeField] int damage = 1;
    readonly HashSet<Health> hit = new HashSet<Health>();
    Health health; float end, nextAttack; Vector2 direction = Vector2.down;
    public bool IsAttacking => Time.time < end && !health.IsDead;
    public float Radius => radius; public float Angle => angle; public Vector2 Direction => direction;
    void Awake() { health = GetComponent<Health>(); }
    void Update()
    {
        if (health.IsDead) return;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextAttack && Camera.main != null)
        {
            Vector3 p = Mouse.current.position.ReadValue(); p.z = -Camera.main.transform.position.z;
            Vector2 aim = (Vector2)Camera.main.ScreenToWorldPoint(p) - (Vector2)transform.position;
            direction = aim.sqrMagnitude > .0001f ? aim.normalized : direction;
            end = Time.time + activeTime; nextAttack = Time.time + cooldown; hit.Clear();
        }
        if (!IsAttacking) return;
        foreach (var c in Physics2D.OverlapCircleAll(transform.position, radius))
        {
            EnemyAI enemy = c.GetComponentInParent<EnemyAI>();
            if (enemy == null) continue;
            Health target = enemy.GetComponent<Health>();
            if (hit.Contains(target)) continue;
            Vector2 delta = (Vector2)c.ClosestPoint(transform.position) - (Vector2)transform.position;
            if (delta.sqrMagnitude < .0001f) delta = (Vector2)enemy.transform.position - (Vector2)transform.position;
            if (Vector2.Angle(direction, delta) <= angle * .5f && target.Damage(damage)) hit.Add(target);
        }
    }
}
