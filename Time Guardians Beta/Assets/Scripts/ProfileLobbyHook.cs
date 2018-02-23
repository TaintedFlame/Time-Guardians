using UnityEngine;
using Prototype.NetworkLobby;
using UnityEngine.Networking;

public class ProfileLobbyHook : LobbyHook
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer,
        GameObject gamePlayer)
    {
        LobbyPlayer lPlayer = lobbyPlayer.GetComponent<LobbyPlayer>();
        Player gPlayer = gamePlayer.GetComponent<Player>();

        gPlayer.playerName = lPlayer.playerName;
        gPlayer.playerColor = lPlayer.playerColor;
    }
}