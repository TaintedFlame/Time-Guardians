using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Throwable : NetworkBehaviour
{
    [SyncVar] public string item;
    [SyncVar] public string thrower;

    [Header("Effects")]

    public GameObject explosionParticleEffect;

    public GameObject smokeParticleEffect;
    public GameObject smokedGrenadeObject;

    public GameObject flashParticleEffect;
    public GameObject flashedGrenadeObject;

    [Header("Essentials")]

    public GameObject[] items;
    GameObject selectedItem;

    Rigidbody rigid;

    ParticleSystem ps;

    int hitElapsedTime;

    void Start()
    {
        // 
        rigid = GetComponent<Rigidbody>();

        // Set Effects
        if (item == "fragGrenade")
        {
            StartCoroutine("Explode", 3);
        }
        if (item == "smokeGrenade")
        {
            StartCoroutine("Smoke", 3);
        }
        if (item == "flashGrenade")
        {
            StartCoroutine("Flash", 3);
        }

        // Enable Visuals
        foreach (GameObject g in items)
        {
            if (g.name == item)
            {
                selectedItem = g;
                selectedItem.SetActive(true);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hitElapsedTime > 5 && collision.transform.root.transform.gameObject.name != Player.player.transform.gameObject.name)
        {
            selectedItem.GetComponent<AudioSource>().Play();
        }

        hitElapsedTime = 0;
    }

    private void FixedUpdate()
    {
        hitElapsedTime++;
    }

    IEnumerator Explode(int time)
    {
        #region Explosion

        yield return new WaitForSeconds(time);

        explosionParticleEffect.SetActive(true);
        foreach (GameObject g in items)
        {
            g.SetActive(false);
        }
        rigid.isKinematic = true;
        transform.rotation = Quaternion.identity;

        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, 10f);

        foreach (Collider col in objectsInRange)
        {
            // bool doContinue = false;
            //
            Quaternion rot = transform.rotation;
            Vector3 pos = transform.position;

            RaycastHit hit;
            transform.LookAt(col.transform);
            transform.position += new Vector3(0, 0.25f, 0);
            Ray ray = new Ray(transform.position + new Vector3(0, 0, 0), transform.forward);
            bool result = Physics.Raycast(ray, out hit, 8);

            Rigidbody newRigid = null;
            if (result && hit.transform.root.transform == col.transform.root.transform)
            {
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

            transform.position = pos;
            transform.rotation = rot;
        }

        yield return new WaitForSeconds(1);
        Destroy(gameObject);

        #endregion
    }

    IEnumerator Smoke(int time)
    {
        yield return new WaitForSeconds(time);

        smokeParticleEffect.SetActive(true);
        selectedItem.SetActive(false);
        smokedGrenadeObject.SetActive(true);

        yield return new WaitForSeconds(10);

        smokeParticleEffect.transform.parent = null;

        yield return new WaitForSeconds(10);

        ps = smokeParticleEffect.GetComponent<ParticleSystem>();
        var em = ps.emission;
        em.SetBursts(new ParticleSystem.Burst[] { });

        yield return new WaitForSeconds(5);
        Destroy(smokeParticleEffect);
    }

    IEnumerator Flash(int time)
    {
        yield return new WaitForSeconds(time);

        // Check Flashbang Effect
        if (selectedItem.GetComponentInChildren<Renderer>().isVisible)
        {
            selectedItem.transform.LookAt(Player.player.playerShooting.cameras[0].transform.position);
            RaycastHit hit;

            Ray ray = new Ray(selectedItem.transform.position, selectedItem.transform.forward);
            bool result = Physics.Raycast(ray, out hit, 500f);

            if (result && hit.transform.root.transform.GetComponent<Rigidbody>() != null)
            {
                PlayerCanvas.canvas.Flashbang();
            }
        }

        // Other shit

        flashParticleEffect.SetActive(true);
        flashParticleEffect.transform.parent = null;
        selectedItem.SetActive(false);
        flashedGrenadeObject.SetActive(true);

        yield return new WaitForSeconds(1);

        Destroy(flashParticleEffect);
    }
}