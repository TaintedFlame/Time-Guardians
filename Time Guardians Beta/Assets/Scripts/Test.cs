using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    public float startSpeed;
    public float deathSpeed;

    public float time;
    bool start;
    float elapsed;
    bool death;

    float speed;

    public GameObject[] visualObjects;

    private void Start()
    {
        speed = startSpeed;
    }

    private void Update()
    {
        if (Input.GetKeyDown("j"))
        {
            start = true;
            print("Pressed");
        }

        visualObjects[0].transform.eulerAngles = transform.eulerAngles + new Vector3(-90, 0, 180);
        visualObjects[1].transform.eulerAngles = transform.eulerAngles + new Vector3(0,0,90);
    }

    void FixedUpdate ()
    {
        if (elapsed < 1 && death)
        {
            elapsed -= Time.deltaTime / time;
        }
        else if (elapsed > 0 && !death)
        {
            elapsed += Time.deltaTime / time;
        }
        if (start)
        {
            if (!death)
            {
                elapsed += Time.deltaTime / time;
            }
            else
            {
                elapsed -= Time.deltaTime / time;
            }
            start = false;
        }
        if (elapsed >= 1 && !death)
        {
            elapsed = 1;
            death = true;
        }
        else if (elapsed <= 0 && death)
        {
            elapsed = 0;
            death = false;
        }

        speed = Mathf.Lerp(startSpeed, deathSpeed, elapsed) * Time.deltaTime;
        GetComponent<Rigidbody>().angularVelocity = new Vector3(0, speed, 0);
    }
}
