//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System.Collections;
using UnityEngine.Networking.Types;


public class GameNetwork : NetworkLobbyManager
{
    [HideInInspector]
    public bool IsDedicatedServer;
    [HideInInspector]
    public List<MatchInfoSnapshot> MatchListResponse;
    [HideInInspector]
    public MatchInfoSnapshot MatchSelected;
    public NetworkGameplayManager NetworkGamePlay;
    public string HostPassword = "";
    public string HostNameFillter = "";
    public int MinimumPlayerToForceStart = 0;
    public bool AutoStart = false;
    static short MsgKicked = MsgType.Highest + 1;
    static short MsgBegin = MsgType.Highest + 2;
    protected ulong currentMatchID;
    private bool isMatchMaking;
    private List<PlayerSpawnInfo> playerWaitingSpawn;
    private bool playerAdded = false;
    private float playerAddedtimeTmp;
    private void Update()
    {

        // in case if some player are not loaded in time so player won't spawn currectly on playing scene, 
        // we can spawn it manually when the player is activated.

        if (UnitZ.gameManager.PlayerNetID == -1 && ClientScene.ready && !IsOnLobby() && !IsDedicatedServer)
        {
            if (!playerAdded)
            {
                if (ClientScene.AddPlayer(0))
                {
                    playerAdded = true;
                    playerAddedtimeTmp = Time.time;
                    Debug.Log("manual spawned player");
                }
            }
            else
            {
                if(Time.time > playerAddedtimeTmp + 1)
                {
                    playerAdded = false;
                }
            }
        }
    }

    public bool IsOnLobby(string scenename = "")
    {
        if (scenename == "")
            scenename = SceneManager.GetActiveScene().name;
        return (scenename == this.lobbyScene);
    }

    bool isHasPlayerConnector(NetworkConnection conn)
    {
        PlayerConnector[] playerCons = (PlayerConnector[])GameObject.FindObjectsOfType(typeof(PlayerConnector));
        for (int i = 0; i < playerCons.Length; i++)
        {
            if (playerCons[i].ConnectedID == conn.connectionId)
                return true;
        }
        return false;
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        if (!IsOnLobby())
        {
            if (!isHasPlayerConnector(conn))
            {
                if (ServerLog.instance != null)
                    ServerLog.instance.Log("add player connector for " + conn.connectionId);

                Debug.Log("Game Network, add player " + conn.connectionId);
                var player = (GameObject)GameObject.Instantiate(playerPrefab, new Vector2(0, 0), Quaternion.identity);
                PlayerConnector connector = player.GetComponent<PlayerConnector>();
                if(connector != null)
                {
                    connector.ConnectedID = conn.connectionId;
                }
                NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
                return;
            }
        }
        base.OnServerAddPlayer(conn, playerControllerId);
    }

    public override void OnLobbyServerSceneChanged(string sceneName)
    {
        Debug.Log("Server scene loaded, spawn all players object *" + playerWaitingSpawn.Count);
        if (ServerLog.instance != null)
            ServerLog.instance.Log("spawn all players object count = " + playerWaitingSpawn.Count);

        if (UnitZ.gameManager.IsAutoSpawn)
        {
            foreach (PlayerSpawnInfo player in playerWaitingSpawn)
            {
                RequestSpawnPlayerObject(Vector3.zero, 0, player.UserID, player.PlayerName, player.CharacterIndex, player.CharacterKey, player.PlayerTeam, 0);
            }
        }
        base.OnLobbyServerSceneChanged(sceneName);
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        if (UnitZ.Hud != null)
            UnitZ.Hud.ProcessPopup.SetActive(false);

        ClientScene.Ready(conn);
        base.OnClientSceneChanged(conn);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (!IsOnLobby(sceneName))
        {
            if (NetworkServer.active)
            {
                if (NetworkGamePlay != null && UnitZ.NetworkGameplay == null)
                {
                    if (ServerLog.instance != null)
                        ServerLog.instance.Log("Setup gameplay :: " + NetworkGamePlay.gameObject.name);

                    GameObject networkobject = (GameObject)GameObject.Instantiate(NetworkGamePlay.gameObject, Vector3.zero, Quaternion.identity);
                    NetworkServer.Spawn(networkobject);
                }
            }
        }
        base.OnServerSceneChanged(sceneName);
    }

    public void FindInternetMatch()
    {
        MatchListResponse = null;
        singleton.StartMatchMaker();
        if (UnitZ.Hud != null)
            UnitZ.Hud.ProcessPopup.SetActive(true);
        singleton.matchMaker.ListMatches(0, 50, HostNameFillter, false, 0, 0, OnMatchList);
    }

    public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        if (UnitZ.Hud != null)
            UnitZ.Hud.ProcessPopup.SetActive(false);
        MatchListResponse = matchList;
        if (MatchListResponse != null && success)
        {
            if (matchList.Count != 0)
            {
                Debug.Log("Server lists ");
                for (int i = 0; i < MatchListResponse.Count; i++)
                {
                    Debug.Log("Game " + MatchListResponse[i].name + " " + MatchListResponse[i].currentSize + "/" + MatchListResponse[i].maxSize + " (Private)" + MatchListResponse[i].isPrivate);
                }
            }
            else
            {
                Debug.Log("No matches in requested room!");
            }
        }
        else
        {
            Debug.LogError("Couldn't connect to match maker");
        }
    }

    public void HostGame(int levelindex, bool online)
    {
        LevelPreset level = UnitZ.levelManager.GetLevel(levelindex);
        if (level == null)
            return;

        if (ServerLog.instance != null)
            ServerLog.instance.Log("Host Map :: " + level.LevelName);

        playScene = level.SceneName;
        NetworkGamePlay = level.GamePlayObject;
        if (NetworkGamePlay != null)
            UnitZ.gameManager.IsAutoSpawn = NetworkGamePlay.AutoSpawn;
        isMatchMaking = online;
        startingServer = false;
        if (online)
        {
            if (UnitZ.Hud != null)
                UnitZ.Hud.ProcessPopup.SetActive(true);
            StartMatchMaker();
            singleton.matchMaker.CreateMatch(matchName, (uint)maxConnections, true, HostPassword, "", "", 0, 0, OnMatchCreate);
        }
        else
        {
            singleton.StartHost();
        }
        Debug.Log("Host game Max" + maxConnections);
    }

    public void JoinGame()
    {
        if (MainMenuManager.menu != null)
            MainMenuManager.menu.SetPreviousPanel(MainMenuManager.menu.currentPanel);
        if (MatchSelected != null)
        {
            if (MatchSelected.isPrivate)
            {
                Debug.Log("Need password");
                if (Popup.Pop != null)
                {
                    Popup.Pop.AskingPassword("need password ", delegate
                    {
                        Popup.Pop.PopupPasswordObject.gameObject.SetActive(false);
                        Debug.Log("Access with Password " + Popup.Pop.PopupPasswordObject.Password);
                        singleton.matchMaker.JoinMatch(MatchSelected.networkId, Popup.Pop.PopupPasswordObject.Password, "", "", 0, 0, OnMatchJoined);
                    },
                        delegate
                        {
                            LeaveMatch();
                            Popup.Pop.PopupPasswordObject.gameObject.SetActive(false);
                        });
                }
            }
            else
            {
                if (UnitZ.Hud != null)
                    UnitZ.Hud.ProcessPopup.SetActive(true);
                singleton.matchMaker.JoinMatch(MatchSelected.networkId, "", "", "", 0, 0, OnMatchJoined);

            }
            Debug.Log("Connecting to matchMaker");
        }
        else
        {
            singleton.networkAddress = networkAddress;
            singleton.networkPort = networkPort;
            singleton.StartClient();
            Debug.Log("Connecting to IP : " + networkAddress);
        }
    }

    public void MatchSelect(MatchInfoSnapshot match)
    {
        currentMatchID = (System.UInt64)match.networkId;
        MatchSelected = match;
    }

    public void SetListed(bool isListed)
    {
        if (singleton == null || singleton.matchMaker == null)
            return;

        singleton.matchMaker.SetMatchAttributes(singleton.matchInfo.networkId, isListed, 0, (success, extendedInfo) =>
        {
            if (!success) Debug.Log("Unable to update match listed: " + extendedInfo);
        });
    }

    public override void OnDropConnection(bool success, string extendedInfo)
    {
        Debug.Log("Game is procesed!");
        base.OnDropConnection(success, extendedInfo);
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (UnitZ.Hud != null)
            UnitZ.Hud.ProcessPopup.SetActive(false);
        Debug.Log("Connecting success " + success + " " + extendedInfo + " " + matchInfo);
        if (success)
        {
            singleton.StartClient(matchInfo);
            Debug.Log("Connected!");
        }
        else
        {
            if (Popup.Pop != null)
            {
                Popup.Pop.Asking("Unable to connect", null, delegate
                {

                });
            }
        }
    }

    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        if (IsOnLobby())
        {
            if (MainMenuManager.menu != null)
                MainMenuManager.menu.OpenPanelByName("Home");
        }
    }

    public void SetServerInfo(string status, string host)
    {
        Debug.Log("Received server info " + status + " " + host);
    }

    public override void OnStartHost()
    {
        base.OnStartHost();
        Debug.Log("On Start Host");
        if (MainMenuManager.menu != null)
            MainMenuManager.menu.OpenPanelByName("Lobby");
        SetServerInfo("Hosting", networkAddress);
    }

    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (UnitZ.Hud != null)
            UnitZ.Hud.ProcessPopup.SetActive(false);
        Debug.Log("On Match created!! " + success);
        base.OnMatchCreate(success, extendedInfo, matchInfo);
        currentMatchID = (System.UInt64)matchInfo.networkId;
    }

    public override void OnDestroyMatch(bool success, string extendedInfo)
    {
        Debug.Log("On Destroy match");
        base.OnDestroyMatch(success, extendedInfo);
        StopMatchMaker();
        StopHost();
    }

    public void RemovePlayer(PlayerLobby player)
    {
        player.RemovePlayer();
    }

    public void DestroyMatch()
    {
        if (isMatchMaking)
        {
            matchMaker.DestroyMatch((NetworkID)currentMatchID, 0, OnDestroyMatch);
        }
        else
        {
            StopHost();
        }
        if (MainMenuManager.menu != null)
            MainMenuManager.menu.OpenPreviousPanel();
    }

    public void LeaveMatch()
    {
        MatchSelected = null;
        StopClient();
        if (isMatchMaking)
        {
            StopMatchMaker();
        }
        if (MainMenuManager.menu != null)
            MainMenuManager.menu.OpenPreviousPanel();
    }

    // Server Callback
    public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log("On Lobby server create lobby player");
        GameObject obj = Instantiate(lobbyPlayerPrefab.gameObject) as GameObject;
        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            PlayerLobby p = lobbySlots[i] as PlayerLobby;

            if (p != null)
            {
                p.RpcUpdatePlayerLobby();
            }
        }
        if (ServerLog.instance != null)
            ServerLog.instance.Log("Player lobby:" + conn.address + " created");
        return obj;
    }

    public override void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log("On Lobby server remove lobby player");
        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            PlayerLobby p = lobbySlots[i] as PlayerLobby;

            if (p != null)
            {
                p.RpcUpdatePlayerLobby();
            }
        }
        if (ServerLog.instance != null)
            ServerLog.instance.Log("Player lobby : ID " + conn.connectionId + " removed");
    }

    public override void OnLobbyServerDisconnect(NetworkConnection conn)
    {
        Debug.Log("On lobby disconnect");
        if (ServerLog.instance != null)
            ServerLog.instance.Log("Player lobby:" + conn.address + " disconnected");
        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            PlayerLobby p = lobbySlots[i] as PlayerLobby;

            if (p != null)
            {
                p.RpcUpdatePlayerLobby();
            }
        }
    }

    public override void OnLobbyServerPlayersReady()
    {
        if (AutoStart)
            StartServerGame();
    }

    public int GetLobbyPlayerCount()
    {
        int playercount = 0;
        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            if (lobbySlots[i] != null)
            {
                playercount += 1;
            }
        }
        return playercount;
    }

    public void AssignPlayersToSpawn(PlayerLobby[] players)
    {
        playerWaitingSpawn = new List<PlayerSpawnInfo>();
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].PlayerID != "")
            {
                PlayerSpawnInfo player = new PlayerSpawnInfo();
                player.CharacterIndex = players[i].CharacterIndex;
                player.PlayerName = players[i].playerName;
                player.PlayerTeam = players[i].playerTeam;
                player.UserID = players[i].PlayerID;
                player.CharacterKey = "";
                playerWaitingSpawn.Add(player);
            }
        }
    }

    public bool StartServerGame()
    {
        bool isallready = true;
        int playernum = 0;
        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            if (lobbySlots[i] != null)
            {
                playernum += 1;
                if (!lobbySlots[i].readyToBegin)
                {
                    Debug.Log("Can't Start!, Someone is not ready");
                    isallready = false;
                }
            }
        }
        if (playernum >= MinimumPlayerToForceStart && MinimumPlayerToForceStart > 0)
        {
            isallready = true;
        }
        if (isallready)
        {
            if (!startingServer)
            {
                StartCoroutine(ServerCountdownCoroutine());
                if (UnitZ.Hud != null)
                    UnitZ.Hud.ProcessPopup.SetActive(true);
            }
        }
        return isallready;
    }

    bool startingServer;
    public IEnumerator ServerCountdownCoroutine()
    {
        startingServer = true;
        float remainingTime = 1;
        int floorTime = Mathf.FloorToInt(remainingTime);

        while (remainingTime > 0)
        {
            yield return null;
            remainingTime -= Time.deltaTime;
            int newFloorTime = Mathf.FloorToInt(remainingTime);

            if (newFloorTime != floorTime)
            {
                //to avoid flooding the network of message, we only send a notice to client when the number of plain seconds change.
                floorTime = newFloorTime;
                if (ServerLog.instance != null)
                    ServerLog.instance.Log("Starting match.." + floorTime);

                for (int i = 0; i < lobbySlots.Length; ++i)
                {
                    if (lobbySlots[i] != null)
                    {
                        //there is maxPlayer slots, so some could be == null, need to test it before accessing!
                        (lobbySlots[i] as PlayerLobby).RpcUpdateCountdown(floorTime);
                    }
                }
            }
        }
        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            if (lobbySlots[i] != null)
            {
                (lobbySlots[i] as PlayerLobby).RpcUpdateCountdown(0);
                Begin(lobbySlots[i].connectionToClient);
            }
        }
        startingServer = false;
        if (UnitZ.Hud != null)
            UnitZ.Hud.ProcessPopup.SetActive(false);

        ServerChangeScene(playScene);
        SetListed(false);
    }

    // Client Callback
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        conn.RegisterHandler(MsgKicked, KickedMessageHandler);
        conn.RegisterHandler(MsgBegin, BeginMessageHandler);
        if (!NetworkServer.active)
        {
            if (MainMenuManager.menu != null)
                MainMenuManager.menu.OpenPanelByName("Lobby");
            SetServerInfo("Client", networkAddress);
        }
        UnitZ.gameManager.PlayerNetID = -1;
        playerAdded = false;

        if (ServerLog.instance != null)
            ServerLog.instance.Log(" Client:" + conn.address + " connected");

    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        //if you are client and disconnected
        Debug.Log("on client disconnected");
        if (MainMenuManager.menu != null)
            MainMenuManager.menu.OpenPanelByName("Home");
        if (UnitZ.Hud != null)
            UnitZ.Hud.ProcessPopup.SetActive(false);

        if (ServerLog.instance != null)
            ServerLog.instance.Log(conn.address + " disconnected");

        UnitZ.gameManager.PlayerNetID = -1;
        playerAdded = false;
        base.OnClientDisconnect(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (ServerLog.instance != null)
            ServerLog.instance.Log(" Client:" + conn.address + " disconnected");
        //if you are host and when someone disconnected.
        Debug.Log("on server disconnected");
        base.OnServerDisconnect(conn);
    }

    public override void OnStopHost()
    {
        if (UnitZ.Hud != null)
            UnitZ.Hud.ProcessPopup.SetActive(false);

        if (UnitZ.dedicatedManager != null)
            UnitZ.dedicatedManager.OnDisconnect();

        base.OnStopHost();
    }

    public override void OnStopServer()
    {
        if (UnitZ.Hud != null)
            UnitZ.Hud.ProcessPopup.SetActive(false);

        if (UnitZ.dedicatedManager != null)
            UnitZ.dedicatedManager.OnDisconnect();
        base.OnStopServer();
    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        Debug.Log("Cient error : " + errorCode.ToString());
    }

    public void OnPlayersNumberModified(int count)
    {
        int localPlayerCount = 0;
        foreach (PlayerController p in ClientScene.localPlayers)
            localPlayerCount += (p == null || p.playerControllerId == -1) ? 0 : 1;
    }

    class KickMsg : MessageBase { }
    public void KickPlayer(NetworkConnection conn)
    {
        conn.Send(MsgKicked, new KickMsg());
    }
    class BeginkMsg : MessageBase { }
    public void Begin(NetworkConnection conn)
    {
        conn.Send(MsgBegin, new BeginkMsg());
    }

    public void KickedMessageHandler(NetworkMessage netMsg)
    {
        Debug.Log("Kicked by Server");
        netMsg.conn.Disconnect();
    }

    public void BeginMessageHandler(NetworkMessage netMsg)
    {
        Debug.Log("Game is begin..");
        this.SendMessage("OnGameIsBegin", SendMessageOptions.DontRequireReceiver);
    }

    public void Disconnect()
    {
        MatchSelected = null;
        if (NetworkServer.connections.Count > 0)
        {
            singleton.StopHost();
        }
        else
        {
            singleton.StopClient();
        }
    }

    public void RequestSpawnPlayerObject(Vector3 position, int connectid, string userid, string usename, int characterindex, string characterkey, byte team, int spawnpoint)
    {
        GameObject player = UnitZ.playerManager.InstantiatePlayer(connectid, userid, usename, characterkey, characterindex, team, spawnpoint);
        if (player == null)
            return;

        NetworkServer.Spawn(player);
        player.GetComponent<CharacterSystem>().CmdGetAuthorized(connectid);
        player.GetComponent<CharacterSystem>().CmdOnSpawned(player.transform.position);
    }

    public void RequestSpawnPlayer(Vector3 position, int connectid, string userid, string usename, int characterindex, string characterkey, byte team, int spawnpoint, NetworkConnection conn)
    {
        GameObject player = UnitZ.playerManager.InstantiatePlayer(connectid, userid, usename, characterkey, characterindex, team, spawnpoint);
        if (player == null)
            return;

        player.GetComponent<CharacterSystem>().CmdGetAuthorized(connectid);
        NetworkServer.ReplacePlayerForConnection(conn, player, 0);
        player.GetComponent<CharacterSystem>().CmdOnSpawned(player.transform.position);
        Debug.Log("Spawn player Net ID" + connectid + " info " + characterindex + " key " + characterkey);
    }

    public GameObject RequestAuthorizedCharacter(string userID, int connectID, NetworkConnection conn)
    {
        CharacterSystem[] players = GameObject.FindObjectsOfType<CharacterSystem>();
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i].UserID != "" && players[i].UserID == userID)
            {
                players[i].CmdGetAuthorized(connectID);
                NetworkServer.ReplacePlayerForConnection(conn, players[i].gameObject, 0);
                Debug.Log("Player Authorized " + connectID);
                return players[i].gameObject;
            }
        }
        return null;
    }

    public GameObject RequestSpawnObject(GameObject gameobj, Vector3 position, Quaternion rotation)
    {
        GameObject obj = (GameObject)Instantiate(gameobj, position, rotation);
        NetworkServer.Spawn(obj);
        return obj;
    }

    public GameObject RequestSpawnItem(GameObject gameobj, int numtag, int num, Vector3 position, Quaternion rotation)
    {
        //Debug.Log("Request spawn object : "+gameobj+" numtag : "+numtag+" num : "+num);
        GameObject obj = (GameObject)Instantiate(gameobj, position, rotation);
        ItemData data = (ItemData)obj.GetComponent<ItemData>();
        data.SetupDrop(numtag, num);
        NetworkServer.Spawn(obj);
        return obj;
    }

    public GameObject RequestSpawnBackpack(GameObject gameobj, string backpackdata, Vector3 position, Quaternion rotation)
    {
        //Debug.Log("Request spawn object : "+gameobj+" numtag : "+numtag+" num : "+num);
        GameObject obj = (GameObject)Instantiate(gameobj, position, rotation);
        ItemBackpack data = (ItemBackpack)obj.GetComponent<ItemBackpack>();
        data.SetDropItem(backpackdata);
        NetworkServer.Spawn(obj);
        return obj;
    }


}
