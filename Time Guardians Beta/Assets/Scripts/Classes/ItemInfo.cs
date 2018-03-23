using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInfo : MonoBehaviour
{
    public string itemName;
    public string displayedName;

    //

    public string itemType;

    [Header("All")]

    public string itemSlot;
    public bool extraItemCompatible;
    public bool canDrop;
    public bool dropOnDeath;

    public float delay;
    public float cooldown;
    public bool automatic;

    public bool canUseWhileSprinting;
    public bool hasSprintAnimation;

    public float reticuleSpacing = -1;
    public float walkReticule = -1;
    public float sprintReticule = -1;

    public float recoil;
    public float walkRecoil;
    public float sprintRecoil;

    public float walkDrag = 1;
    public float sprintDrag = 1;

    public bool canGrab;

    [Header("Weapon")]

    public int damage;
    public float headShotMultiplier = 1;
    public float viewRange;
    public float maxHitRange;

    public string ammoType;
    public int maxclipSize;
    public int maxAmmo;

    public bool canScope;
    public float scopeZoomValue = 1;
    public float scopeViewRange;
    public float scopeMaxHitRange;
    public float scopeReticuleSpacing = -1;
    public float scopeDrag = 1;
    public float scopeRecoil;
    public float scopeWalkRecoil;
    public bool scopeImage;

    public float hitStrength;
    public bool canPushPlayers;

    public bool hasAction;
    public float actionTime;

    [Header("Throwable")]

    public float weight;
    public float speed;

    [Header("Shop")]

    public string shopType;
    public GameObject shopVisual;
}