//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using UnityEngine.Networking;

public class NetworkGameplayManager : NetworkBehaviour
{
    public PlayersManager playersManager;
    public ScoreManager scoreManager;
    public EnvironmentManager environment;
    public ChatLog chatLog;
    public bool MapEnabled = true;
    [SyncVar]
    [HideInInspector]
    public GameView View;
    [SyncVar]
    [HideInInspector]
    public int PlayersAlive = 0;
    [SyncVar]
    [HideInInspector]
    public int PlayersAliveMax = 0;
    [HideInInspector]
    public int EndgameRanked = 0;
    [SyncVar]
    [HideInInspector]
    public bool IsBattleStart;
    [HideInInspector]
    public bool IsBattleEnded;
    public bool AutoSpawn = true;
    public bool ShowDeadUI = true;


    public virtual void Awake()
    {
        UnitZ.NetworkGameplay = this;
        UnitZ.gameManager.IsBattleStart = false;
        UnitZ.gameManager.IsPlaying = true;
        UnitZ.gameManager.IsAutoSpawn = AutoSpawn;
        UnitZ.playerManager.ShowDeadUI = ShowDeadUI;
        if (playersManager == null)
            playersManager = this.GetComponent<PlayersManager>();
        if (scoreManager == null)
            scoreManager = this.GetComponent<ScoreManager>();
        if (chatLog == null)
            chatLog = this.GetComponent<ChatLog>();
    }


    public void PlayerKilled(NetworkConnection target)
    {
        if (isServer && target != null)
            TargetPlayerKilled(target, PlayersAlive + 1);
    }

    [TargetRpc(channel = 0)]
    private void TargetPlayerKilled(NetworkConnection target, int currentRank)
    {
        Debug.Log("*** You got kill at rank " + currentRank);
        EndgameRanked = currentRank;
        GameOver(false);
    }

    public void PlayerWin(NetworkConnection target)
    {
        if (isServer)
            TargetPlayerWin(target);
    }

    [TargetRpc(channel = 0)]
    private void TargetPlayerWin(NetworkConnection target)
    {
        EndgameRanked = 1;
        GameOver(true);
    }

    [ClientRpc(channel = 0)]
    private void RpcBroadcastText(string text, float sec)
    {
        UnitZ.Hud.BroadcastingText(text, sec);
    }

    public void Broadcast(string text, float sec)
    {
        RpcBroadcastText(text, sec);
    }

    public void GameOver(bool isWin)
    {
        if (isWin)
        {
            UnitZ.Hud.OpenPanelByName("Win");
        }
        else
        {
            if (UnitZ.playerManager.ShowDeadUI)
                UnitZ.Hud.OpenPanelByName("Lose");
        }
    }

    public virtual void Start()
    {
        UnitZ.Hud.CountdownText.text = "";
        UnitZ.Hud.AliveText.text = "";
        View = UnitZ.gameManager.GameViewType;
    }

    public virtual void Update()
    {
        if (isClient)
        {
            UnitZ.gameManager.GameViewType = View;
        }

        if (UnitZ.Hud.Map != null)
        {
            if (!MapEnabled)
                UnitZ.Hud.Map.gameObject.SetActive(false);
        }
        if (UnitZ.Hud.MiniMap != null)
        {
            if (!MapEnabled)
                UnitZ.Hud.MiniMap.gameObject.SetActive(false);
        }
    }
}
