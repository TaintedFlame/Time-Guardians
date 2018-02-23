using UnityEngine;
using UnityEngine.Networking;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] int maxHealth = 100;

    [SyncVar(hook = "OnHealthChanged")] public int health;

    Player player;
    Rigidbody rigid;


    void Awake()
    {
        player = GetComponent<Player>();
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.KeypadMultiply) && isLocalPlayer)
        {
            CmdInflictSelfHarm(1, 0);
        }
        if (transform.position.y < -100)
        {
            CmdInflictSelfHarm(1, 0);
        }
    }

    [ServerCallback]
    void OnEnable()
    {
        health = maxHealth;
    }

    public void RequestSelfHarm(int damage, int direction)
    {
        CmdInflictSelfHarm(damage, direction);
    }

    [Command]
    void CmdInflictSelfHarm(int damage, int direction)
    {
        if (player.alive)
        {
            TakeDamage(damage, direction, "");
        }
    }

    [Server]
    public bool TakeDamage(int damage, int direction, string playerHitter)
    {
        bool died = false;

        if (health <= 0)
            return died;

        health -= damage;
        died = health <= 0;
        if (health < 0)
        {
            health = 0;
        }

        RpcTakeDamage(died, damage, direction);

        // Check if special event on death
        if (died && NetworkGameInfo.networkGameInfo.gameOn)
        {
            player.CmdGotKilled(playerHitter);
        }

        return died;
    }

    [ClientRpc]
    void RpcTakeDamage(bool died, int damage, int direction)
    {
        if (died)
        {
            player.Die();
        }
        else if (isLocalPlayer)
        {
            if (damage <= 15)
            {
                PlayerCanvas.canvas.ReceiveDamage(0, direction);
            }
            else if (damage <= 50)
            {
                PlayerCanvas.canvas.ReceiveDamage(1, direction);
            }
            else
            {
                PlayerCanvas.canvas.ReceiveDamage(2, direction);
            }
        }
    }

    void OnHealthChanged(int value)
    {
        health = value;
        if (isLocalPlayer)
        {
            PlayerCanvas.canvas.SetHealth(value);
        }
    }
}