using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DedicatedManager : MonoBehaviour
{
    public PlayerLobby CurrentPlayerLobby;
    public PlayerConnector CurrentPlayer;
    public bool Online = true;
    public string ServerName = "UnitZ BG dedicated server";
    public string ServerPassword = "";
    public int SceneStartIndex = 0;
    public int MinimumPlayer = 1;
    public int BotNumber = 1;
    public float UpdateTime = 1;
    public float RestartTime = 5;
    public float DisconnectTime = 5;
    public bool AutoRestart = true;
    public GameObject LogCam;

    private float timeTmp;
    private bool isServerRestarting = false;
    private bool isGameEnded = false;
    private bool isStarted = false;
    private GameObject lobbyInstance;

    void Start()
    {
        Debug.Log(SystemInfo.deviceUniqueIdentifier);
        StartDedicated();
        AudioListener.pause = true;
    }

    private void findAllPlayers()
    {
        PlayerLobby[] players = GameObject.FindObjectsOfType<PlayerLobby>();
        UnitZ.gameNetwork.AssignPlayersToSpawn(players);
    }

    public void StartDedicated()
    {
        if (lobbyInstance)
            Destroy(lobbyInstance);
        lobbyInstance = new GameObject("LobbyInstance");

        // setup all parameter
        UnitZ.gameNetwork.IsDedicatedServer = true;
        UnitZ.gameNetwork.matchName = ServerName;
        UnitZ.gameNetwork.HostPassword = ServerPassword;
        UnitZ.gameManager.BotNumber = BotNumber;
        UnitZ.gameManager.IsAutoSpawn = true;
        isServerRestarting = false;
        isGameEnded = false;
        isStarted = false;

        if (Online)
        {
            // start online with unity relay server
            UnitZ.gameNetwork.StartMatchMaker();
            UnitZ.gameNetwork.HostGame(SceneStartIndex, true);
            if (ServerLog.instance != null)
                ServerLog.instance.Log("Starting new lobby using Unity Relay Server");
        }
        else
        {
            // start with local network
            UnitZ.gameNetwork.HostGame(SceneStartIndex, false);
            if (ServerLog.instance != null)
                ServerLog.instance.Log("Starting new lobby on Local network : "+UnitZ.gameNetwork.networkAddress);
        }

    }

    public void OnDisconnect()
    {
        if (!isActiveAndEnabled)
            return;
        UnitZ.gameManager.IsPlaying = false;
        Debug.Log("Server disconnected, Restarting..");
        isServerRestarting = true;
        timeTmp = Time.time;
        if (ServerLog.instance != null)
            ServerLog.instance.Log("Stopped Host.");
    }


    void Update()
    {
        if (UnitZ.gameManager.IsPlaying)
        {
            if (CurrentPlayer != null && CurrentPlayer.isServer)
            {
                if (UnitZ.NetworkGameplay.IsBattleStart && UnitZ.NetworkGameplay.IsBattleEnded)
                {
                    if (!isGameEnded)
                    {
                        // game is over
                        Debug.Log("Game is over");
                        if (ServerLog.instance != null)
                            ServerLog.instance.Log("Match is over");
                        isGameEnded = true;
                        timeTmp = Time.time;
                    }
                    else
                    {
                        // game is already ended
                        if (!isServerRestarting)
                        {
                            if (Time.time >= timeTmp + DisconnectTime)
                            {
                                // count down to disconnecting
                                UnitZ.gameNetwork.Disconnect();
                                Debug.Log("Stop host!");
                                isServerRestarting = true;
                                timeTmp = Time.time;
                            }
                        }
                    }
                }
            }
            if (UnitZ.playerManager.Spectre != null)
            {
                GameObject.Destroy(UnitZ.playerManager.Spectre.gameObject);
            }
        }
        else
        {
            if (isServerRestarting)
            {
                if (Time.time >= timeTmp + RestartTime)
                {
                    if (AutoRestart)
                    {
                        // Restart
                        Debug.Log("Restart!");
                        StartDedicated();
                        isServerRestarting = false;
                        if (ServerLog.instance != null)
                            ServerLog.instance.Log("Restart hosting..");
                    }
                    else
                    {
                        if (ServerLog.instance != null)
                            ServerLog.instance.Log("No restart");
                    }
                    timeTmp = Time.time;
                }
            }
            else
            {
                if (Time.time > timeTmp + UpdateTime)
                {
                    if (CurrentPlayerLobby != null && CurrentPlayerLobby.isServer)
                    {
                        int playercount = UnitZ.gameNetwork.GetLobbyPlayerCount();
                        if (playercount - 1 >= MinimumPlayer)
                        {
                            if (UnitZ.gameNetwork.IsOnLobby() && !isStarted)
                            {
                                Debug.Log("Start Server");
                                findAllPlayers();
                                isStarted = UnitZ.gameNetwork.StartServerGame();
                            }
                        }
                    }
                    timeTmp = Time.time;
                }
            }
        }
    }

    public void ForceStartServer()
    {
        if (ServerLog.instance != null)
            ServerLog.instance.Log("Force start match");
        StartCoroutine(UnitZ.gameNetwork.ServerCountdownCoroutine());
    }
}
