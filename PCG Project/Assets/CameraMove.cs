using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{

    [SerializeField] private int cameraMoveSpeed;

    [SerializeField] private int cameraZoomSpeed;

    [SerializeField] private Camera camera;
    
    //Aint happening unless I stop drawing everything with Debug.UI;

    void Update()
    {
        camera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * cameraZoomSpeed;
        if(camera.orthographicSize < 0)
        {
            camera.orthographicSize = 1;
        }

        if(Input.GetButton("Horizontal"))
        {
            Vector3 temp = camera.transform.position;
            temp.x += Input.GetAxis("Horizontal") * cameraMoveSpeed * Time.deltaTime;
            camera.transform.position = temp;
        }

        if(Input.GetButton("Vertical"))
        {
            Vector3 temp = camera.transform.position;
            temp.y += Input.GetAxis("Vertical") * cameraMoveSpeed * Time.deltaTime;
            camera.transform.position = temp;
        }
    }

    public void baseGenerated(int average)
    {
        cameraMoveSpeed = average;
        cameraZoomSpeed = average;
    }
}
