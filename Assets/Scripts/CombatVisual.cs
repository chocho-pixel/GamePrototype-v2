using UnityEngine;

public class CombatVisual : MonoBehaviour
{
    PlayerAttack attack; EnemyAI enemy; Health health; SpriteRenderer sprite;
    GameObject indicator; MeshFilter mesh; MeshRenderer meshRenderer;
    void Awake()
    {
        attack=GetComponent<PlayerAttack>(); enemy=GetComponent<EnemyAI>(); health=GetComponent<Health>(); sprite=GetComponent<SpriteRenderer>();
        indicator=new GameObject("Attack indicator"); indicator.transform.SetParent(transform, false); indicator.transform.localPosition=new Vector3(0,0,-.1f); indicator.transform.localScale=new Vector3(1f/transform.lossyScale.x,1f/transform.lossyScale.y,1f);
        mesh=indicator.AddComponent<MeshFilter>(); meshRenderer=indicator.AddComponent<MeshRenderer>();
        meshRenderer.material=new Material(Shader.Find("Sprites/Default")); meshRenderer.sortingOrder=3;
    }
    void Update()
    {
        if(sprite!=null) sprite.color=health.IsDead ? Color.gray : health.HitInvulnerable ? Color.white : enemy!=null ? new Color(.9f,.3f,.25f) : new Color(.3f,.7f,1f);
        bool visible=attack!=null ? attack.IsAttacking : enemy!=null && enemy.CurrentState==EnemyAI.State.Attack && enemy.Phase!=EnemyAI.AttackPhase.Recovery;
        indicator.SetActive(visible); if(!visible) return;
        if(attack!=null) DrawSector(attack.Direction,attack.Radius,attack.Angle,new Color(1,.8f,.2f,.55f));
        else DrawBox(enemy.Direction,enemy.Phase==EnemyAI.AttackPhase.Active ? new Color(1,.1f,.1f,.65f) : new Color(1,.4f,.2f,.3f));
    }
    void SetMesh(Vector3[] vertices,int[] triangles,Color color){ var m=mesh.mesh; m.Clear(); m.vertices=vertices; m.triangles=triangles; m.RecalculateBounds(); meshRenderer.material.color=color; }
    void DrawSector(Vector2 dir,float radius,float angle,Color color)
    {
        const int steps=24; var v=new Vector3[steps+2]; var t=new int[steps*3]; float center=Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg;
        for(int i=0;i<=steps;i++){float a=(center-angle*.5f+angle*i/steps)*Mathf.Deg2Rad;v[i+1]=new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0);if(i<steps){t[i*3]=0;t[i*3+1]=i+1;t[i*3+2]=i+2;}}
        SetMesh(v,t,color);
    }
    void DrawBox(Vector2 dir,Color color){Vector3 c=dir; SetMesh(new[]{c+new Vector3(-.5f,-.5f),c+new Vector3(.5f,-.5f),c+new Vector3(.5f,.5f),c+new Vector3(-.5f,.5f)},new[]{0,1,2,0,2,3},color);}
    void OnDestroy(){if(meshRenderer!=null) Destroy(meshRenderer.material); if(mesh!=null && mesh.mesh!=null) Destroy(mesh.mesh);}
}

