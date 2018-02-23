using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorControl : MonoBehaviour {

    public static GameObject camera;

    public static Transform teleport;

    public Transform defaultTransform;

    private float speed;
    [SerializeField] float defaultspeed = 10;
    [SerializeField] float accelerateSpeed = 40;

    public string forward;
    public string backward;
    public string left;
    public string right;
    public string up;
    public string down;

    public string accelerate;
    public string resetTransform;

    //

    private void Awake()
    {
        if (camera == null)
        {
            camera = gameObject;
        }
    }

    void Start ()
    {
        speed = defaultspeed;
        if (defaultTransform != null)
        {
            transform.position = defaultTransform.transform.position;
            transform.rotation = defaultTransform.transform.rotation;
        }
    }

    void Update()
    {
        if (Input.GetKey(forward))
        {
            transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(backward))
        {
            transform.Translate(-transform.forward * speed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(left))
        {
            transform.Translate(-transform.right * speed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(right))
        {
            transform.Translate(transform.right * speed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(up))
        {
            transform.Translate(transform.up * speed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(down))
        {
            transform.Translate(-transform.up * speed * Time.deltaTime, Space.World);
        }


        if (Input.GetKeyDown(accelerate))
        {
            speed = accelerateSpeed;
        }
        if (Input.GetKeyUp(accelerate))
        {
            speed = defaultspeed;
        }

        if (Input.GetKey(resetTransform))
        {
            if (defaultTransform != null)
            {
                transform.position = defaultTransform.transform.position;
                transform.rotation = defaultTransform.transform.rotation;
            }
        }

        //
    }

    private void FixedUpdate()
    {
        if (teleport != null)
        {
            transform.position = teleport.transform.position;
            // transform.rotation = teleport.transform.rotation;
            teleport = null;
        }
    }
}
