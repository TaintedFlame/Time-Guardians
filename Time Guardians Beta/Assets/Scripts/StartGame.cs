using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class StartGame : NetworkBehaviour
{
    [SerializeField] GameObject prefab;

    public List<GameObject> pickUpSpawnpoints = new List<GameObject>();

    private void Awake()
    {
        /* Player.player = null;
        Player.players.Clear();
        PickUp.pickUps.Clear(); */
    }

    [ServerCallback]
    void Start()
    {
        Invoke("Spawn", 1f);
    }

    void Spawn()
    {
        GameObject obj = Instantiate(prefab, transform.position, transform.rotation);
        NetworkServer.Spawn(obj);

        // Syncing Local Scnene Info

        NetworkGameInfo.pickUpSpawnpoints = pickUpSpawnpoints;
    }
}