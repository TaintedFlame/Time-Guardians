using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour {

	public static GameObject camera;
	public GameObject player;

	public float mouseSensitivity = 100.0f;
	public float clampAngle = 80.0f;

	private float rotY = 0.0f; // rotation around the up/y axis
	private float rotX = 0.0f; // rotation around the right/x axis

	private void Awake()
	{
		if (camera == null)
		{
			camera = gameObject;
		}
	}
	// Use this for initialization
	void Start () {
		
	}
	
	void Update () {
        //Recoil Test
        if (Input.GetMouseButton(0))
        {
            rotX -= 0.5f;
        }

        float mouseX = Input.GetAxis("Mouse X");
		float mouseY = -Input.GetAxis("Mouse Y");

		rotY += mouseX * mouseSensitivity * Time.deltaTime;
		rotX += mouseY * mouseSensitivity * Time.deltaTime;

		rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

		Quaternion localRotation = Quaternion.Euler(rotX, player.transform.eulerAngles.y, 0.0f);
		Quaternion localRotationPlayer = Quaternion.Euler(0, rotY, 0);
		transform.rotation = localRotation;	
		player.transform.rotation = localRotationPlayer;
	}

}
