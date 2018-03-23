using UnityEngine;
using UnityEngine.Networking;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] int maxHealth = 100;

    [SyncVar(hook = "OnHealthChanged")] public int health;

    Player player;
    Rigidbody rigid;

    public int immuneTime;

    public float lastHitElapsed;


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

        if (immuneTime > 0)
        {
            immuneTime--;
        }

        lastHitElapsed += Time.deltaTime;
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
        TakeDamage(damage, direction, "");
    }

    [Server]
    public bool TakeDamage(int damage, int direction, string playerHitter)
    {
        bool died = false;

        if (immuneTime == 0 && player.alive)
        {
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

            // Play Hurt Sound

            if (lastHitElapsed > 0.3f)
            {
                Vector3 pos = new Vector3();
                Vector2 volume = new Vector2(0.1f, 0.2f);
                Vector2 pitch = new Vector2(0.8f, 1f);

                player.playerSounds.PlaySound("hurt", pos, volume, pitch, 20, false);
            }
        }

        //

        lastHitElapsed = 0;
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