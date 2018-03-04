using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopController : MonoBehaviour {

    public enum SortingTab {All, Weapons, Support, Misc };
    public SortingTab sortingTab;


    GameObject gamePlayer;
    public static ShopController Instance;
    public static ShopContents selectedShop;
    public ShopContents traitorShop;
    Player player;

    public GameObject shopIcon;
    public GameObject IconHolder;

    int lastIndex;
    float lastClick = 0;

    public Text crystalText;

    void Awake()
    {
        Instance = this;
    }
    
    void Update()
    {
        print(player.clientRole);
        if (player.clientRole == "traitor")
        {
            selectedShop = traitorShop;
            print(selectedShop.shopName);
        }
        crystalText.text = player.inventory.crystals.ToString();

    }

   
    void Start()
    {
        gamePlayer = GameObject.FindGameObjectWithTag("Player");
        player = gamePlayer.GetComponent<Player>();
    }
    void OnEnabled() {
        print("Enabled");
        
    }

    public void OnSelected(int index)
    {
        
        
        if ((Time.time - lastClick) < 0.3 && lastIndex == index)
        {
            //Purchasing
            if (player.inventory.crystals >= selectedShop.allItems[index].worth)
            {
                //Take Away the worth of the item to the player's crystals
                player.inventory.crystals -= selectedShop.allItems[index].worth;
                //Find Empty Slot
                for (int i = 4; i < 10; i++)
                {
                    if (player.inventory.items[i].itemName == null || player.inventory.items[i].itemName == "")
                    {
                        //Give the Item to the player
                        Debug.Log("Empty Slot");
                        player.inventory.NewItem(i, selectedShop.allItems[index].itemName);
                        i = 10;
                    }
                }

                Debug.Log(selectedShop.shopName);
                Debug.Log(player.inventory.crystals);
                Debug.Log(player.inventory.items[player.inventory.items.Length - 1]);

            }
        }
        Debug.Log((Time.time - lastClick) + "" + lastIndex);
        lastClick = Time.time;
        lastIndex = index;
    }
}
