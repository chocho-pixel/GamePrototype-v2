using System.Collections.Generic;
using UnityEngine;

public class GridPathfinder : MonoBehaviour
{
    [SerializeField] Vector2 min = new Vector2(-9, -5), max = new Vector2(9, 5);
    [SerializeField] float cellSize = .5f, clearance = .38f;
    [SerializeField] LayerMask obstacles;
    public void SetBounds(Vector2 minimum, Vector2 maximum) { min = minimum; max = maximum; }
    int Width => Mathf.RoundToInt((max.x - min.x) / cellSize);
    int Height => Mathf.RoundToInt((max.y - min.y) / cellSize);
    Vector2 World(int x, int y) => min + new Vector2((x + .5f) * cellSize, (y + .5f) * cellSize);
    bool Free(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height && !Physics2D.OverlapCircle(World(x,y), clearance, obstacles);
    public List<Vector2> Find(Vector2 start, Vector2 goal)
    {
        int w = Width, h = Height, n = w*h;
        int sx = Mathf.Clamp((int)((start.x-min.x)/cellSize),0,w-1), sy = Mathf.Clamp((int)((start.y-min.y)/cellSize),0,h-1);
        int source = sy*w+sx; var open = new List<int>{source}; var closed = new bool[n]; var parent = new int[n]; var cost = new float[n];
        for (int i=0;i<n;i++) { parent[i]=-1; cost[i]=float.PositiveInfinity; } cost[source]=0;
        int best=source; float bestDistance=Vector2.SqrMagnitude(World(sx,sy)-goal);
        while(open.Count>0)
        {
            int selected=0; float score=float.PositiveInfinity;
            for(int i=0;i<open.Count;i++) { int id=open[i]; float f=cost[id]+Vector2.Distance(World(id%w,id/w),goal); if(f<score){score=f;selected=i;} }
            int current=open[selected]; open.RemoveAt(selected); if(closed[current]) continue; closed[current]=true;
            int x=current%w,y=current/w; float distance=Vector2.SqrMagnitude(World(x,y)-goal);
            if(distance<bestDistance){best=current;bestDistance=distance;}
            if(distance<cellSize*cellSize*.26f) {best=current; break;}
            for(int dy=-1;dy<=1;dy++) for(int dx=-1;dx<=1;dx++)
            {
                if(dx==0 && dy==0) continue; int nx=x+dx,ny=y+dy;
                if(!Free(nx,ny) || (dx!=0 && dy!=0 && (!Free(x+dx,y) || !Free(x,y+dy)))) continue;
                int next=ny*w+nx; if(closed[next]) continue;
                float tentative=cost[current]+(dx!=0 && dy!=0 ? 1.4142f : 1f)*cellSize;
                if(tentative>=cost[next]) continue; cost[next]=tentative; parent[next]=current; if(!open.Contains(next)) open.Add(next);
            }
        }
        var result=new List<Vector2>(); for(int p=best;p!=source && p>=0;p=parent[p]) result.Add(World(p%w,p/w)); result.Reverse(); return result;
    }
}
