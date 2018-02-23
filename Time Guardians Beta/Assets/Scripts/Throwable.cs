using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Throwable : NetworkBehaviour
{
    [SyncVar] public string item;
    [SyncVar] public string thrower;

    public GameObject explosionParticleEffect;

    public GameObject[] items;

    void Start()
    {
        StartCoroutine("Explode", 3);
    }

    IEnumerator Explode(int time)
    {
        yield return new WaitForSeconds(time);

        explosionParticleEffect.SetActive(true);
        foreach (GameObject g in items)
        {
            g.SetActive(false);
        }

        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, 10f);

        foreach (Collider col in objectsInRange)
        {
            bool doContinue = false;
            //
            Quaternion rot = transform.rotation;

            RaycastHit hit;
            transform.LookAt(col.transform);
            Ray ray = new Ray(transform.position + new Vector3(0,0.5f,0), transform.forward);
            bool result = Physics.Raycast(ray, out hit, 8);

            Rigidbody newRigid = null;
            if (col.transform.root.GetComponent<Rigidbody>() != null && isServer)
            {
                newRigid = col.transform.root.GetComponent<Rigidbody>();
                newRigid.AddExplosionForce(5 + 2 * newRigid.mass, transform.position, 8, 0.5f + 0.25f * newRigid.mass, ForceMode.VelocityChange);
            }
            if (col.transform.root.GetComponent<Player>() != null && col.transform.root.GetComponent<Player>().isLocalPlayer)
            {
                int damage = 10;
                if (Vector3.Distance(transform.position, hit.point) < 1.5f) { damage = 100; }
                else if (Vector3.Distance(transform.position, hit.point) < 3f) { damage = 75; }
                else if (Vector3.Distance(transform.position, hit.point) < 6f) { damage = 40; }

                int direction = 0;
                
                if (result)
                {
                    Vector3 toTarget = (transform.position - hit.transform.root.transform.position).normalized;

                    if (Mathf.Abs(Vector3.Dot(toTarget, hit.transform.root.transform.forward)) > Mathf.Abs(Vector3.Dot(toTarget, hit.transform.root.transform.right)))
                    {
                        direction = 0;
                    }
                    else
                    {
                        if (Vector3.Dot(toTarget, hit.transform.root.transform.right) > 0)
                        {
                            direction = 1;
                        }
                        else
                        {
                            direction = 2;
                        }
                    }
                }
                Player.player.CmdSendDamage(col.transform.root.GetComponent<Player>().playerName, damage, direction, thrower);
            }
        }

        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}