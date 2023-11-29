using UnityEngine;
using System.Collections;

public class WorldSpacePanel : MonoBehaviour {
    public Camera CameraToFace;
    public bool FaceCamera = true;

    Vector3 offset;

    void Start()
    {
        offset = transform.localPosition;
    }

    void OnEnable() 
    {
        Camera.onPreRender += doProjection;
    }

    void OnDisable() 
    {
        Camera.onPreRender -= doProjection;
    }

    void doProjection(Camera cam) 
    {
        var targetCamera = (CameraToFace != null) ? CameraToFace : Camera.main;

        if (cam != targetCamera) 
        {
            return;
        }

        if (FaceCamera) {
            if (transform.parent != null)
                transform.position = transform.parent.position + offset;
            else
                transform.position = offset;
            transform.rotation = Quaternion.LookRotation(-cam.transform.forward, cam.transform.up);
        }
    }
}
