using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    Rigidbody rb;
    public CapsuleCollider capsuleCollider;

    public float walkSpeed = 10;
    public float strafeSpeed = 4;
    public float jumpPower = 1;
    public float settableSprintMultiplier = 1.6f;
    float sprintMultiplier = 1;

    public float height = 1.8f;

    public float[] settableCrouchMultiplier;
    float[] crouchMultiplier = new float[] { 1, 1 };

    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    public LayerMask groundLayer;

    public Animator holdMovement;

    public CursorLockMode lockedMode;
    public CursorLockMode unlockedMode;

    //get the radius of the players capsule collider, and make it a tiny bit smaller than that
    float radius;
    //get the position (assuming its right at the bottom) and move it up by almost the whole radius
    Vector3 pos;
    //returns true if the sphere touches something on that layer
    bool isGrounded;

    // #Dom (all under)

    public Player player;

    public float itemDragMultiplier = 1;

    public Vector3 moveDirection;
    int directions;
    float y;
    float maximumMagnitude = 100;

    public bool isMoving;

    int glideTime;
    int waitingForGrounded;

    int touching;
    bool touchingAndJumped;

    public PhysicMaterial moveMaterial;
    public PhysicMaterial stopMaterial;

    float stepTime;

    public GameObject groundSphere;

    int fallTime;
    float lastDownwardVelocity;
    float lastHight;
    float hight;
    int fallAminTime;

    float reticuleSpeed = 0.17f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        fallTime = 0;
        lastDownwardVelocity = 0;
        lastHight = 0;
        hight = 0;
        touching = 0;
    }

    void FixedUpdate()
    {
        //Crouching
        /* if (Input.GetKey(KeyCode.LeftControl) && Grounded())
        {
            //Is crouching
            capsuleCollider.height = height / 2;
            //crouchMultiplier = settableCrouchMultiplier;
            for (int i = 0; i < settableCrouchMultiplier.Length; i++)
            {
                crouchMultiplier[i] = settableCrouchMultiplier[i];
            }
            Debug.Log(crouchMultiplier[1]);

        }
        else
        {
            //Not crouching
            capsuleCollider.height = height;
            for (int i = 0; i < crouchMultiplier.Length; i++)
            {
                crouchMultiplier[i] = 1;
            }
            
        } */

        // Sprinting
        if ((Input.GetButton("Fire3") && player == null) || (Input.GetButton("Fire3") && player != null && !player.playerShooting.scoped))
        {
            sprintMultiplier = settableSprintMultiplier;
        }
        else
        {
            sprintMultiplier = 1;
        }

        // Moving

        moveDirection = Vector3.zero;
        directions = 0;

        #region WASD

        if (Input.GetKey("w"))
        {
            moveDirection += transform.forward * walkSpeed;
            directions++;
        }
        if (Input.GetKey("s"))
        {
            moveDirection += -transform.forward * walkSpeed;
            directions++;
        }
        if (Input.GetKey("d"))
        {
            moveDirection += transform.right * strafeSpeed;
            directions++;
        }
        if (Input.GetKey("a"))
        {
            moveDirection += -transform.right * strafeSpeed;
            directions++;
        }
        #endregion

        // Save y velocity
        y = rb.velocity.y;

        // Slow down for diagonals
        moveDirection = new Vector3(moveDirection.x, 0, moveDirection.z);
        if (directions == 2)
        {
            moveDirection /= 1.41f;
        }

        // Add Multipliers
        if (directions >= 1)
        {
            // Drag
            if (player.playerShooting.itemInfo != null)
            {
                if (Input.GetButton("Fire3"))
                {
                    itemDragMultiplier = player.playerShooting.itemInfo.sprintDrag;
                }
                else if (player.playerShooting.scoped)
                {
                    itemDragMultiplier = player.playerShooting.itemInfo.scopeDrag;
                }
                else if (itemDragMultiplier != player.playerShooting.itemInfo.walkDrag)
                {
                    itemDragMultiplier = player.playerShooting.itemInfo.walkDrag;
                }
            }
            else // No Item Info
            {
                itemDragMultiplier = 1;
            }

            // Move player (set velocity)
            moveDirection *= sprintMultiplier * crouchMultiplier[0] / itemDragMultiplier;
        }

        // Was suddenly hit
        if (player != null)
        {
            if (player.velocities[0].magnitude - player.velocities[4].magnitude > 4)
            {
                glideTime = 30;
                waitingForGrounded = 3;
            }
        }

        // If falling
        if (fallTime > 150)
        {
            waitingForGrounded = 3;
        }

        // Set TouchingAndJumped
        touchingAndJumped = (fallAminTime > 10 && touching > 0);

        // Stop waiting for grounded
        if (waitingForGrounded > 0 && !holdMovement.GetBool("Falling"))
        {
            waitingForGrounded--;
        }

        // Set Walk or Run
        isMoving = (glideTime == 0 && waitingForGrounded == 0 && !touchingAndJumped);

        // Walk or Run
        if (isMoving)
        {
            rb.velocity = moveDirection;

            // Set correct physic material
            if (Input.GetKey("w") || Input.GetKey("a") || Input.GetKey("s") || Input.GetKey("d"))
            {
                if (GetComponent<CapsuleCollider>().material != moveMaterial)
                {
                    GetComponent<CapsuleCollider>().material = moveMaterial;
                }
            }
            else
            {
                if (GetComponent<CapsuleCollider>().material != stopMaterial)
                {
                    GetComponent<CapsuleCollider>().material = stopMaterial;
                }
            }
        }
        else // Glide
        {
            rb.velocity += moveDirection * Time.deltaTime;

            if (glideTime > 0)
            {
                glideTime--;
            }
        }

        // Exceeded max vel

        if (rb.velocity.magnitude > maximumMagnitude)
        {
            rb.velocity /= rb.velocity.magnitude / maximumMagnitude;
        }

        // Insuring Y value is untouched
        rb.velocity = new Vector3(rb.velocity.x, y, rb.velocity.z);

        // Falling

        if (player != null)
        {
            // If falling
            if (holdMovement.GetBool("Falling"))
            {
                if (stepTime >= 1)
                {
                    Vector3 pos = Vector3.zero;
                    Vector2 vol = new Vector2(0.4f, 0.5f);
                    Vector2 pit = new Vector2(0.7f, 0.9f);

                    player.playerSounds.PlaySound("step", pos, vol, pit, 10, true);
                }

                if (!player.playerShooting.scoped)
                {
                    PlayerCanvas.canvas.EditReticuleSize(player.playerShooting.itemInfo.sprintReticule, reticuleSpeed);
                }

                stepTime = -1;
            }
            // If not falling
            else
            {
                // If landed
                if (stepTime == -1)
                {
                    Vector3 pos = Vector3.zero;
                    Vector2 vol = new Vector2(0.4f, 0.5f);
                    Vector2 pit = new Vector2(0.95f, 1f);

                    player.playerSounds.PlaySound("step", pos, vol, pit, 10, true);

                    stepTime = 0;
                }

                // If walking
                if (isMoving && moveDirection != Vector3.zero)
                {
                    stepTime += 2;

                    // If sprinting
                    if (Input.GetButton("Fire3") && (!player.playerShooting.scoped || (player.playerShooting.scoped && player.playerShooting.itemInfo.canUseWhileSprinting)))
                    {
                        stepTime += Random.Range(1f, 1.5f);

                        if (!player.playerShooting.scoped)
                        {
                            PlayerCanvas.canvas.EditReticuleSize(player.playerShooting.itemInfo.sprintReticule, reticuleSpeed);
                        }
                    }
                    else // If not sprinting
                    {
                        if (!player.playerShooting.scoped)
                        {
                            PlayerCanvas.canvas.EditReticuleSize(player.playerShooting.itemInfo.walkReticule, reticuleSpeed);
                        }
                    }
                }
                // When stop walking
                if (moveDirection == Vector3.zero)
                {
                    if (stepTime >= 30)
                    {
                        Vector3 pos = Vector3.zero;
                        Vector2 vol = new Vector2(0.4f, 0.5f);
                        Vector2 pit = new Vector2(1f, 1f);

                        player.playerSounds.PlaySound("step", pos, vol, pit, 10, true);
                    }

                    if (!player.playerShooting.scoped)
                    {
                        PlayerCanvas.canvas.EditReticuleSize(player.playerShooting.itemInfo.reticuleSpacing, reticuleSpeed);
                    }

                    stepTime = 0;
                }
                // Full Step sound
                if (stepTime >= 100)
                {
                    Vector3 pos = Vector3.zero;
                    Vector2 vol = new Vector2(0.4f, 0.5f);
                    Vector2 pit = new Vector2(1f, 1.1f);

                    player.playerSounds.PlaySound("step", pos, vol, pit, 20, true);

                    stepTime -= 100;
                }
            }
        }

        //Jump Physics
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        //Debug.Log(Input.GetAxis("Jump"));
        if (Input.GetButton("Jump") && Grounded())
        {
            //rb.AddForce(Vector3.up * , ForceMode.Impulse);
            rb.velocity = new Vector3(rb.velocity.x, (jumpPower * crouchMultiplier[1]), rb.velocity.z);
            // Debug.Log("Jumping?");
            //gameObject.transform.position += new Vector3(0, jumpPower, 0);
        }

        // Falling
        if (holdMovement != null && groundSphere != null)
        {
            RaycastHit hit;
            Ray ray = new Ray(groundSphere.transform.position, -Vector3.up);
            bool result = Physics.Raycast(ray, out hit, 100);

            float slopeAngleDistance = 0.6f;
            if ((hit.distance < slopeAngleDistance && rb.velocity.y > 0) || hit.distance > slopeAngleDistance || hit.distance == 0)
            {
                if (!Grounded() && !holdMovement.GetBool("Falling") && Input.GetButton("Jump"))
                {
                    holdMovement.Rebind();
                }
                holdMovement.SetBool("Falling", !Grounded());
            }
            else
            {
                holdMovement.SetBool("Falling", false);
            }
        }
        // Fall Damage
        if (player != null)
        {
            FallDamage();
        }

        // Untouching
        if (touching > 0)
        {
            touching--;
        }
    }

    void FallDamage()
    {
        if (holdMovement.GetBool("Falling"))
        {
            fallAminTime++;
        }
        else
        {
            fallAminTime = 0;
        }

        // Fall Damage
        if (rb.velocity.y < -1)
        {
            lastDownwardVelocity = rb.velocity.y;
        }

        if (rb.velocity.y < -1)
        {
            if (fallTime == 0)
            {
                lastHight = transform.position.y;
            }

            fallTime++;
        }
        else
        {
            if (fallTime != 0)
            {
                int damage = 0;

                if (lastHight - hight > 5)
                {
                    damage = (int)(lastHight - hight - 5) / 2 * (int)-lastDownwardVelocity / 4;
                    print("Fell and took " + damage + " damage");
                }

                if (damage > 0)
                {
                    GetComponent<PlayerHealth>().RequestSelfHarm(damage, 0);
                }

                fallTime = 0;
            }
            lastHight = hight;
        }
        hight = transform.position.y;
    }

    bool Grounded()
    {
        // New Way
        if (groundSphere != null)
        {
            isGrounded = Physics.CheckSphere(groundSphere.transform.position, groundSphere.GetComponent<SphereCollider>().radius, groundLayer);
        }
        else // Old Way
        {
            //get the radius of the players capsule collider, and make it a tiny bit smaller than that
            radius = capsuleCollider.radius * 0.9f;
            //get the position and move it to the required position
            pos = transform.position - Vector3.up * (radius * 0.9f) * 3;
            //returns true if the sphere touches something on that layer
            isGrounded = Physics.CheckSphere(pos, radius, groundLayer);
        }
        return isGrounded;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pos, radius);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            GetComponent<Rigidbody>().AddExplosionForce(20, transform.position + new Vector3(0, 0, 0), 8, 3, ForceMode.VelocityChange);
        }

        // print((Mathf.Round(rb.velocity.magnitude * 100))/100);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 20f)
        {
            glideTime = 100;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        touching++;
    }
}
//BTW hey Dom :)
