using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    public float speed = 10.0f;
    public float sensitivity = 1.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Look around with Right Mouse Button
        if (Input.GetMouseButton(1) || true)
        {
            yaw += sensitivity * Input.GetAxis("Mouse X");
            pitch -= sensitivity * Input.GetAxis("Mouse Y");
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }

        // Movement
        float xMove = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float zMove = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float yMove = 0.0f;

        // Ascend with E, descend with Q
        if (Input.GetKey(KeyCode.Space))
        {
            yMove = speed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            yMove = -speed * Time.deltaTime;
        }

        Vector3 move = transform.right * xMove + transform.up * yMove + transform.forward * zMove;
        transform.position += move;
    }
}
