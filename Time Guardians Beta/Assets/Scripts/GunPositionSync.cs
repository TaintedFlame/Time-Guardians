using UnityEngine;
using UnityEngine.Networking;

public class GunPositionSync : NetworkBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform handMount;
    [SerializeField] Transform gunPivot;
    [SerializeField] Transform rightHandHold;
    [SerializeField] Transform leftHandHold;
    [SerializeField] float threshold = 10f;
    [SerializeField] float smoothing = 5f;

    [SyncVar] float pitch;
    Vector3 lastOffset;
    float lastSyncedPitch;
    Animator anim;
    PlayerShooting playerShooting;

    public float maxPitch;

    [SyncVar] string currentItem;
    public GameObject[] thirdItems;

    public ShotEffectsManager[] itemEffects;

    void Start()
    {
        anim = GetComponent<Animator>();
        playerShooting = GetComponent<PlayerShooting>();

        if (isLocalPlayer)
        {
            // gunPivot.parent = cameraTransform;
            gunPivot.gameObject.SetActive(false);
        }
        else
        {
            lastOffset = handMount.position - transform.position;
        }
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            pitch = cameraTransform.localRotation.eulerAngles.x;
            if (lastSyncedPitch - pitch >= threshold || lastSyncedPitch - pitch <= -threshold)
            {
                CmdUpdatePitch(pitch);
                lastSyncedPitch = pitch;
            }
        }
        else
        {
            float newPitch = pitch;

            if (newPitch > 180)
            {
                newPitch -= 360;
            }
            newPitch = Mathf.Clamp(newPitch, -maxPitch, maxPitch);

            Vector3 currentOffset = handMount.position - transform.position;
            gunPivot.localPosition += currentOffset - lastOffset;
            lastOffset = currentOffset;

            float prevPitch = gunPivot.localEulerAngles.z;
            if (prevPitch > 180)
            {
                prevPitch -= 360;
            }

            gunPivot.localEulerAngles = new Vector3(0f, 0f, (Mathf.Lerp(prevPitch + 180, newPitch + 180, Time.deltaTime * smoothing)) - 180);

            // Checking Items
            for (int a = 0; a < thirdItems.Length; a++)
            {
                // Is my current item not active?
                if (thirdItems[a].name == currentItem && !thirdItems[a].activeInHierarchy)
                {
                    // Go through all items
                    for (int i = 0; i < thirdItems.Length; i++)
                    {
                        if (thirdItems[i].name == currentItem)
                        {
                            // Found Item
                            thirdItems[i].SetActive(true);

                            // Set Effect based on item index
                            playerShooting.SetEffect(itemEffects[i]);
                        }
                        else
                        {
                            // Disabling Others
                            thirdItems[i].SetActive(false);
                        }
                    }
                }
            }
        }
    }

    [Command]
    void CmdUpdatePitch(float newPitch)
    {
        pitch = newPitch;
    }

    void OnAnimatorIK()
    {
        if (!anim)
            return;

        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
        anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandHold.position);
        anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandHold.rotation);

        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
        anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandHold.position);
        anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandHold.rotation);
    }

    [Command]
    public void CmdChangeItem(string value)
    {
        currentItem = value;
    }
}