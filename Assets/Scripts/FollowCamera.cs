using UnityEngine;
[RequireComponent(typeof(Camera))]
public class FollowCamera : MonoBehaviour
{
    public Transform target; [SerializeField] Vector2 roomSize=new Vector2(18,10); [SerializeField] float smoothTime=.18f;
    Vector3 velocity; Camera cam;
    void Awake(){cam=GetComponent<Camera>();}
    void LateUpdate()
    {
        if(target==null)return;
        float aspect=Mathf.Max(.01f,cam.aspect); cam.orthographicSize=Mathf.Min(4f,roomSize.y*.5f,roomSize.x*.5f/aspect);
        float halfH=cam.orthographicSize, halfW=halfH*aspect;
        Vector3 desired=new Vector3(Mathf.Clamp(target.position.x,-roomSize.x*.5f+halfW,roomSize.x*.5f-halfW),Mathf.Clamp(target.position.y,-roomSize.y*.5f+halfH,roomSize.y*.5f-halfH),transform.position.z);
        transform.position=Vector3.SmoothDamp(transform.position,desired,ref velocity,smoothTime);
    }
}
