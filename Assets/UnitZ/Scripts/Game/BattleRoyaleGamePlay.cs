//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine.Networking;
using UnityEngine;

[System.Serializable]
public class DeadArea
{
    public float Time;
    public float EscapeTime;
    public float Radius;
    public byte Damage;
    public bool AirDrop;
}

[NetworkSettings(channel = 0, sendInterval = 1.0f)]
public class BattleRoyaleGamePlay : NetworkGameplayManager
{

    private float timeTmp;
    public float CountdownToPlane = 10;
    public GameObject AirplaneObject;
    public DeadArea[] DeadAreas;
    private int currentArea;
    [SyncVar]
    public int CountdownToPlaneSync;

    [SyncVar]
    [HideInInspector]
    public Vector3 CentreArea;
    [SyncVar]
    [HideInInspector]
    public float Radius;
    [SyncVar(hook = "onLerp")]
    [HideInInspector]
    public float LerpValue = 0;
    public GameObject SafeArea;
    public GameObject DeadArea;

    private float timeAreaTmp;
    [HideInInspector]
    public GameObject safeArea;
    [HideInInspector]
    public GameObject lastDeadArea;
    private Vector3 scaleTmp;
    private Vector3 positionTmp;
    private float timeEscapeTmp;
    private bool isEscape;


    public override void Start()
    {
        UnitZ.gameManager.IsAutoSpawn = true;
        GameObject centre = GameObject.Find("CentreOfBattle");
        if (centre)
        {
            CentreArea = centre.transform.position;
        }
        else
        {
            Debug.LogWarning("There have no Centre of Battle object");
        }
        
        IsBattleEnded = false;
        IsBattleStart = false;
        timeTmp = Time.timeSinceLevelLoad;
        base.Start();
    }

    void onLerp(float lerp)
    {
        LerpValue = lerp;
    }

    public void SpawnPlane(bool drop, Vector3 passingTo)
    {
        if (AirplaneObject != null)
        {
            AirplaneSpawner spawner = GameObject.FindObjectOfType<AirplaneSpawner>();
            if (spawner != null)
            {
                GameObject planeobj = UnitZ.gameNetwork.RequestSpawnObject(AirplaneObject, spawner.SpawnPoint(), Quaternion.identity);
                if (planeobj != null)
                {
                    Debug.Log("Spawn plane");
                    planeobj.transform.LookAt(passingTo + (Vector3.up * planeobj.transform.position.y));
                    Airplane plane = planeobj.GetComponent<Airplane>();
                    if (plane != null)
                    {
                        plane.DropSupply = drop;
                        plane.SetDrop(passingTo);
                    }
                }
            }
        }
    }

    [Command(channel = 0)]
    void CmdStartNewArea(Vector3 position, Vector3 scale, int index)
    {
        RpcStartNewArea(position, scale, index);
    }

    [ClientRpc(channel = 0)]
    void RpcStartNewArea(Vector3 position, Vector3 scale, int index)
    {
        timeAreaTmp = Time.time;
        positionTmp = position;
        scaleTmp = scale;
        currentArea = index;
        if (lastDeadArea)
        {
            lastDeadArea.transform.position = positionTmp;
            lastDeadArea.transform.localScale = scaleTmp;
        }
    }

    public float GetTimeToNextArea
    {
        get
        {
            float time = 0;
            if (currentArea > 0)
                time = DeadAreas[currentArea].Time - (Time.time - timeAreaTmp);
            if (time < 0)
                time = 0;

            time = Mathf.Abs(time);
            return time;
        }
    }

    private float broadcastTimpTmp = 0;
    public GameObject[] allPlayer;
    public override void Update()
    {
        if (UnitZ.Hud.AliveText != null)
        {
            UnitZ.Hud.AliveText.text = PlayersAlive.ToString() + " Alive";
        }

        if (isServer)
        {
            allPlayer = GameObject.FindGameObjectsWithTag("Player");
            int alive = 0;
            int lastplayer = -1;

            for (int i = 0; i < allPlayer.Length; i++)
            {
                if (allPlayer[i] != null)
                {
                    CharacterSystem player = allPlayer[i].GetComponent<CharacterSystem>();
                    if (player != null && player.IsAlive)
                    {
                        lastplayer = i;
                        alive += 1;
                    }
                }
            }
            PlayersAlive = alive;

            if (IsBattleStart && !IsBattleEnded)
            {
                if (alive <= 1)
                {
                    if (lastplayer != -1)
                    {
                        if (allPlayer[lastplayer] != null)
                        {
                            CharacterSystem player = allPlayer[lastplayer].GetComponent<CharacterSystem>();
                            if (player != null && player.NetID != -1)
                                PlayerWin(player.connectionToClient);
                        }
                    }
                    IsBattleEnded = true;
                }
            }

        }

        if (!IsBattleStart)
        {
            if (UnitZ.Hud.CountdownText)
            {
                if (CountdownToPlaneSync > 0)
                {
                    UnitZ.Hud.CountdownText.text = CountdownToPlaneSync.ToString();
                }
                else
                {
                    UnitZ.Hud.CountdownText.text = "";
                }
            }

            UnitZ.Hud.BroadcastingText("Battle Starts in", 0.5f);
            if (isServer)
            {
                CountdownToPlaneSync = (int)((timeTmp + CountdownToPlane) - Time.timeSinceLevelLoad);
                if (Time.timeSinceLevelLoad > timeTmp + CountdownToPlane)
                {
                    SpawnPlane(false, Vector3.zero);
                    isEscape = false;
                    timeAreaTmp = Time.time;
                    IsBattleStart = true;
                    UnitZ.gameManager.IsBattleStart = true;
                    IsBattleEnded = false;
                    getInThePlane();
                }
            }
        }
        else
        {
            if (safeArea == null && SafeArea)
                safeArea = (GameObject)GameObject.Instantiate(SafeArea.gameObject, Vector3.zero, Quaternion.identity);

            if (lastDeadArea == null && DeadArea)
                lastDeadArea = (GameObject)GameObject.Instantiate(DeadArea.gameObject, Vector3.zero, Quaternion.identity);

            if (lastDeadArea)
                lastDeadArea.SetActive(currentArea > 0);

            if (safeArea)
                safeArea.SetActive(currentArea > 0);

            if (isServer)
            {
                if (DeadAreas.Length > currentArea)
                {
                    LerpValue = 0;
                    DeadArea NextArea = DeadAreas[currentArea];
                    currentAreaDamage = NextArea.Damage;
                    Radius = NextArea.Radius;

                    if (Time.time >= timeAreaTmp + NextArea.Time)
                    {
                        if (!isEscape)
                        {
                            timeEscapeTmp = Time.time;
                            isEscape = true;
                        }

                        float timing = 1 - (((NextArea.EscapeTime + timeEscapeTmp) - Time.time) / NextArea.EscapeTime);
                        if (currentArea > 0)
                            LerpValue = timing;

                        if (Time.time > NextArea.EscapeTime + timeEscapeTmp)
                        {
                            if (lastDeadArea)
                            {
                                positionTmp = CentreArea;
                                scaleTmp = new Vector3(Radius, SafeArea.transform.localScale.y, Radius) * 2;
                            }

                            if (currentArea >= DeadAreas.Length - 1)
                                return;

                            currentArea += 1;
                            isEscape = false;
                            timeAreaTmp = Time.time;
                            CentreArea = GetArea(CentreArea, Radius - (DeadAreas[currentArea].Radius));
                            CmdStartNewArea(positionTmp, scaleTmp, currentArea);

                            if (DeadAreas[currentArea].AirDrop)
                                SpawnPlane(true, CentreArea);
                        }
                    }
                    else
                    {
                        float timeLeft = (timeAreaTmp + NextArea.Time) - Time.time;
                        if (timeLeft < 60)
                        {
                            if (Time.time >= broadcastTimpTmp + 10)
                            {
                                {
                                    Broadcast("Retracting Area in " + Mathf.Ceil(timeLeft) + " second", 3);
                                    broadcastTimpTmp = Time.time;
                                }
                            }
                        }
                    }
                }
                damageUpdate();
            }

            if (safeArea && lastDeadArea)
            {
                safeArea.transform.position = CentreArea;
                safeArea.transform.localScale = new Vector3(Radius, SafeArea.transform.localScale.y, Radius) * 2;
                lastDeadArea.transform.position = Vector3.Lerp(positionTmp, safeArea.transform.position, LerpValue);
                lastDeadArea.transform.localScale = Vector3.Lerp(scaleTmp, safeArea.transform.localScale, LerpValue);
            }
        }
        base.Update();
    }

    private byte currentAreaDamage;
    private float dmTimeTmp;
    private void damageUpdate()
    {
        // damage a players if they out of the blue area every second
        if (Time.time >= dmTimeTmp + 1)
        {
            for (int i = 0; i < allPlayer.Length; i++)
            {
                if (allPlayer[i] != null)
                {
                    CharacterSystem player = allPlayer[i].GetComponent<CharacterSystem>();
                    if (player != null)
                    {
                        if (lastDeadArea != null && lastDeadArea.activeSelf)
                        {
                            Vector3 playerPos = player.transform.position;
                            playerPos.y = 0;
                            Vector3 deadAreaPos = lastDeadArea.transform.position;
                            deadAreaPos.y = 0;

                            float distance = Vector3.Distance(player.transform.position, lastDeadArea.transform.position);
                            if (distance > (lastDeadArea.transform.localScale.x / 2.0f))
                            {
                                // apply damage 1 to player
                                player.ApplyDamage(currentAreaDamage, Vector3.up, -1, 0);
                            }
                        }
                    }
                }
            }
            dmTimeTmp = Time.time;
        }
    }

    private Vector3 GetArea(Vector3 pos, float radius)
    {
        Vector2 newpos = Random.insideUnitCircle * radius;
        return pos + new Vector3(newpos.x, 0, newpos.y);
    }

    private void getInThePlane()
    {
        Airplane plane = (Airplane)GameObject.FindObjectOfType(typeof(Airplane));
        SpawnAI();

        if (plane != null)
        {
            GameObject[] allPlayer = GameObject.FindGameObjectsWithTag("Player");
            PlayersAliveMax = allPlayer.Length;
            for (int i = 0; i < allPlayer.Length; i++)
            {
                Vehicle planev = plane.GetComponent<Vehicle>();
                if (planev != null)
                {
                    int openseat = planev.FindOpenSeatID();
                    CharacterDriver driver = allPlayer[i].GetComponent<CharacterDriver>();
                    if (driver != null)
                        planev.GetInTheVehicle(driver, openseat);
                }
            }
        }
    }

    private GameObject[] AIs;
    public void SpawnAI()
    {
        if (isServer)
        {
            EnemySpawner spawner = (EnemySpawner)GameObject.FindObjectOfType<EnemySpawner>();
            if (spawner)
            {
                spawner.MaxObject = UnitZ.gameManager.BotNumber;
                AIs = spawner.Spawn(UnitZ.gameManager.BotNumber);
                for (int i = 0; i < AIs.Length; i++)
                {
                    if (AIs[i] != null)
                    {
                        AICharacterShooterNav ai = AIs[i].GetComponent<AICharacterShooterNav>();
                        if (ai != null)
                        {
                            ai.Fighting = false;
                        }
                    }
                }
            }
        }
    }

    private bool alreadyEquip;
    public void EquipItemAI()
    {
        if (isServer)
        {
            if (!alreadyEquip)
            {
                for (int i = 0; i < AIs.Length; i++)
                {
                    if (AIs[i] != null)
                    {
                        CharacterSystem character = AIs[i].GetComponent<CharacterSystem>();
                        if (character != null)
                        {
                            character.inventory.ApplyStarterItem();
                        }

                        AICharacterShooterNav ai = AIs[i].GetComponent<AICharacterShooterNav>();
                        if (ai != null)
                        {
                            ai.Fighting = true;
                        }
                    }
                }
                alreadyEquip = true;
            }
        }
    }
}
