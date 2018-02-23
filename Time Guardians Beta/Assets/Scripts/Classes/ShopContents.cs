using UnityEngine;
using System.Collections;

[System.Serializable]
public class ShopContents
{
    public string shopName;

    public ShopItemInfo[] allItems;
    public ShopItemInfo[] weaponsItems;
    public ShopItemInfo[] supportItems;
    public ShopItemInfo[] miscItems;
}
