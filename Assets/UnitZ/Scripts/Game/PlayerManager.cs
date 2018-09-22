//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector]
    public CharacterSystem PlayingCharacter;
    public float AutoRespawnDelay = 3;
    public bool AutoRespawn = false;
    public bool AskForRespawn = true;
    public float SaveInterval = 5;
    public bool SavePlayer = true;
    public bool SaveToServer = false;
    public bool ShowDeadUI = true;
    [HideInInspector]
    public SpectreCamera Spectre;
    private float timeTemp = 0;
    private bool savePlayerTemp;
    private float timeAlivetmp = 0;
    private bool autoRespawntmp = false;
    private bool askForRespawntmp = true;
    [HideInInspector]
    public bool spectreMode = false;
    [HideInInspector]
    public float respawnTimer;

    void Start()
    {
        savePlayerTemp = SavePlayer;
        autoRespawntmp = AutoRespawn;
        askForRespawntmp = AskForRespawn;
    }

    public void Reset()
    {
        SavePlayer = savePlayerTemp;
        AutoRespawn = autoRespawntmp;
        AskForRespawn = askForRespawntmp;
        spectreMode = false;
    }

    void Update()
    {
        if (UnitZ.gameNetwork.IsDedicatedServer)
            return;

        if (UnitZ.gameManager.IsPlaying)
            OnPlaying();
    }


    public void Respawn(int spawner)
    {
        if (PlayingCharacter && !PlayingCharacter.IsAlive)
        {
            PlayingCharacter.ReSpawn(spawner);
        }
    }

    public void Respawn(byte team, int spawner)
    {
        if (PlayingCharacter && !PlayingCharacter.IsAlive)
        {
            PlayingCharacter.ReSpawn(team, spawner);
        }
    }

    public void SpectreMode(bool enable)
    {
        spectreMode = enable;
        if (Spectre)
        {
            Spectre.enabled = !enable;
            FPSFlyCamera flycam = Spectre.GetComponent<FPSFlyCamera>();
            if (flycam)
                flycam.enabled = enable;

            if (UnitZ.Hud.IsPanelOpened("Lose"))
            {
                UnitZ.Hud.ClosePanelByName("Lose");
            }
        }
    }

    public void OnPlaying()
    {
        if (PlayingCharacter)
        {
            // player spawned
            if (UnitZ.playerSave && PlayingCharacter.IsAlive)
            {
                // player alive
                if (Time.time >= timeTemp + SaveInterval)
                {
                    timeTemp = Time.time;
                    if (SavePlayer)
                    {
                        PlayingCharacter.SaveCharacterData(SaveToServer);
                    }
                }
                // if close Lose GUI if player alive
                if (UnitZ.Hud.IsPanelOpened("Lose"))
                {
                    UnitZ.Hud.ClosePanelByName("Lose");
                }
                timeAlivetmp = Time.time;
            }

            if (!PlayingCharacter.IsAlive)
            {
                // player is dying
                if (AutoRespawn)
                {
                    // auto matic respawned
                    respawnTimer = (timeAlivetmp + AutoRespawnDelay) - Time.time;
                    if (Time.time > timeAlivetmp + AutoRespawnDelay)
                    {
                        Respawn(-1);
                        UnitZ.Hud.ClosePanelByName("Lose");
                        Debug.Log("Chaacter respawned (" + Time.timeSinceLevelLoad + ")");
                    }
                }
                else
                {
                    if (!UnitZ.Hud.IsPanelOpened("Lose") && !spectreMode)
                    {
                        if (ShowDeadUI)
                            UnitZ.Hud.OpenPanelByName("Lose");
                    }
                }
            }
        }
        else
        {
            // if player still not exist, keep looking for a player
            findPlayerCharacter();
            if (PlayingCharacter == null)
            {
                //Debug.LogWarning ("Can't find player (CharacterSystem) object in the scene. this is may drain game performance (" + Time.timeSinceLevelLoad + ")");
            }
        }

        if (Spectre != null && PlayingCharacter)
        {
            // spectre dead player
            if (!PlayingCharacter.IsAlive)
            {
                // if player dead spectre mode enabled
                Spectre.Active(true);
            }
            else
            {
                Spectre.Active(false);
                Spectre.LookingAt(PlayingCharacter.gameObject.transform.position);
                PlayingCharacter.spectreThis = true;
            }
        }
        else
        {
            Spectre = (SpectreCamera)GameObject.FindObjectOfType(typeof(SpectreCamera));
            if (Spectre == null)
            {
                //Debug.LogWarning ("Can't find (SpectreCamera) object in the scene. this is may drain game performance (" + Time.timeSinceLevelLoad + ")");
            }
        }
    }

    private void findPlayerCharacter()
    {
        if (PlayingCharacter == null)
        {
            CharacterSystem[] go = (CharacterSystem[])GameObject.FindObjectsOfType(typeof(CharacterSystem));
            for (int i = 0; i < go.Length; i++)
            {
                CharacterSystem character = go[i];
                if (character)
                {
                    if (character.IsMine)
                    {
                        spectreMode = false;
                        PlayingCharacter = character;
                        if (SavePlayer)
                        {
                            PlayingCharacter.LoadCharacterData(SaveToServer);
                        }
                    }
                }
            }
        }
    }

    public Vector3 FindASpawnPoint(int spawner)
    {
        Vector3 spawnposition = Vector3.zero;
        PlayerSpawner[] spawnPoint = (PlayerSpawner[])GameObject.FindObjectsOfType(typeof(PlayerSpawner));

        if (spawner < 0 || spawner >= spawnPoint.Length)
        {
            spawner = Random.Range(0, spawnPoint.Length);
        }
        for(int i=0;i< spawnPoint.Length; i++)
        {
            if(spawnPoint[i].Index == spawner)
            {
                spawnposition = spawnPoint[i].transform.position;
                break;
            }
        }

        return spawnposition;
    }

    public GameObject InstantiatePlayer(int playerID, string userID, string userName, string characterKey, int characterIndex, byte team, int spawner)
    {
        if (UnitZ.characterManager == null || UnitZ.characterManager.CharacterPresets.Length <= characterIndex || characterIndex < 0)
            return null;

        CharacterSystem characterspawn = UnitZ.characterManager.CharacterPresets[characterIndex].CharacterPrefab;

        if (characterspawn)
        {

            GameObject player = (GameObject)GameObject.Instantiate(characterspawn.gameObject, FindASpawnPoint(spawner), Quaternion.identity);
            CharacterSystem character = player.GetComponent<CharacterSystem>();
            character.NetID = playerID;
            character.Team = team;
            character.CharacterKey = characterKey;
            character.UserName = userName;
            character.UserID = userID;
            MouseLock.MouseLocked = true;
            return player;
        }
        return null;
    }

}
