﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopObject : MonoBehaviour
{
    public Image itemImage;
    public Image onSpecialImage;
    public Image favouriteImage;

    public ShopItemInfo shopObject;

    private void Update()
    {
        itemImage.sprite = shopObject.image;
    }
}
