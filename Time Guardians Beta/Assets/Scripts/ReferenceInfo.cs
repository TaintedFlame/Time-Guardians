using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceInfo : MonoBehaviour {

    public static ReferenceInfo referenceInfo;

    public int initialDelay = 60;
    public int restartDelay = 15;

    public RoleInfo[] rolesInfo;

    public StringArray[] roleListOrder;

    private void Awake()
    {
        referenceInfo = this;
    }
}
