using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Chase, Attack, Hit, Dead }
    public enum AttackPhase { Track, Lock, Active, Recovery }
    [SerializeField] float speed=2.2f, repathInterval=.4f, attackDistance=1.15f, trackTime=.4f, lockTime=.15f, activeTime=.15f, recoveryTime=.7f, hitTime=.2f;
    [SerializeField] int damage=1;
    public Transform target; public GridPathfinder pathfinder;
    public State CurrentState { get; private set; } = State.Chase;
    public AttackPhase Phase { get; private set; }
    public Vector2 Direction { get; private set; } = Vector2.down;
    public List<Vector2> Path { get; private set; } = new List<Vector2>();
    Rigidbody2D body; Health health; float until, nextPath; int waypoint; bool hitThisAttack;
    void Awake(){ body=GetComponent<Rigidbody2D>(); health=GetComponent<Health>(); health.Damaged += OnDamaged; }
    void OnDamaged(){ body.linearVelocity=Vector2.zero; CurrentState=health.IsDead ? State.Dead : State.Hit; until=Time.time+hitTime; if(health.IsDead) GetComponent<Collider2D>().enabled=false; }
    static Vector2 Cardinal(Vector2 v) => Mathf.Abs(v.x)>Mathf.Abs(v.y) ? new Vector2(Mathf.Sign(v.x),0) : new Vector2(0, v.y >= 0 ? 1 : -1);
    void Update()
    {
        if(CurrentState==State.Dead || target==null) return;
        Health player=target.GetComponent<Health>(); if(player==null || player.IsDead) { body.linearVelocity=Vector2.zero; return; }
        if(CurrentState==State.Hit) { if(Time.time>=until) CurrentState=State.Chase; return; }
        Vector2 delta=(Vector2)target.position-body.position;
        if(CurrentState==State.Chase)
        {
            if(delta.magnitude<=attackDistance) { CurrentState=State.Attack; Phase=AttackPhase.Track; until=Time.time+trackTime; body.linearVelocity=Vector2.zero; }
            else if(Time.time>=nextPath && pathfinder!=null) { Path=pathfinder.Find(body.position,target.position); waypoint=0; nextPath=Time.time+repathInterval; }
        }
        else if(CurrentState==State.Attack)
        {
            if(Phase==AttackPhase.Track) Direction=Cardinal(delta);
            if(Phase==AttackPhase.Active && !hitThisAttack)
            {
                Vector2 center=body.position+Direction; var colliders=Physics2D.OverlapBoxAll(center, Vector2.one, 0);
                foreach(var c in colliders) if(c.GetComponentInParent<Health>()==player) { player.Damage(damage); hitThisAttack=true; break; }
            }
            if(Time.time>=until)
            {
                switch(Phase){
                    case AttackPhase.Track: Phase=AttackPhase.Lock; until=Time.time+lockTime; break;
                    case AttackPhase.Lock: Phase=AttackPhase.Active; until=Time.time+activeTime; hitThisAttack=false; break;
                    case AttackPhase.Active: Phase=AttackPhase.Recovery; until=Time.time+recoveryTime; break;
                    default: CurrentState=State.Chase; nextPath=0; break;
                }
            }
        }
    }
    void FixedUpdate()
    {
        if(CurrentState!=State.Chase || target==null || target.GetComponent<Health>().IsDead) { body.linearVelocity=Vector2.zero; return; }
        while(waypoint<Path.Count && Vector2.Distance(body.position,Path[waypoint])<.12f) waypoint++;
        Vector2 move=waypoint<Path.Count ? Path[waypoint]-body.position : Vector2.zero;
        body.linearVelocity=move.sqrMagnitude>.001f ? move.normalized*Mathf.Min(speed, move.magnitude / Time.fixedDeltaTime) : Vector2.zero;
    }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    void OnDrawGizmos(){ if(Path!=null){Gizmos.color=Color.cyan; Vector3 p=transform.position; foreach(var point in Path){Gizmos.DrawLine(p,point);p=point;}} if(CurrentState==State.Attack){Gizmos.color=Color.red;Gizmos.DrawWireCube((Vector2)transform.position+Direction,Vector3.one);} }
#endif
}
