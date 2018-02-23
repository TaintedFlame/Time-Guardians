using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
    [Header("Camera")]
    [Range(0.1f, 10.0f)]
    public float sensitivity;
    public float xMax;
    public float xMin;
    float currentRotation = 0.0f;
    float mouseInput;
    [Space(10)]
    public GameObject player;

    void Update()
    {
        mouseInput = Input.GetAxis("Mouse Y");
       /*
        Vector3 rot = transform.eulerAngles;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;
        rot.x = Mathf.Clamp(rot.x + mouseY, xMin, xMax);
        transform.eulerAngles = rot;
        */currentRotation += Input.GetAxisRaw("Mouse Y") * sensitivity;
        

        
        //transform.eulerAngles = new Vector3(transform.eulerAngles.x, player.transform.eulerAngles.y, player.transform.eulerAngles.z);
        /*Rotate the camera according to the player input
        gameObject.transform.eulerAngles += new Vector3(-Input.GetAxisRaw("Mouse Y") * sensitivity, 0, 0);
       // transform.eulerAngles = new Vector3(Mathf.Clamp(transform.eulerAngles.x, xMin, xMax) % 360, transform.eulerAngles.y, transform.eulerAngles.z);    
       
        //Rotate the player as the camera rotates
        player.transform.eulerAngles += new Vector3(player.transform.eulerAngles.x, Input.GetAxisRaw("Mouse X") * sensitivity, player.transform.eulerAngles.z);
        float angle = transform.rotation.x;
        /*if (angle < xMax)
        {
            Debug.Log("OVER THE MAX" + transform.localEulerAngles.x);
            transform.eulerAngles = new Vector3(xMax, 0, 0);
        }
        else
        {
            if (angle > xMin)
            {
                Debug.Log("OVER THE MIN" + transform.localEulerAngles.x);
                transform.eulerAngles = new Vector3(xMin, 0, 0);
            }
        }*/

        //transform.localEulerAngles = new Vector3 (Mathf.Clamp(transform.eulerAngles.x, xMin, xMax), transform.eulerAngles.y, transform.eulerAngles.z);
    }
    void FixedUpdate ()
    {
        currentRotation = Mathf.Clamp(currentRotation, xMin, xMax);
        Debug.Log(currentRotation);
        transform.eulerAngles = new Vector3(-currentRotation, transform.eulerAngles.y, transform.eulerAngles.z);
        //transform.rotation = Quaternion.identity * Quaternion.AngleAxis(currentRotation, transform.right);
        player.transform.eulerAngles += new Vector3(player.transform.eulerAngles.x, Input.GetAxisRaw("Mouse X") * sensitivity, player.transform.eulerAngles.z);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        /* Debug.Log(transform.eulerAngles.x);
         if ((transform.eulerAngles.x > xMin && transform.eulerAngles.x < 360)|| (transform.eulerAngles.x < xMax))
         {
             transform.Rotate(-mouseInput * sensitivity, 0, 0);
         }
         else if (transform.eulerAngles.x < xMin && transform.eulerAngles.x < 360)
         {
             gameObject.transform.eulerAngles = new Vector3(xMin + 0.01f, transform.eulerAngles.y, transform.eulerAngles.z);
             Debug.Log(transform.eulerAngles.x+" clamped: too high");
         } else if (transform.eulerAngles.x > xMax)
         {
             gameObject.transform.eulerAngles = new Vector3(xMax - 0.01f, transform.eulerAngles.y, transform.eulerAngles.z);
             Debug.Log(transform.eulerAngles.x + " clamped: too low");
         }
         player.transform.eulerAngles += new Vector3(0, Input.GetAxisRaw("Mouse X") * sensitivity,0);
         transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);*/
    }
}
