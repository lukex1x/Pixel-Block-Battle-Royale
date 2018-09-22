//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnector : NetworkBehaviour
{
    [SyncVar]
    public int ConnectedID = -1;
    [SyncVar]
    public int PlayerNetID = -1;
    public float SpawnwDelay = 1;
    
    void Start()
    {
        Debug.Log("Spawn Player connecter server " + isServer);
    }

    public override void OnStartLocalPlayer()
    {
        SendPlayerInfo();
        base.OnStartLocalPlayer();
    }


    void Update()
    {
        if (!isLocalPlayer || PlayerNetID == -1)
            return;

        UnitZ.gameManager.PlayerNetID = PlayerNetID;

        if (isLocalPlayer && isServer)
        {
            if (UnitZ.gameNetwork.IsDedicatedServer)
            {
                if (UnitZ.dedicatedManager != null)
                    UnitZ.dedicatedManager.CurrentPlayer = this;
                return;
            }
        }

        if (isLocalPlayer)
        {
            if (UnitZ.NetworkGameplay != null && UnitZ.NetworkGameplay.AutoSpawn)
            {
                if (UnitZ.playerManager.PlayingCharacter != null)
                {
                    CmdRequestAuthorizedCharacter(UnitZ.gameManager.UserID, PlayerNetID);
                    UnitZ.playerManager.PlayingCharacter.SendMessage("OnPlayerAuthorized", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }

    public void RequestSpawnWithTeam(byte team, int spawnpoint)
    {
        CmdRequestSpawnWithTeam(Vector3.zero, PlayerNetID, UnitZ.gameManager.UserID, UnitZ.gameManager.UserName, UnitZ.characterManager.CharacterIndex, UnitZ.gameManager.CharacterKey, team, spawnpoint);
        NetworkServer.Destroy(this.gameObject);
    }

    public void RequestSpawn(int spawnpoint)
    {
        CmdRequestSpawnPlayer(Vector3.zero, PlayerNetID, UnitZ.gameManager.UserID, UnitZ.gameManager.UserName, UnitZ.characterManager.CharacterIndex, UnitZ.gameManager.CharacterKey, spawnpoint);
        NetworkServer.Destroy(this.gameObject);
    }

    [Command(channel = 3)]
    public void CmdRequestAuthorizedCharacter(string userID, int netID)
    {
        Debug.Log("CmdRequestAuthorizedCharacter !!!!!");
        if (ServerLog.instance != null)
            ServerLog.instance.Log("Request Authorized Character " + userID + " : "+ netID);
        UnitZ.gameNetwork.RequestAuthorizedCharacter(userID, netID, this.connectionToClient);
        NetworkServer.Destroy(this.gameObject);
    }

    [Command(channel = 3)]
    public void CmdRequestSpawnPlayer(Vector3 position, int netID, string userid, string usename, int characterindex, string characterkey, int spawn)
    {
        UnitZ.gameNetwork.RequestSpawnPlayer(position, netID, userid, usename, characterindex, characterkey, 0, spawn, this.connectionToClient);
        NetworkServer.Destroy(this.gameObject);
    }

    [Command(channel = 3)]
    public void CmdRequestSpawnWithTeam(Vector3 position, int netID, string userid, string usename, int characterindex, string characterkey, byte team, int spawn)
    {
        UnitZ.gameNetwork.RequestSpawnPlayer(position, netID, userid, usename, characterindex, characterkey, team, spawn, this.connectionToClient);
        NetworkServer.Destroy(this.gameObject);
    }

    [Client]
    void SendPlayerInfo()
    {
        NetworkInstanceId netId = this.GetComponent<NetworkIdentity>().netId;
        CmdTellServerMyInfo((int)netId.Value, UnitZ.gameManager.UserName, UnitZ.gameManager.Team, UnitZ.GameKeyVersion);
    }

    [Command(channel = 3)]
    void CmdTellServerMyInfo(int id, string username, string team, string gamekey)
    {
        PlayerNetID = id;
        if (UnitZ.NetworkGameplay && UnitZ.NetworkGameplay.playersManager)
            UnitZ.NetworkGameplay.playersManager.UpdatePlayerInfo(id, 0, 0, username, team, gamekey, true);

        Debug.Log("NetID " + id + " has connect to the server");
    }
}
