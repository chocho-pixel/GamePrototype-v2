using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int maxHealth = 5;
    [SerializeField] float hitInvulnerability = .4f;
    public int Current { get; private set; }
    public int Maximum => maxHealth;
    public bool IsDead => Current <= 0;
    public bool DashInvulnerable { get; set; }
    public bool HitInvulnerable => Time.time < invulnerableUntil;
    float invulnerableUntil;
    public event System.Action Changed;
    public event System.Action Damaged;
    void Awake() { Current = maxHealth; }
    public bool Damage(int amount)
    {
        if (IsDead || DashInvulnerable || HitInvulnerable || amount <= 0) return false;
        Current = Mathf.Max(0, Current - amount);
        invulnerableUntil = Time.time + hitInvulnerability;
        Damaged?.Invoke(); Changed?.Invoke(); return true;
    }
}
