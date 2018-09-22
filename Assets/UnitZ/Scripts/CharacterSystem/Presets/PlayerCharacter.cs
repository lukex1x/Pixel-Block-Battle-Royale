using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterFootStep))]
[RequireComponent(typeof(CharacterAnimation))]
[RequireComponent(typeof(CharacterDriver))]
[RequireComponent(typeof(CharacterInventory))]
[RequireComponent(typeof(FPSController))]
[RequireComponent(typeof(CharacterMotor))]

public class PlayerCharacter : CharacterSystem
{
    [SyncVar]
    public byte Pill;
    public byte PillMax = 100;
    public byte RegetRate = 1;

    public override void Start()
    {
        if (animator)
            animator.SetInteger("Shoot_Type", AttackType);

        InvokeRepeating("pillUpdate", 1.0f, 1.0f);
        base.Start();
    }

    public override void DoApplyDamage(byte damage, Vector3 direction, int attackerID, byte team)
    {
        if (UnitZ.gameManager && !UnitZ.gameManager.IsBattleStart)
            return;

        base.DoApplyDamage(damage, direction, attackerID, team);
    }

    public override void Update()
    {
        if (animator == null)
            return;

        animator.SetInteger("BodyState", MovementIndex);

        base.Update();
    }

    public override void PlayMoveAnimation(float magnitude, float side)
    {
        if (animator)
        {
            animator.SetFloat("Velocity", magnitude);
            animator.SetFloat("SideVelocity", side);
        }

        base.PlayMoveAnimation(magnitude, side);
    }

    public override void PlayAttackAnimation(bool attacking, int attacktype)
    {
        if (animator)
        {
            if (attacking)
            {
                animator.SetTrigger("Shoot");
            }
            animator.SetInteger("Shoot_Type", attacktype);
        }
        base.PlayAttackAnimation(attacking, attacktype);
    }

    public void OnEquipChanged(int type)
    {
        if (animator)
            animator.SetInteger("Shoot_Type", type);
    }

    public override void OnDeadBodySpawned(GameObject deadbody)
    {
        //tell the spectre cam to look at this corpse
        SpectreCamera spectre = (SpectreCamera)GameObject.FindObjectOfType(typeof(SpectreCamera));
        if (spectre)
        {
            spectre.LookingAtObject(deadbody);
        }
        base.OnDeadBodySpawned(deadbody);
    }

    public override void SetEnable(bool enable)
    {

        if (this.GetComponent<PlayerView>())
            this.GetComponent<PlayerView>().enabled = enable;

        if (this.GetComponent<FPSController>())
            this.GetComponent<FPSController>().enabled = enable;

        if (this.GetComponent<CharacterMotor>())
        {
            this.GetComponent<CharacterMotor>().enabled = enable;
            this.GetComponent<CharacterMotor>().Reset();
        }

        if (this.GetComponent<CharacterController>())
            this.GetComponent<CharacterController>().enabled = enable;

        if (this.GetComponent<NetworkTransform>())
            this.GetComponent<NetworkTransform>().enabled = enable;

        if (this.GetComponent<CharacterDriver>())
            this.GetComponent<CharacterDriver>().NoVehicle();

        base.SetEnable(enable);
    }

    public override void OnKilled(int killer, int me, string killtype)
    {
        // this function can be call on client
        if (UnitZ.NetworkGameplay.scoreManager)
        {
            if (NetID != -1)
            {
                UnitZ.NetworkGameplay.PlayerKilled(connectionToClient);
            }
            UnitZ.NetworkGameplay.scoreManager.UpdateScore(LastHitByID, NetID, 1);
        }

        base.OnKilled(killer, me, killtype);
    }

    public override void OnThisThingDead()
    {
        if (NetID != -1)
        {
            RemoveCharacterData();
        }

        CharacterItemDroper itemdrop = this.GetComponent<CharacterItemDroper>();
        if (itemdrop)
            itemdrop.DropItem();

        if (isServer)
        {
            ItemDropAfterDead dropafterdead = this.GetComponent<ItemDropAfterDead>();
            if (dropafterdead)
                dropafterdead.DropItem();
        }

        base.OnThisThingDead();
    }

    public override void OnRespawn(int spawner)
    {
        if (isLocalPlayer)
        {
            this.transform.position = UnitZ.playerManager.FindASpawnPoint(spawner);
        }
        this.SendMessage("Respawn", SendMessageOptions.DontRequireReceiver);
        if (this.GetComponent<CharacterInventory>())
            this.GetComponent<CharacterInventory>().SetupStarterItem();
        base.OnRespawn(spawner);
    }


    public override void OnDestroyed()
    {
        if (isServer)
        {
            if (NetID != -1)
            {
                if (UnitZ.NetworkGameplay.playersManager != null)
                {
                    UnitZ.NetworkGameplay.playersManager.RemovePlayer(NetID);
                }
            }
        }
        base.OnDestroyed();
    }

    [Command]
    private void CmdUsepill(byte num)
    {
        if (Pill + num > PillMax)
        {
            Pill = PillMax;
            return;
        }

        Pill += num;
    }

    public void UsePill(byte num)
    {
        CmdUsepill(num);
    }

    [Command]
    private void CmdUseheal(byte num)
    {
        if (HP + num > HPmax)
            HP = HPmax;

        HP += num;
    }

    public void UseHeal(byte num)
    {
        CmdUseheal(num);
    }

    private void pillUpdate()
    {
        if (isServer)
        {
            if (Pill > 0)
            {
                if (HP < HPmax)
                {
                    if (HP + RegetRate > HPmax)
                    {
                        HP = HPmax;
                    }
                    else
                    {
                        HP += RegetRate;
                    }
                }
                Pill -= 1;
            }
        }
    }
}
