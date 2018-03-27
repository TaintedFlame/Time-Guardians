using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] Transform firePosition;
    [SerializeField] Transform throwPosition;
    [SerializeField] Transform grabPosition;

    public float elapsedTime;
    bool elapseTime;
    bool canShoot;
    bool firing;
    bool actioning;

    bool couldHit;
    bool cantHit;

    public bool pressScopeToggle;
    public bool scoped;
    public float[] defaultScopes;
    float scopeElapsedTime;
    float scopeSpeed;
    float scopeDefaultSpeed = 3;
    float lastFov;

    float scopeTransitionValue = 0.17f;
    float scopeTransitionTime;
    
    Rigidbody grabbing;
    public float grabPower;
    public float grabRange;
    public float grabStopRange;
    public float grabSlowRange;
    public float grabAngularDrag;
    public float grabSlowPower;
    public float maxGrabDistance;

    [SyncVar] public bool grabMatch;
    bool grabMove;

    Player player;
    Rigidbody rigid;
    GunPositionSync gunPositionSync;

    public Animator holdMovement;

    public Camera[] cameras;
    public SimpleSmoothMouseLook cameraScript;

    public string currentItem;
    public GameObject[] firstItem;
    GameObject currentItemObject;
    public ItemInfo itemInfo;

    ShotEffectsManager currentEffect;
    public ShotEffectsManager[] itemEffects;

    public GameObject throwable;

    void Awake ()
    {
        player = GetComponent<Player>();
        rigid = GetComponent<Rigidbody>();
        gunPositionSync = GetComponent<GunPositionSync>();
    }
        
    void Start ()
    {
        if (isLocalPlayer)
        {
            canShoot = true;
            
            GetItem("empty");
        }
    }

    private void FixedUpdate ()
    {
        // Syncing Grab Position
        if (grabbing != null)
        {
            // grabbing.GetComponent<NetworkTransform>().;
        }

        if (!canShoot)
        {
            return;
        }
        // Continue If can shoot
        if (elapseTime)
        {
            elapsedTime += Time.deltaTime;
        }
        // Grab Movement
        if (grabMove && grabbing != null)
        {
            GrabMove();
        }

        FOV();
    }

    void Update ()
    {
        if (!canShoot)
        {
            return;
        }
        // Continue If can shoot

        // Can't hit if jester
        if (player.clientRole == "jester")
        {
            if (!cantHit)
            {
                cantHit = true;
            }
        }
        else
        {
            if (cantHit)
            {
                cantHit = false;
            }
        }

        // View Players
        RaycastHit hit;

        Ray ray = new Ray(firePosition.position, firePosition.forward);

        bool result = Physics.Raycast(ray, out hit, 500f);
        if (result)
        {
            // Detect if could hit
            if (hit.transform.root.transform.GetComponent<Player>() != null && hit.transform.root.transform.GetComponent<Player>() != player)
            {
                couldHit = true;
            }
            else if (itemInfo.delay > 0)
            {
                couldHit = true;
            }
            else
            {
                couldHit = false;
            }
            // Show View info if in range
            if ((hit.distance <= itemInfo.viewRange && !scoped) || (hit.distance <= itemInfo.scopeViewRange && scoped))
            {
                if (hit.transform.root.transform.GetComponent<Player>() != null && hit.transform.root.transform.GetComponent<Player>() != player)
                {
                    Player viewPlayer = hit.transform.root.transform.GetComponent<Player>();
                    PlayerHealth viewPlayerHealth = hit.transform.root.transform.GetComponent<PlayerHealth>();

                    if (PlayerCanvas.canvas.viewName != viewPlayer.playerName || PlayerCanvas.canvas.viewHealth != viewPlayerHealth.health)
                    {
                        PlayerCanvas.canvas.View(viewPlayer.playerName, viewPlayerHealth.health, viewPlayer.masked);
                    }
                }
                else
                {
                    PlayerCanvas.canvas.View("", -1, false);
                }
            }
            else
            {
                PlayerCanvas.canvas.View("", -1, false);
            }

            // Identifying Body
            if (Input.GetKeyDown("e") && hit.transform.root.transform.GetComponent<Body>() != null && hit.distance < 4f)
            {
                Body body = hit.transform.root.transform.GetComponent<Body>();
                string inspection = "";

                print(Player.player.clientRole);
                print(body.playerName);
                // Determining whether to view and/or post role
                if (Player.player.clientRole == "detective" || Player.player.clientRole == "traitor")
                {
                    for (int i = 0; i < NetworkGameInfo.networkGameInfo.playerIds.Count; i++)
                    {
                        if (body.playerName == NetworkGameInfo.networkGameInfo.playerIds[i])
                        {
                            inspection = NetworkGameInfo.networkGameInfo.roles[i];
                            print(inspection);
                            if (Player.player.clientRole == "detective")
                            {
                                CmdChangeBodyInspection(body.playerName, inspection);
                            }
                        }
                    }
                }

                // Sending to Canvas
                PlayerCanvas.canvas.ToggleBodyInspection(body.playerName, inspection);
            }
        }
        else
        {
            PlayerCanvas.canvas.View("", -1, false);
        }

        // Shooting Related stuff (left and right click)
        
        if (!Cursor.visible)
        {
            // Right-Click
            if (itemInfo.canGrab)
            {
                #region Grabbing
                
                /// 
                // Grabbing
                //

                // Animation

                if (Input.GetMouseButton(1))
                {
                    if (grabbing != null && currentItemObject.GetComponent<Animator>().GetInteger("Force") != 2)
                    {
                        currentItemObject.GetComponent<Animator>().SetInteger("Force", 2);
                    }
                    if (grabbing == null && currentItemObject.GetComponent<Animator>().GetInteger("Force") != 1)
                    {
                        currentItemObject.GetComponent<Animator>().SetInteger("Force", 1);
                    }
                }
                else if (currentItemObject.GetComponent<Animator>().GetInteger("Force") != 0)
                {
                    currentItemObject.GetComponent<Animator>().SetInteger("Force", 0);

                    ClientGrab(false);
                    CmdGrabObject(Vector3.zero, Vector3.zero, 0, false);
                }

                // Selecting Object

                if (grabbing != null && (grabMatch || (!grabMatch && isServer)))
                {
                    // Moving Grabbed Object

                    if (Input.GetMouseButton(1))
                    {
                        // Determining whether object is worthy of grabment
                        if (Vector3.Distance(grabbing.transform.position, grabPosition.transform.position) > maxGrabDistance)
                        {
                            ClientGrab(false);
                            CmdGrabObject(Vector3.zero, Vector3.zero, 0, false);

                            print("Too far away");
                        }
                        // Moving Object
                        if (grabbing != null)
                        {
                            grabMove = true;
                        }
                    }
                    else
                    {
                        grabMove = false;
                    }
                }
                else if (Input.GetMouseButton(1))
                {
                    // Finding potential grabby

                    RaycastHit grabHit;

                    Ray grabRay = new Ray(firePosition.position, firePosition.forward);
                    bool grabResult = Physics.Raycast(grabRay, out grabHit, grabRange);

                    if (grabResult && grabHit.transform.GetComponent<Rigidbody>() != null && grabHit.transform.GetComponent<Player>() == null)
                    {
                        grabbing = hit.transform.GetComponent<Rigidbody>();
                        ClientGrab(true);
                        CmdGrabObject(firePosition.position, firePosition.forward, grabRange, true);
                    }
                }

                #endregion
            }
            else
            {
                ///
                // Scoping
                //

                if (Input.GetMouseButtonDown(1) && scopeTransitionTime == 0 && itemInfo.canScope)
                {
                    pressScopeToggle = true;
                }

                if (itemInfo.canScope && pressScopeToggle && !firing && elapsedTime == 0 && !elapseTime && scopeTransitionTime == 0)
                {
                    scoped = !scoped;
                    scopeTransitionTime = scopeTransitionValue;
                    pressScopeToggle = false;

                    if (itemInfo.scopeImage)
                    {
                        PlayerCanvas.canvas.ScopeImage(scoped);
                        cameras[1].gameObject.SetActive(!scoped);
                    }

                    currentItemObject.GetComponent<Animator>().SetBool("Scope", scoped);
                    if (scoped)
                    {
                        currentItemObject.GetComponent<Animator>().SetTrigger("Scoping");

                        scopeSpeed = scopeDefaultSpeed;
                        cameraScript.sensitivity /= itemInfo.scopeZoomValue;

                        PlayerCanvas.canvas.EditReticuleSize(itemInfo.scopeReticuleSpacing, 0.1f);
                    }
                    else
                    {
                        currentItemObject.GetComponent<Animator>().SetTrigger("Unscoping");

                        scopeSpeed = -scopeDefaultSpeed;
                        cameraScript.sensitivity *= itemInfo.scopeZoomValue;
                    }
                }
            }
            // Left-Click or Left-Clicking
            if (((Input.GetMouseButtonDown(0) && !itemInfo.automatic) || (Input.GetMouseButton(0) && itemInfo.automatic)) && !elapseTime && scopeTransitionTime == 0)
            {
                if (itemInfo.canUseWhileSprinting || (!itemInfo.canUseWhileSprinting && (!Input.GetButton("Fire3") || (Input.GetButton("Fire3") && scoped))))
                {
                    if (NetworkGameInfo.networkGameInfo != null && (NetworkGameInfo.networkGameInfo.gameOn || (!NetworkGameInfo.networkGameInfo.gameOn && !couldHit)))
                    {
                        if (!cantHit || (cantHit && !couldHit))
                        {
                            if (itemInfo.delay != 0)
                            {
                                if (itemInfo.itemType == "weapon")
                                {
                                    currentItemObject.GetComponent<Animator>().SetTrigger("Fire");
                                }
                                if (itemInfo.itemType == "throwable")
                                {
                                    currentItemObject.GetComponent<Animator>().SetTrigger("Throw");
                                }
                            }

                            elapseTime = true;
                        }
                    }
                }
            }
        }

        // Closing Shop
        if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown("e")) && itemInfo.itemType == "shop" && itemInfo.shopVisual.activeInHierarchy)
        {
            Shop();
        }

        // Scope Transition Time management
        if (scopeTransitionTime > 0)
        {
            scopeTransitionTime -= Time.deltaTime;
        }
        if (scopeTransitionTime < 0)
        {
            scopeTransitionTime = 0;
        }

        // Fire on time (after delay)
        if (elapsedTime > itemInfo.delay && !firing)
        { 
            if (itemInfo.itemType == "weapon")
            {
                Fire();
            }
            if (itemInfo.itemType == "throwable")
            {
                Throw();
            }
            if (itemInfo.itemType == "shop")
            {
                Shop();
            }
            firing = true;
        }
        // If Done Firing / Action
        if (elapsedTime > itemInfo.cooldown && firing)
        {
            // If has action, declare so
            if (itemInfo.hasAction && !actioning)
            {
                actioning = true;

                // Action Sound
                Vector3 pos = new Vector3();
                Vector2 volume = new Vector2(0.7f, 0.8f);
                Vector2 pitch = new Vector2(1f, 1.02f);

                player.playerSounds.PlaySound(itemInfo.itemName+"Action", pos, volume, pitch, 20, true);
            }
            // If action time succeeded
            if (elapsedTime > itemInfo.cooldown + itemInfo.actionTime)
            {
                actioning = false;
            }
            // Finish Firing / Actioning
            if (!actioning)
            {
                firing = false;
                elapseTime = false;
                elapsedTime = 0;

                if (itemInfo.itemType == "throwable")
                {
                    GetItem("empty");
                }
            }
        }

        // Set Walk animation
        if (player.playerMovement.moveDirection != Vector3.zero && rigid.velocity.magnitude > 1)
        {
            holdMovement.SetBool("Walking", true);
        }
        else
        {
            holdMovement.SetBool("Walking", false);
        }
        // Set Sprint animation
        if (Input.GetButton("Fire3") && !scoped && rigid.velocity.magnitude > 1)
        {
            if (itemInfo.hasSprintAnimation)
            {
                currentItemObject.GetComponent<Animator>().SetBool("Sprinting", true);
            }
            else
            {
                holdMovement.SetBool("Sprinting", true);
            }
        }
        else
        {
            if (itemInfo.hasSprintAnimation)
            {
                currentItemObject.GetComponent<Animator>().SetBool("Sprinting", false);
            }
            else
            {
                holdMovement.SetBool("Sprinting", false);
            }
        }
    }

    void Fire ()
    {
        // Play Sound

        Vector3 pos = new Vector3();
        Vector2 volume = new Vector2(0.7f, 0.8f);
        Vector2 pitch = new Vector2(1f, 1.02f);

        player.playerSounds.PlaySound(itemInfo.itemName, pos, volume, pitch, itemInfo.maxHitRange, true);

        // If Sprinting, do sprint recoil
        if ((Input.GetButton("Fire3") && player == null) || (Input.GetButton("Fire3") && player != null && !player.playerShooting.scoped))
        {
            cameraScript.Recoil(itemInfo.sprintRecoil);
        }
        else if (Input.GetKey("w") || Input.GetKey("a") || Input.GetKey("s") || Input.GetKey("d")) // Walk Recoil
        {
            if (!scoped) // Walk Recoil
            {
                cameraScript.Recoil(itemInfo.walkRecoil);
            }
            else // Walk-Scope Recoil
            {
                cameraScript.Recoil(itemInfo.scopeWalkRecoil);
            }
        }
        else if (scoped) // Scope Recoil
        {
            cameraScript.Recoil(itemInfo.scopeRecoil);
        }
        else // Normal Recoil
        {
            cameraScript.Recoil(itemInfo.recoil);
        }

        if (itemInfo.delay == 0)
        {
            currentItemObject.GetComponent<Animator>().SetTrigger("Fire");
        }

        RaycastHit hit;

        Ray ray = new Ray(firePosition.position, firePosition.forward);
        Debug.DrawRay(ray.origin, ray.direction * 3f, Color.red, 1f);

        // Get Range
        float range = itemInfo.maxHitRange;
        if (scoped)
        {
            // Get Scope Range, if scoping
            range = itemInfo.scopeMaxHitRange;
        }

        bool result = Physics.Raycast(ray, out hit, range);

        if (result)
        {
            // Determine Damage based on potential headshots
            int finalDamage = itemInfo.damage;
            if (hit.point.y - hit.transform.position.y > 1.5f)
            {
                float floatDamage = finalDamage;
                floatDamage *= itemInfo.headShotMultiplier;

                finalDamage = (int)floatDamage;
            }

            // No Damage if cant hit
            if (cantHit)
            {
                finalDamage = 0;
            }
            
            // If hit other player
            PlayerHealth enemy = hit.transform.GetComponent<PlayerHealth>();
            if (enemy != null && finalDamage != 0)
            {
                // Getting Direction
                int direction = -1;
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

                player.CmdSendDamage(enemy.GetComponent<Player>().playerName, finalDamage, direction, player.playerName);
            }

            Rigidbody hitRigid = hit.transform.root.transform.GetComponent<Rigidbody>();
            if (hitRigid != null)
            {
                if (enemy == null || (enemy != null && itemInfo.canPushPlayers))
                {
                    CmdGiveVelocity(firePosition.position, firePosition.forward, itemInfo.maxHitRange, itemInfo.hitStrength);
                }
            }
        }

        ProcessShotEffects(result, hit.point);
        CmdProcessShotEffects(result, hit.point);
    }

    void Throw ()
    {
        cameraScript.Recoil(itemInfo.recoil);

        if (itemInfo.delay == 0)
        {
            currentItemObject.GetComponent<Animator>().SetTrigger("Throw");
        }

        CmdRequestThrow(throwPosition.position, throwPosition.forward * itemInfo.speed, itemInfo.itemName);
    }

    public void Shop ()
    {
        if (!itemInfo.shopVisual.activeInHierarchy)
        {
            
            PlayerCanvas.canvas.ToggleShop(itemInfo.shopType);
        }
        else
        {
            PlayerCanvas.canvas.ToggleShop("");
        }

        Player.CursorLocked(itemInfo.shopVisual.activeInHierarchy);
        itemInfo.shopVisual.SetActive(!itemInfo.shopVisual.activeInHierarchy);
    }

    void C4 (int value)
    {
        // Throw
        if (value == 0)
        {

        }
        // Plant
        if (value == 1)
        {

        }
    }

    void FOV ()
    {
        if (scopeSpeed != 0)
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                float scopeFov = -1;

                // If has Zoom
                if (itemInfo.scopeZoomValue > 0)
                {
                    scopeFov = defaultScopes[i] / itemInfo.scopeZoomValue;
                }
                else
                {
                    scopeFov = defaultScopes[i] / lastFov;

                }

                // Unscoping
                if (scopeElapsedTime > 0)
                {
                    if (scopeSpeed < 0)
                    {
                        scopeElapsedTime += scopeSpeed;
                    }
                }
                else if (scopeElapsedTime != 0)
                {
                    scopeElapsedTime = 0;
                    scopeSpeed = 0;
                }
                // Scoping
                if (scopeElapsedTime < 100)
                {
                    if (scopeSpeed > 0)
                    {
                        scopeElapsedTime += scopeSpeed;
                    }
                }
                else if (scopeElapsedTime != 100)
                {
                    scopeElapsedTime = 100;
                    scopeSpeed = 0;
                }
                // Set Scope value to fov (0 is zoomed out, 100 is zoomed in)
                cameras[i].fieldOfView = defaultScopes[i] - (((defaultScopes[i] - scopeFov) / 100) * scopeElapsedTime);
            }
        }
    }

    /// <summary>
    /// Grabbing
    /// </summary>

    void GrabMove()
    {
        Quaternion rot = grabbing.transform.rotation;
        grabbing.transform.LookAt(grabPosition);

        // When Close enough, stop.
        if (Vector3.Distance(grabbing.transform.position, grabPosition.transform.position) < grabStopRange)
        {
            grabbing.velocity *= 0;
        }
        else if (Vector3.Distance(grabbing.transform.position, grabPosition.transform.position) < grabSlowRange)
        {
            grabbing.velocity = (grabbing.transform.forward);

            grabbing.velocity *= (Vector3.Distance(grabbing.transform.position, grabPosition.transform.position)) * grabSlowPower;
            grabbing.angularVelocity *= grabAngularDrag;
        }
        else
        {
            grabbing.velocity = (grabbing.transform.forward);

            grabbing.velocity *= (Vector3.Distance(grabbing.transform.position, grabPosition.transform.position)) * grabPower;
        }

        grabbing.transform.rotation = rot;

        grabbing.velocity += rigid.velocity;

        CmdSendGrabTransform(grabbing.position, grabbing.rotation, grabbing.velocity, grabbing.angularVelocity);
    }

    void GrabObject (Rigidbody value)
    {
        // Find Object
        if (value != null && grabbing == null && !value.gameObject.GetComponent<NetworkIdentity>().localPlayerAuthority)
        {
            grabbing = value;
            grabbing.useGravity = false;

            grabMatch = true;
        }
        // Leave Object
        if (value == null && grabbing != null)
        {
            grabbing.useGravity = true;

            grabbing = null;

            grabMatch = false;
        }
    }

    [Command]
    void CmdGrabObject (Vector3 pos, Vector3 dir, float range, bool getAuthority)
    {
        if (player != Player.player)
        {
            // Server Finds Object
            if (getAuthority && grabbing == null)
            {
                // Finding potential grabby

                RaycastHit hit;

                Ray ray = new Ray(pos, dir);
                bool result = Physics.Raycast(ray, out hit, range);

                if (result && hit.transform.GetComponent<Rigidbody>() != null && hit.transform.GetComponent<Player>() == null)
                {
                    GrabObject(hit.transform.GetComponent<Rigidbody>());
                }
            }

            // Server Leaves Object
            if (!getAuthority && grabbing != null)
            {
                GrabObject(null);
            }
        }
    }

    void ClientGrab (bool value)
    {
        if (!isServer)
        {
            // Find
            if (value)
            {
                grabbing.GetComponent<NetworkTransform>().enabled = false;
            }
            // Leave
            if (!value && grabbing != null)
            {
                grabbing.GetComponent<NetworkTransform>().enabled = true;
            }
        }
        if (!value)
        {
            grabbing = null;
        }
    }

    [Command]
    void CmdSendGrabTransform (Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel)
    {
        if (grabbing != null)
        {
            grabbing.position = pos;
            grabbing.rotation = rot;
            grabbing.velocity = vel;
            grabbing.angularVelocity = angVel;
        }
    }

    /// <summary>
    /// Other
    /// </summary>


    [Command]
    void CmdRequestThrow (Vector3 pos, Vector3 vel, string itemName)
    {
        GameObject obj = Instantiate(throwable, pos, Quaternion.identity);
        obj.GetComponent<Rigidbody>().velocity = vel;
        NetworkServer.Spawn(obj);
        obj.GetComponent<Throwable>().item = itemName;
        obj.GetComponent<Throwable>().thrower = player.playerName;
    }

    [Command]
    void CmdGiveVelocity(Vector3 firePos, Vector3 forwardPos, float range, float hitStrength)
    {
        RaycastHit hit;

        Ray ray = new Ray(firePos, forwardPos);
        bool result = Physics.Raycast(ray, out hit, range);

        if (result && hit.transform.root.transform.GetComponent<Rigidbody>() != null)
        {
            Rigidbody hitRig = hit.transform.root.transform.GetComponent<Rigidbody>();
            float y = hitRig.velocity.y;
            hitRig.AddForceAtPosition(forwardPos * hitStrength * 50, hit.point);
            hitRig.velocity = new Vector3(hitRig.velocity.x, y + (2f * hitStrength), hitRig.velocity.z);
        }
    }

    [Command]
    void CmdChangeBodyInspection(string id, string inspection)
    {
        for (int i = 0; i < NetworkGameInfo.bodies.Count; i++)
        {
            if (NetworkGameInfo.bodies[i].GetComponent<Body>().playerName == id)
            {
                NetworkGameInfo.bodies[i].GetComponent<Body>().identified = inspection;
            }
        }
    }

    public void GetItem(string value)
    {
        if (currentItem != value)
        {
            // Reset Animation

            holdMovement.Rebind();

            // Close up previous item's loose-ends

            if (itemInfo != null)
            {
                if (itemInfo.itemType == "shop" && itemInfo.shopVisual.activeInHierarchy)
                {
                    Shop();
                }

                lastFov = itemInfo.scopeZoomValue;
                if (scoped)
                {
                    cameraScript.sensitivity *= itemInfo.scopeZoomValue;
                    scopeSpeed = -scopeDefaultSpeed;
                }
                scoped = false;
                pressScopeToggle = false;

                PlayerCanvas.canvas.ScopeImage(false);
                cameras[1].gameObject.SetActive(true);

                if (grabbing != null)
                {
                    ClientGrab(false);
                    CmdGrabObject(Vector3.zero, Vector3.zero, 0, false);
                }
            }

            // Change Item

            currentItem = value;
            gunPositionSync.CmdChangeItem(currentItem);

            if (currentItemObject != null)
            {
                currentItemObject.GetComponent<Animator>().Rebind();
            }               

            for (int i = 0; i < firstItem.Length; i++)
            {
                if (firstItem[i].name == currentItem)
                {
                    firstItem[i].SetActive(true);
                    currentItemObject = firstItem[i];
                    itemInfo = firstItem[i].GetComponent<ItemInfo>();
                    // Set Effect based on item index
                    currentEffect = itemEffects[i];
                }
                else
                {
                    firstItem[i].SetActive(false);
                }
            }

            // Set Action

            currentItemObject.GetComponent<Animator>().SetBool("Action", itemInfo.hasAction);

            // Set Reticule

            PlayerCanvas.canvas.EditReticuleSize(itemInfo.reticuleSpacing, 0);
        }
    }

    void ProcessShotEffects(bool playImpact, Vector3 point)
    {
        if (currentEffect != null)
        {
            currentEffect.PlayShotEffects();

            if (playImpact)
            {
                currentEffect.PlayImpactEffect(point);
            }
        }
    }

    [Command]
    void CmdProcessShotEffects (bool playImpact, Vector3 point)
    {
        RpcProcessShotEffects(playImpact, point);
    }

    [ClientRpc]
    void RpcProcessShotEffects(bool playImpact, Vector3 point)
    {
        if (currentEffect != null)
        {
            if (!isLocalPlayer)
            {
                if (playImpact)
                {
                    currentEffect.PlayImpactEffect(point);
                }

                currentEffect.PlayShotEffects();
            }
        }
    }

    public void SetEffect(ShotEffectsManager sem)
    {
        currentEffect = sem;
    }
}