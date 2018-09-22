using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDMGamePlay : NetworkGameplayManager
{
    public GameObject GUISpawnMenu;
    public GameObject GUIDead;
    private PlayerConnector playerConnector;

    public override void Start()
    {
        UnitZ.gameManager.IsBattleStart = true;
        setupAI();
        base.Start();
    }

    public override void Update()
    {
        if (UnitZ.playerManager == null)
            return;

        bool isNoPlayer = !UnitZ.playerManager.PlayingCharacter || !UnitZ.playerManager.PlayingCharacter.IsAlive;

        if (GUISpawnMenu)
            GUISpawnMenu.gameObject.SetActive(isNoPlayer);



        if (UnitZ.playerManager.PlayingCharacter == null || isNoPlayer)
        {
            MouseLock.MouseLocked = false;
        }

        if (playerConnector == null)
        {
            playerConnector = (PlayerConnector)GameObject.FindObjectOfType(typeof(PlayerConnector));
        }
        base.Update();
    }

    private void setupAI()
    {
        if (isServer)
        {
            EnemySpawner[] spawner = (EnemySpawner[])GameObject.FindObjectsOfType<EnemySpawner>();
            for (int i = 0; i < spawner.Length; i++)
            {
                if (spawner[i] != null)
                    spawner[i].MaxObject = (int)((float)UnitZ.gameManager.BotNumber / spawner.Length);
            }
        }
    }


    public void SpawnButtonA()
    {
        if (playerConnector)
        {
            playerConnector.RequestSpawnWithTeam(1, 0);
        }
        else
        {
            UnitZ.playerManager.Respawn(1, 0);
        }
    }

    public void SpawnButtonB()
    {
        if (playerConnector)
        {
            playerConnector.RequestSpawnWithTeam(2, 1);
        }
        else
        {
            UnitZ.playerManager.Respawn(2, 1);
        }
    }
}
