using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Shop/Item", order = 1)]
public class ShopItemInfo : ScriptableObject
{
    public string shopItemName;

    public string displayName;
    public int worth;
    public Sprite image;
    public string description;

    public bool mutliPurchasable;

    public string itemName;
}
