using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PickUp : NetworkBehaviour
{
    [Header("Main Components")]

    public static List<GameObject> pickUps = new List<GameObject>();

    [SyncVar] public int id;

    public static int disable = -1;
    public bool disabled = false;

    public int delay;

    GameObject touchingPlayer;

    PickUpInfo item;
    [SyncVar] public string syncedItemName;

    [Header("Spawning")]

    public string itemNameSpawn;
    public Vector3 startVelocity;

    public PickUpInfo[] items;

    private void Awake()
    {
        id = -1;

        pickUps.Add(gameObject);
    }

    [ServerCallback]
    void Start()
    {
        id = Random.Range(0, int.MaxValue);

        if (itemNameSpawn == null || itemNameSpawn == "")
        {
            List<PickUpInfo> spawnPickups = new List<PickUpInfo>();
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].itemSlotType == "pistol" || items[i].itemSlotType == "heavy" || items[i].itemSlotType == "grenade")
                {
                    spawnPickups.Add(items[i]);
                }
            }

            int rdm = Random.Range(0, spawnPickups.Count);
            item = spawnPickups[rdm];
        }
        else
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].itemName == itemNameSpawn)
                {
                    item = items[i];
                }
            }
        }
        
        syncedItemName = item.itemName;

        GetComponent<Rigidbody>().velocity = startVelocity;
    }

    private void FixedUpdate()
    {
        if (!isServer && (syncedItemName != null && syncedItemName != "") && (item == null || item.itemName == null || item.itemName == ""))
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].itemName == syncedItemName)
                {
                    item = items[i];
                }
            }
        }

        if (item != null && item.obj != null)
        {
            if (!item.obj.activeInHierarchy && !disabled)
            {
                item.obj.SetActive(true);
            }

            if (disable == id)
            {
                item.obj.SetActive(false);
                disabled = true;
                disable = -1;
            }
        }

        if (isServer && delay > 0)
        {
            delay--;
            if (delay == 0)
            {
                disabled = false;
            }
        }
    }

    [ServerCallback]
    void OnCollisionEnter(Collision col)
    {
        if (isServer && touchingPlayer == null && delay == 0)
        {
            if (col.transform.root.gameObject.CompareTag("Player"))
            {
                col.transform.root.GetComponent<Player>().RpcPickUp(id, item.itemName, item.itemSlotType);
                touchingPlayer = col.transform.root.gameObject;
            }
        }
    }
    [ServerCallback]
    void OnCollisionExit(Collision col)
    {
        if (isServer && touchingPlayer != null)
        {
            if (col.transform.root.gameObject == touchingPlayer)
            {
                Invoke("UnTouchPlayer", 1);
            }
        }
    }

    public void UnTouchPlayer ()
    {
        touchingPlayer = null;
    }
}