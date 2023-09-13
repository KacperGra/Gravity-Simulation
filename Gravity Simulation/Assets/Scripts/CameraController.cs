using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1;
    [SerializeField] private Camera _camera;

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal") * _moveSpeed * Time.deltaTime;
        float moveY = Input.GetAxis("Vertical") * _moveSpeed * Time.deltaTime;

        transform.position += new Vector3(moveX, moveY, 0);    

        if(Input.mouseScrollDelta.y > 0)
        {
            _camera.orthographicSize += 0.25f;
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            _camera.orthographicSize -= 0.25f;
        }
    }
}
