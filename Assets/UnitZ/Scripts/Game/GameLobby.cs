//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLobby : MonoBehaviour
{

    public static GameLobby instance = null;
    public RectTransform playerListContentTransform;
    public GameObject warningDirectPlayServer;
    public Transform addButtonRow;
    public List<PlayerLobby> players = new List<PlayerLobby>();
    public PlayerLobby currentPlayerLobby;

    public InputField BotNumber;
    public Button StartButton;
    public Button ReadyButton;
    public GameObject ReadyIcon;

    private void Start()
    {
        instance = this;
    }

    private void OnEnable()
    {
        instance = this;
        if (BotNumber != null)
            BotNumber.text = UnitZ.gameManager.BotNumber.ToString();
    }

    public void DisconnectMatch()
    {
        if (currentPlayerLobby == null)
            return;

        if (currentPlayerLobby.isServer)
        {
            Debug.Log("stop lobby");
            UnitZ.gameNetwork.DestroyMatch();
        }
        else
        {
            Debug.Log("leave lobby");
            UnitZ.gameNetwork.LeaveMatch();
        }
    }

    public void Update()
    {
        if (currentPlayerLobby == null)
            return;

        if (BotNumber != null)
        {
            BotNumber.gameObject.SetActive(currentPlayerLobby.isServer);
            int.TryParse(BotNumber.text, out UnitZ.gameManager.BotNumber);
            if (UnitZ.gameManager.BotNumber > UnitZ.gameManager.BotMax)
                UnitZ.gameManager.BotNumber = UnitZ.gameManager.BotMax;
            BotNumber.text = UnitZ.gameManager.BotNumber.ToString();
        }

        if (currentPlayerLobby.isServer)
        {
            if (ReadyButton)
                ReadyButton.gameObject.SetActive(false);

            if (StartButton)
                StartButton.gameObject.SetActive(true);
        }
        else
        {
            if (ReadyButton)
                ReadyButton.gameObject.SetActive(true);

            if (StartButton)
                StartButton.gameObject.SetActive(false);
        }

        if (ReadyIcon != null)
            ReadyIcon.SetActive(currentPlayerLobby.IsReady);

    }

    public void StartMatch()
    {
        if (currentPlayerLobby != null)
        {
            if (currentPlayerLobby.isServer)
            {
                UnitZ.gameNetwork.AssignPlayersToSpawn(players.ToArray());
                UnitZ.gameNetwork.StartServerGame();
            }
        }
    }

    public void GetReady()
    {
        if (currentPlayerLobby != null)
            currentPlayerLobby.OnReadyClicked();
    }

    public void DisplayDirectServerWarning(bool enabled)
    {
        if (warningDirectPlayServer != null)
            warningDirectPlayServer.SetActive(enabled);
    }

    public void AddPlayer(PlayerLobby player)
    {
        if (players.Contains(player))
            return;

        players.Add(player);

        if (playerListContentTransform != null)
        {
            player.transform.SetParent(playerListContentTransform, false);
            RectTransform playerRect = player.GetComponent<RectTransform>();
            playerRect.sizeDelta = new Vector2(playerListContentTransform.rect.width, playerRect.sizeDelta.y);
            playerListContentTransform.sizeDelta = new Vector2(playerListContentTransform.sizeDelta.x, players.Count * playerRect.sizeDelta.y);
        }

        if (addButtonRow != null)
            addButtonRow.transform.SetAsLastSibling();

        PlayerListModified();
    }

    public void RemovePlayer(PlayerLobby player)
    {
        players.Remove(player);
        PlayerListModified();
    }

    public void PlayerListModified()
    {
        int i = 0;
        foreach (PlayerLobby p in players)
        {
            p.OnPlayerListChanged(i);
            ++i;
        }
    }
}