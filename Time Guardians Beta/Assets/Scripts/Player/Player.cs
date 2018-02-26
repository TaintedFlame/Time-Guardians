using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class ToggleEvent : UnityEvent<bool> { }

public class Player : NetworkBehaviour
{
    public static Player player;

    [SyncVar(hook = "OnNameChanged")] public string playerName;
    [SyncVar(hook = "OnColorChanged")] public Color playerColor;

    [SerializeField] ToggleEvent onToggleShared;
    [SerializeField] ToggleEvent onToggleLocal;
    [SerializeField] ToggleEvent onToggleRemote;

    public static List<Player> players = new List<Player>();
    
    NetworkAnimator anim;
    Rigidbody rigid;

    public bool alive;
    [SyncVar] string role;
    public string clientRole;
    [SyncVar(hook = "OnMaskedChanged")] public bool masked;

    public Transform viewTrasform;
    
    public Inventory inventory;
    public PlayerSounds playerSounds;
    public PlayerHealth playerHealth;
    public PlayerShooting playerShooting;
    public PlayerMovement playerMovement;

    public Text nameText;
    public GameObject maskedObject;

    public GameObject body;

    public Vector3[] velocities = new Vector3[20];

    void Start()
    {
        anim = GetComponent<NetworkAnimator>();
        rigid = GetComponent<Rigidbody>();
        inventory = GetComponent<Inventory>();

        EnablePlayer();
    }

    [ServerCallback]
    void OnEnable()
    {
        if (!players.Contains(this))
        {
            players.Add(this);
            NetworkGameInfo.players = players;
        }
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            if (playerName != "")
            {
                OnNameChanged(playerName);
                OnColorChanged(playerColor);
            }

            if (maskedObject.activeInHierarchy != masked)
            {
                maskedObject.SetActive(masked);
            }
        }
        if (isLocalPlayer)
        {
            if (player == null)
            {
                player = this;
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                PlayerCanvas.canvas.TabMenuEnabled(true);
            }
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                PlayerCanvas.canvas.TabMenuEnabled(false);
            }

            anim.animator.SetFloat("Speed", Input.GetAxis("Vertical"));
            anim.animator.SetFloat("Strafe", Input.GetAxis("Horizontal"));

            if (playerName != null && playerName != "" && NetworkGameInfo.networkGameInfo != null && !NetworkGameInfo.networkGameInfo.localised)
            {
                NetworkGameInfo.networkGameInfo.localised = true;
                CmdSendName(playerName);
                print("Alocating as " + playerName);
            }
        }
    }

    void FixedUpdate()
    {
        if (isServer)
        {
            Velocities();

            // Check for left players
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] == null)
                {
                    NetworkGameInfo.networkGameInfo.LeftMemeber();
                    players.RemoveAt(i);
                    i = players.Count;

                    print("Detected Player Left");
                }
            }
        }
        if (clientRole != role)
        {
            clientRole = role;

            // Get Role Item
            if (isLocalPlayer)
            {
                if (clientRole == "traitor")
                {
                    inventory.NewItem(4, "traitorShop");
                }
            }
        }
    }

    

    void Velocities ()
    {
        // Move back velocities
        for (int i = 1; i < velocities.Length; i++)
        {
            // Only do so if not already
            if (velocities[i] != velocities[i - 1])
            {
                velocities[i] = velocities[i - 1];
            }
        }
        // Set current velocity
        velocities[0] = rigid.velocity;
    }

    [Command]
    void CmdSendName(string value)
    {
        NetworkGameInfo.networkGameInfo.Ids(value);
    }

    [Command]
    public void CmdRole(string value)
    {
        role = value;
    }

    [Command]
    public void CmdSendDamage(string playerId, int damage, int direction, string hitter)
    {
        for(int i = 0; i < players.Count; i++)
        {
            if (players[i].playerName == playerId && players[i].alive)
            {
                players[i].playerHealth.TakeDamage(damage, direction, hitter);
            }
        }
    }

    [Command]
    public void CmdGotKilled(string killer)
    {
        if (role == "jester" && killer != playerName && (killer != null && killer != ""))
        {
            NetworkGameInfo.networkGameInfo.JesterWin(playerName);
        }
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerName == killer && (killer != null && killer != "") && NetworkGameInfo.networkGameInfo.gameOn)
            {
                if (NetworkGameInfo.networkGameInfo.roles[i] == "amnesiac")
                {
                    players[i].ChangeRole(role);
                    players[i].RpcChangeRoleVisual(role);
                }
            }
        }

        // Karma
    }

    [Server]
    void ChangeRole(string newRole)
    {
        // Add Role Count
        NetworkGameInfo.networkGameInfo.RoleCountByRole(newRole, 1, false);
        // Remove Role Count
        NetworkGameInfo.networkGameInfo.RoleCountByRole(role, -1, false);
        // Set Role in roles list
        NetworkGameInfo.networkGameInfo.SetRoleForPlayer(playerName, newRole);
    }

    [ClientRpc]
    void RpcChangeRoleVisual(string newRole)
    {
        // Find Role Info
        for (int i = 0; i < ReferenceInfo.referenceInfo.rolesInfo.Length; i++)
        {
            if (newRole == ReferenceInfo.referenceInfo.rolesInfo[i].name)
            {
                // Display new role
                PlayerCanvas.canvas.SetRoleVisual(ReferenceInfo.referenceInfo.rolesInfo[i].name, ReferenceInfo.referenceInfo.rolesInfo[i].displayedName, ReferenceInfo.referenceInfo.rolesInfo[i].roleColour, ReferenceInfo.referenceInfo.rolesInfo[i].textColour, ReferenceInfo.referenceInfo.rolesInfo[i].image);
            }
        }
    }

    [ClientRpc]
    public void RpcPickUp(int pickUpId, string itemName, string itemType)
    {
        inventory.CollectNewItem(pickUpId, itemName, itemType);
    }
    [Command]
    public void CmdRequestPickupDestroy(int pickUpId)
    {
        DestroyPickUp(pickUpId);
    }
    [Server]
    public void DestroyPickUp(int pickUpId)
    {
        for (int i = 0; i < PickUp.pickUps.Count; i++)
        {
            if (PickUp.pickUps[i].GetComponent<PickUp>().id == pickUpId)
            {
                NetworkServer.Destroy(PickUp.pickUps[i]);
                PickUp.pickUps.RemoveAt(i);
            }
        }
    }


    void DisablePlayer()
    {
        if (isLocalPlayer)
        {
            SpectatorControl.camera.SetActive(true);
            SpectatorControl.teleport = viewTrasform;

            if (masked)
            {
                CmdToggleMasked(false);
            }
            inventory.ResetInv();

            PlayerCanvas.canvas.View("", -1, false);
            PlayerCanvas.canvas.ScopeImage(false);
        }

        onToggleShared.Invoke(false);

        if (isLocalPlayer)
        {
            onToggleLocal.Invoke(false);
        }
        else
        {
            onToggleRemote.Invoke(false);
        }

        alive = false;
    }

    void EnablePlayer()
    {
        if (isLocalPlayer)
        {
            SpectatorControl.camera.SetActive(false); 

            CursorLocked(true);
        }

        if (isServer)
        {
            playerHealth.immuneTime = 100;
        }

        onToggleShared.Invoke(true);

        if (isLocalPlayer)
            onToggleLocal.Invoke(true);
        else
            onToggleRemote.Invoke(true);

        alive = true;
    }

    public void Die()
    {
        if (isLocalPlayer)
        {
            if (NetworkGameInfo.networkGameInfo.gameOn)
            {
                CmdSendLiveState(playerName, -1);
            }

            inventory.Die();
            CmdRequestBodyDrop(transform.position, transform.rotation, playerName);
        }
        if (playerControllerId == -1)
        {
            anim.SetTrigger("Died");
        }

        if (isLocalPlayer)
        {
            CursorLocked(false);
        }

        DisablePlayer();

        // Invoke ("Respawn", respawnTime);
    }

    public void ToggleMasked()
    {
        print("ToggledMasked");

        PlayMaskSound(!masked);

        CmdToggleMasked(true);
    }

    [Command]
    public void CmdToggleMasked(bool sound)
    {
        masked = !masked;

        if (sound)
        {
            RpcPlayMaskSound(masked);
        }
    }

    [ClientRpc]
    public void RpcPlayMaskSound(bool value)
    {
        if (!isLocalPlayer)
        {
            PlayMaskSound(value);
        }
    }

    void PlayMaskSound(bool value)
    {
        Vector3 pos = new Vector3();
        Vector2 volume = new Vector2(0.5f, 0.8f);
        Vector2 pitch = new Vector2(0.9f, 1.1f);

        if (!value)
        {
            playerSounds.PlaySound("zipDown", pos, volume, pitch, 20, false);
        }
        else
        {
            playerSounds.PlaySound("zipUp", pos, volume, pitch, 20, false);
        }
    }

    [Command]
    public void CmdRequestBodyDrop (Vector3 pos, Quaternion rot, string name)
    {
        BodyDrop(pos, rot, name);
    }

    [Server]
    public void BodyDrop (Vector3 pos, Quaternion rot, string name)
    {
        // Create Body
        GameObject obj = Instantiate(body, pos + new Vector3(0,0.1f,0), rot);

        // Get Last Highest Velocity

        Vector3 highestVelocity = Vector3.zero;
        for (int i = 0; i < velocities.Length; i++)
        {
            if (velocities[i].magnitude > highestVelocity.magnitude)
            {
                highestVelocity = velocities[i];
            }
        }
        obj.GetComponent<Rigidbody>().velocity = highestVelocity;

        // Set Body Name
        obj.gameObject.name = name;

        NetworkServer.Spawn(obj);

        obj.GetComponent<Body>().playerName = playerName;

        NetworkGameInfo.bodies.Add(obj);
    }

    [Command]
    void CmdSendLiveState(string name, int count)
    {
        NetworkGameInfo.networkGameInfo.UpdateRoleCount(name, count);
    }

    [ClientRpc]
    public void RpcRespawn()
    {
        if (isLocalPlayer || playerControllerId == -1)
            anim.SetTrigger("Restart");

        if (isLocalPlayer)
        {
            Transform spawn = NetworkManager.singleton.GetStartPosition();
            transform.position = spawn.position;
            transform.rotation = spawn.rotation;
        }

        GetComponent<Rigidbody>().velocity = Vector3.zero;

        EnablePlayer();
    }

    void OnNameChanged(string value)
    {
        playerName = value;
        gameObject.name = playerName;
        // nameText.text = playerName;
    }

    void OnColorChanged(Color value)
    {
        playerColor = value;
        // nameText.color = (playerColor);
    }

    void OnMaskedChanged(bool value)
    {
        masked = value;
    }

    [ClientRpc]
    public void RpcRoundOver(string roleName, string[] winners)
    {
        DisablePlayer();

        CursorLocked(false);

        if (isLocalPlayer)
        {
            PlayerCanvas.canvas.GameWinMenu(roleName, winners);
        }

        PlayerCanvas.canvas.TabMenuClear();

        Invoke("RpcRespawn", 4);
    }

    void BackToLobby()
    {
        CursorLocked(false);
        FindObjectOfType<NetworkLobbyManager>().SendReturnToLobby();
    }

    [ClientRpc]
    public void RpcRequestCallback(string callback, int time)
    {
        if (isLocalPlayer)
        {
            NetworkGameInfo.networkGameInfo.Callback(callback, time);
        }
    }

    public static void CursorLocked(bool value)
    {
        if (value)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}