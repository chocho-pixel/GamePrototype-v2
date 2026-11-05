using UnityEngine;
[RequireComponent(typeof(Camera))]
public class FollowCamera : MonoBehaviour
{
    public Transform target; [SerializeField] float smoothTime=.18f;
    Vector3 velocity; Camera cam;
    void Awake(){cam=GetComponent<Camera>(); cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=Color.black;}
    public void SnapToTarget()
    {
        if(target==null) return;
        UpdatePosition(true);
    }
    void LateUpdate()
    {
        if(target==null)return;
        UpdatePosition(false);
    }
    void UpdatePosition(bool snap)
    {
        Vector3 desired=new Vector3(target.position.x,target.position.y,transform.position.z);
        if(snap) velocity=Vector3.zero;
        transform.position=snap ? desired : Vector3.SmoothDamp(transform.position,desired,ref velocity,smoothTime);
    }
}
