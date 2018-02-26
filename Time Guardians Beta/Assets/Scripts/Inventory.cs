using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Inventory : NetworkBehaviour
{
    [Header("Main Components")]

    public string[] slotOrder;

    public InvSlotInfo[] items;
    public ItemInfo[] itemInfos;

    int selected;

    [SerializeField] GameObject pickUp;
    [SerializeField] GameObject dropPosition;
    [SerializeField] float dropSpeed;

    Player player;

    [Header("Shop Components")]

    public int crystals;


    void Start ()
    {
        player = GetComponent<Player>();

        Invoke("ResetInv", 0.01f);
    }

    public void ResetInv ()
    {
        for (int i = 0; i < 5; i++)
        {
            NewItem(i, "empty");
        }
        SelectItem(0);
    }

    public void NewItem (int slot, string newItemName)
    {
        items[slot] = new InvSlotInfo();

        items[slot].itemName = newItemName;
        // Get Weapon Info
        for (int i = 0; i < itemInfos.Length; i++)
        {
            if (itemInfos[i].itemName == items[slot].itemName)
            {
                items[slot].clipAmmo = itemInfos[i].maxclipSize;
                items[slot].totalAmmo = itemInfos[i].maxclipSize;
            }
        }

        PlayerCanvas.canvas.NewItem(slot, newItemName);

        if (slot == selected)
        {
            SelectItem(slot);
        }
    }

    public void SelectItem (int value)
    {
        int clipSize = -1;
        string displayName = "";
        // Get Weapon Info
        for (int i = 0; i < itemInfos.Length; i++)
        {
            if (itemInfos[i].itemName == items[value].itemName)
            {
                clipSize = itemInfos[i].maxclipSize;
                displayName = itemInfos[i].displayedName;
            }
        }

        PlayerCanvas.canvas.NewSelection(value, displayName);

        PlayerCanvas.canvas.NewAmmo(items[value].clipAmmo, clipSize, items[value].totalAmmo);
        selected = value;

        player.playerShooting.GetItem(items[value].itemName);
    }

    void UpdateItemSlot ()
    {

    }
    
    public void CollectNewItem(int pickUpId, string itemName, string itemType)
    {
        int slot = -1;
        for (int i = 0; i < slotOrder.Length; i++)
        {
            if (slotOrder[i] == itemType)
            {
                slot = i;
            }
        }

        if (items[slot].itemName == "empty")
        {
            NewItem(slot, itemName);

            PickUp.disable = pickUpId;
            Player.player.CmdRequestPickupDestroy(pickUpId);
        }
    }

	void Update ()
    {
        if (Input.GetKeyDown("1"))
        {
            SelectItem(0);
        }
        if (Input.GetKeyDown("2"))
        {
            SelectItem(1);
        }
        if (Input.GetKeyDown("3"))
        {
            SelectItem(2);
        }
        if (Input.GetKeyDown("4"))
        {
            SelectItem(3);
        }
        if (Input.GetKeyDown("5"))
        {
            SelectItem(4);
        }

        if (Input.GetKeyDown("q") && items[selected] != null && items[selected].itemName != "empty" && items[selected].itemName != null && items[selected].itemName != "")
        {
            for (int i = 0; i < player.playerShooting.firstItem.Length; i++)
            {
                if (player.playerShooting.firstItem[i].GetComponent<ItemInfo>().itemName == items[selected].itemName && player.playerShooting.firstItem[i].GetComponent<ItemInfo>().canDrop)
                {
                    CmdDropItem(items[selected].itemName);
                    NewItem(selected, "empty");
                }
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            int newSelection = selected + 1;
            if (newSelection == 5) { newSelection = 0; }

            SelectItem(newSelection);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            int newSelection = selected - 1;
            if (newSelection == -1) { newSelection = 4; }

            SelectItem(newSelection);
        }
    }

    public void Die()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].itemName != "empty" && items[i].itemName != null && items[i].itemName != "")
            {
                for (int a = 0; a < player.playerShooting.firstItem.Length; a++)
                {
                    if (player.playerShooting.firstItem[a].GetComponent<ItemInfo>().itemName == items[i].itemName && player.playerShooting.firstItem[a].GetComponent<ItemInfo>().dropOnDeath)
                    {
                        CmdDropItem(items[i].itemName);
                        NewItem(i, "empty");
                    }
                }
            }
        }
    }
    
    [Command]
    public void CmdDropItem(string itemName)
    {
        GameObject newPickup = Instantiate(pickUp, dropPosition.transform.position, dropPosition.transform.rotation);
        NetworkServer.Spawn(newPickup);

        newPickup.GetComponent<PickUp>().itemNameSpawn = itemName;
        newPickup.GetComponent<PickUp>().delay = 100;
        newPickup.GetComponent<PickUp>().startVelocity = transform.forward * dropSpeed;
    }
}