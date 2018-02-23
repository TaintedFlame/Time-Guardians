using UnityEngine;

public class ShotEffectsManager : MonoBehaviour
{
    public static ParticleSystem impactEffect;

    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] AudioSource gunAudio;
    [SerializeField] GameObject impactPrefab;
    LineRenderer line;

    // ParticleSystem impactEffect;

    //Create the impact effect for our shots
    void Start()
    {
        if (impactEffect == null)
        {
            impactEffect = Instantiate(impactPrefab).GetComponent<ParticleSystem>();
        }
        if (GetComponent<LineRenderer>() != null)
        {
            line = GetComponent<LineRenderer>();
        }
    }

    //Play muzzle flash and audio
    public void PlayShotEffects()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true);
            muzzleFlash.Play(true);
        }

        if (line != null)
        {
            line.enabled = true;

            Quaternion q = transform.rotation;
            transform.LookAt(impactEffect.gameObject.transform.position);

            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            line.SetPosition(0, ray.origin);
            if (Physics.Raycast(ray, out hit, 1000))
            {
                line.SetPosition(1, hit.point);
            }
            else
            {
                line.SetPosition(1, ray.GetPoint(1000));
            }
            Invoke("DisableLine", Time.deltaTime * 3);

            transform.rotation = q;
        }
    }

    void DisableLine()
    {
        line.enabled = false;
    }

    //Play impact effect and target position
    public void PlayImpactEffect(Vector3 impactPosition)
    {
        impactEffect.transform.position = impactPosition;
        impactEffect.Stop();
        impactEffect.Play();
    }
}