using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;

public class Body : NetworkBehaviour
{
    [SyncVar(hook = "OnNameChange")] public string playerName;
    [SyncVar(hook = "OnIdChange")] public string identified;

    void OnNameChange(string value)
    {
        playerName = value;
    }
    void OnIdChange(string value)
    {
        playerName = value;
    }
}
