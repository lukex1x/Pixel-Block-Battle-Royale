//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using UnityEngine.Networking;


[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(AudioSource))]
public class DamageManager : NetworkBehaviour
{
    [Header("Living")]
    public bool IsAlive = true;
    [SyncVar(hook = "OnHPChanged")]
    public byte HP = 100;
    public byte HPmax = 100;
    public GameObject DeadReplacement;
    public float DeadReplaceLifeTime = 180;
    public bool DestroyOnDead = true;
    public float DestroyDelay = 3;
    public AudioClip[] SoundPain;
    public AudioSource Audiosource;
    public Text3D NameTag;

    [HideInInspector]
    public bool dieByLifeTime = false;
    //[HideInInspector]
    public bool spectreThis = false;
    [SyncVar]
    public byte Team = 0;
    //[HideInInspector]
    [SyncVar]
    public int NetID = -1;
    //[HideInInspector]
    [SyncVar]
    public string UserID = "";
    //[HideInInspector]
    [SyncVar]
    public string UserName = "";
    [HideInInspector]
    [SyncVar]
    public int LastHitByID = -1;
    private Vector3 directionHit;
    protected Text3D nameTag;
    public virtual void Awake()
    {

    }

    public virtual void Start()
    {
        if (NameTag != null)
        {
            nameTag = GameObject.Instantiate(NameTag, this.transform);
            nameTag.transform.localPosition = new Vector3(0, 2.1f, 0);
            nameTag.SetText(UserName);
        }
        Audiosource = this.GetComponent<AudioSource>();
    }

    public override void OnStartClient()
    {
        if (HP <= 0)
        {
            SetEnable(false);
        }
        base.OnStartClient();
    }

    public virtual void Update()
    {
        if (HP > HPmax)
            HP = HPmax;
    }

    public virtual void DirectDamage(DamagePackage pack)
    {
        ApplyDamage((byte)((float)pack.Damage), pack.Direction, pack.ID, pack.Team);
    }

    public virtual void ApplyDamage(byte damage, Vector3 direction, int attackerID, byte team)
    {
        directionHit = direction;
        DoApplyDamage(damage, direction, attackerID, team);
        if (Audiosource && SoundPain.Length > 0)
        {
            Audiosource.PlayOneShot(SoundPain[Random.Range(0, SoundPain.Length)]);
        }
    }

    public virtual void DoApplyDamage(byte damage, Vector3 direction, int attackerID, byte team)
    {
        if (isServer)
        {
            lastHP = HP;
            directionHit = direction;
            if (Team != team || team == 0)
            {
                if (HP <= 0)
                    return;

                if (damage >= HP)
                {
                    HP = 0;
                    LastHitByID = attackerID;
                    CmdOnDead(LastHitByID, NetID, "Kill");
                    return;
                }
                else
                {
                    HP -= damage;
                }

                if (HP <= 0)
                {
                    LastHitByID = attackerID;
                    CmdOnDead(LastHitByID, NetID, "Kill");
                }
            }
        }
    }

    private byte lastHP = 0;
    private bool alreadyDead = false;

    void OnHPChanged(byte hp)
    {
        if (hp <= 0)
        {
            SetEnable(false);
        }
        else
        {
            if (hp >= HPmax)
            {
                if (!IsAlive)
                {
                    SetEnable(true);
                }
            }
        }

        if (!IsAlive && lastHP > 0)
        {
            if (!alreadyDead)
            {
                SpawnDeadBody();
                alreadyDead = true;
            }
        }

        lastHP = HP;
        HP = hp;
    }

    private void SpawnDeadBody()
    {
        if (!isQuitting)
        {
            if (DeadReplacement)
            {
                GameObject deadbody = (GameObject)GameObject.Instantiate(DeadReplacement, this.transform.position, Quaternion.identity);
                OnDeadBodySpawned(deadbody);
                CopyTransformsRecurse(this.transform, deadbody);
                if (dieByLifeTime)
                    DeadReplaceLifeTime = 3;
                GameObject.Destroy(deadbody, DeadReplaceLifeTime);
            }
        }
    }

    [Command(channel = 0)]
    void CmdOnDead(int killer, int me, string killtype)
    {
        RpcODead(killer, me, killtype);
    }

    [ClientRpc(channel = 0)]
    void RpcODead(int killer, int me, string killtype)
    {
        OnThisThingDead();
        OnKilled(killer, me, killtype);
        if (DestroyOnDead)
            GameObject.Destroy(this.gameObject, DestroyDelay);
    }

    public void ReSpawn(byte team, int spawner)
    {
        CmdRespawn(team, spawner);
    }

    public void ReSpawn(int spawner)
    {
        CmdRespawn(Team, spawner);
    }

    [Command(channel = 0)]
    private void CmdRespawn(byte team, int spawner)
    {
        HP = HPmax;
        Team = team;
        RpcRespawn(team, spawner);
    }

    [ClientRpc(channel = 0)]
    private void RpcRespawn(byte team, int spawner)
    {
        HP = HPmax;
        Team = team;
        OnRespawn(spawner);
    }

    [Command]
    public void CmdGetAuthorized(int netID)
    {
        NetID = netID;
    }

    public virtual void SetEnable(bool enable)
    {
        IsAlive = enable;
        foreach (Transform ob in this.transform)
        {
            ob.gameObject.SetActive(enable);
        }

        Renderer[] renderers = this.GetComponentsInChildren<Renderer>();
        foreach (Renderer ob in renderers)
        {
            ob.enabled = enable;
        }
        Collider[] cols = this.GetComponentsInChildren<Collider>();
        foreach (Collider col in cols)
        {
            col.enabled = enable;
        }
        if (nameTag)
            nameTag.gameObject.SetActive(enable);
    }

    public virtual void OnKilled(int killer, int me, string killtype)
    {
        // Do something when get killed
    }

    public virtual void OnThisThingDead()
    {
        // Do something when dying
    }

    public virtual void OnRespawn(int spawner)
    {
        alreadyDead = false;
        // Do something when respawn
    }

    public virtual void OnDestroyed()
    {
        // De something before removed
    }

    public virtual void OnDeadBodySpawned(GameObject deadbody)
    {
        // De something on the deadbody object
    }

    public void CopyTransformsRecurse(Transform src, GameObject dst)
    {
        dst.transform.position = src.position;
        dst.transform.rotation = src.rotation;
        if (dst.GetComponent<Rigidbody>())
            dst.GetComponent<Rigidbody>().AddForce(directionHit * 5, ForceMode.VelocityChange);

        foreach (Transform child in dst.transform)
        {
            var curSrc = src.Find(child.name);
            if (curSrc)
            {
                CopyTransformsRecurse(curSrc, child.gameObject);
            }
        }
    }

    private bool isQuitting = false;

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    public override void OnNetworkDestroy()
    {
        if (isQuitting)
            return;

        OnDestroyed();
        base.OnNetworkDestroy();
    }
}
