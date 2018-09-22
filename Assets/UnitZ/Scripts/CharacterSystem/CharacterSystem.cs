//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com

using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(FPSRayActive))]

public class CharacterSystem : DamageManager
{
    [HideInInspector]
    [SyncVar]
    public string CharacterKey = "";
    [HideInInspector]
    public CharacterInventory inventory;
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public FPSRayActive rayActive;
    [HideInInspector]
    public CharacterController controller;
    [HideInInspector]
    public CharacterMotor Motor;
    [HideInInspector]
    public bool Sprint;
    [HideInInspector]
    public bool IsMine;

    [Header("Moving")]
    public BodyStatePreset[] MovementPreset = { new BodyStatePreset() };
    [HideInInspector]
    public float CurrentMoveSpeed = 0.9f;
    [HideInInspector]
    public float CurrentMoveSpeedMax = 1.5f;
    public float TurnSpeed = 5;
    [SyncVar]
    public byte MovementIndex = 0;
    [HideInInspector]
    public Vector3 MoveVelocity;

    [Header("Attack")]
    public float PrimaryWeaponDistance = 1;
    public int PrimaryItemType;
    public int AttackType = 0;
    public byte Damage = 2;
    public float DamageLength = 1;
    public byte Penetrate = 1;
    public Vector3 DamageOffset = Vector3.up;

    [Header("Sound")]
    public AudioClip[] DamageSound;
    public AudioClip[] SoundIdle;

    [Header("Networking")]
    public float SendRate = 5;
    public float FarSendRate = 0.5f;
    public float SyncDistance = 10;
    [HideInInspector]
    public bool isSeeAround;
    [HideInInspector]
    public float currentSendingRate = 0;
    [HideInInspector]
    public bool isServerControl = false;
    [HideInInspector]
    public float spdMovAtkMult = 1;
    [SyncVar]
    private Vector3 positionSync;
    [SyncVar]
    private float rotationYSync;
    private Vector3 positionSyncTmp;
    private float rotationYSyncTmp;

    private Vector3 positionLastTrip;
    private Vector3 positionLate;
    private float timeTmpsending;
    private float timeLastTrip;
    private float latencyTime = 0;
    private Vector3 positionInterpolate;
    private Quaternion rotationInterpolate;
    private Vector3 previousPosition;
    private float updateTimeTmp;

    public override void Awake()
    {
        SetupAwake();
        base.Awake();
    }

    public void SetupAwake()
    {
        Motor = this.GetComponent<CharacterMotor>();
        controller = this.GetComponent<CharacterController>();
        Audiosource = this.GetComponent<AudioSource>();
        animator = this.GetComponent<Animator>();
        rayActive = this.GetComponent<FPSRayActive>();
        inventory = this.GetComponent<CharacterInventory>();
        spdMovAtkMult = 1;
        positionSync = this.transform.position;
        rotationYSync = this.transform.rotation.eulerAngles.y;
        positionLate = positionSync;
        positionLastTrip = positionSync;
        if (animator)
            animator.applyRootMotion = false;
    }

    public override void Update()
    {
        UpdateFunction();
        base.Update();
    }

    public void UpdateFunction()
    {
        currentSendingRate = SendRate;
        if (nameTag != null)
        {
            nameTag.gameObject.SetActive(!IsMine && IsAlive);
        }
        if (UnitZ.gameManager && UnitZ.gameManager.UserID == UserID && NetID != -1)
        {
            IsMine = true;
        }
        else
        {
            IsMine = false;
        }
        if (Time.time > updateTimeTmp + 1f)
        {
            isSeeAround = UnitZ.aiManager.IsPlayerAround(this.gameObject, SyncDistance);
            updateTimeTmp = Time.time;
        }
        if (!isSeeAround)
        {
            // reduce send rate if far from player
            currentSendingRate = FarSendRate;
        }
        UpdatePosition();
    }

    public virtual void PlayAttackAnimation(bool attacking, int attacktype)
    {

    }

    public virtual void PlayMoveAnimation(float magnitude, float side)
    {

    }

    bool jumped = false;
    public virtual void Jump()
    {
        if (Motor.IsGroundedTest() && !jumped)
        {
            animator.SetTrigger("Jump");
            jumped = true;
        }
        else
        {
            jumped = false;
        }
    }

    public virtual void Reload()
    {
        if (inventory != null && inventory.FPSEquipment != null)
        {
            if (inventory.FPSEquipment.Reload())
                animator.SetTrigger("Reload");
        }
    }

    public void Sit()
    {
        if (!Motor.grounded)
            return;

        MovementIndex += 1;
        if (MovementIndex > 2)
            MovementIndex = 0;
        CmdChangeState((byte)(MovementIndex));
    }

    public void ChangeState(byte state)
    {
        CmdChangeState(state);
    }

    [Command(channel = 0)]
    private void CmdChangeState(byte state)
    {
        RpcChangeState(state);
    }

    [ClientRpc(channel = 0)]
    void RpcChangeState(byte state)
    {
        MovementIndex = state;
    }

    float sidevel = 0;
    public void MoveAnimation()
    {
        if (this.transform.parent == null)
        {
            if (Motor != null && Motor.grounded || Motor == null)
            {
                float fw = Vector3.Dot(MoveVelocity, this.transform.forward);
                float si = Vector3.Dot(MoveVelocity, this.transform.right);

                if (fw < 1 && fw > -1)
                    fw = 1;
                if (fw < 1 && fw < 0)
                    fw = -1;

                float velocity = MoveVelocity.magnitude * fw;
                sidevel = Mathf.Lerp(sidevel, si, 20 * Time.deltaTime);
                PlayMoveAnimation(velocity, sidevel);
            }
        }
        else
        {
            MoveVelocity = Vector3.zero;
            PlayMoveAnimation(0, 0);
        }
    }

    public float GetCurrentMoveSpeed()
    {
        float speed = CurrentMoveSpeed;
        float speedMax = CurrentMoveSpeedMax;

        if (MovementIndex >= MovementPreset.Length)
            MovementIndex = 0;

        if (MovementPreset.Length > MovementIndex)
        {
            speed = MovementPreset[MovementIndex].MoveSpeed;
            speedMax = MovementPreset[MovementIndex].MoveSpeedMax;
            animator.SetLayerWeight(1, MovementPreset[MovementIndex].UpperWeightBlend);
        }

        if (Sprint)
            speed = speedMax;

        CurrentMoveSpeedMax = speedMax;
        CurrentMoveSpeed = speed;

        return speed;
    }

    public void MoveTo(Vector3 dir)
    {
        float speed = GetCurrentMoveSpeed();
        Move(dir * speed * spdMovAtkMult);
    }

    public void MoveToPosition(Vector3 position)
    {
        float speed = GetCurrentMoveSpeed();

        Vector3 direction = (position - transform.position);
        direction = Vector3.ClampMagnitude(direction, 1);
        direction.y = 0;
        Move(direction.normalized * speed * direction.magnitude * spdMovAtkMult);
        if (direction != Vector3.zero)
        {
            Quaternion newrotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, newrotation, Time.deltaTime * direction.magnitude);
        }
    }

    public void OnTransformSyn()
    {
        if (rotationYSyncTmp != rotationYSync || positionSyncTmp != positionSync)
        {
            positionLate = positionLastTrip;
            latencyTime = Time.time - timeLastTrip;
            timeLastTrip = Time.time;

            positionLastTrip = positionSync;
            positionInterpolate = this.transform.position;
            rotationInterpolate = this.transform.rotation;

            positionSyncTmp = positionSync;
            rotationYSyncTmp = rotationYSync;
        }
    }

    [Command(channel = 1)]
    public void CmdUpdateTransform(Vector3 position, float rotationY)
    {
        positionSync = position;
        rotationYSync = rotationY;
    }

    public void UpdatePosition()
    {
        if (Motor != null)
        {
            Motor.MovementIndex = MovementIndex;
            Motor.isEnabled = (IsMine || isServerControl);
        }
        OnTransformSyn();
        // calculate delay of sending
        float fps = (1 / Time.deltaTime);
        float delay = (fps / currentSendingRate) * Time.deltaTime;
        if (Time.time > timeTmpsending + delay)
        {
            // sending translation data.
            if (IsAlive)
            {
                if (IsMine)
                {
                    // local player sending position and rotation
                    CmdUpdateTransform(this.transform.position, this.transform.rotation.eulerAngles.y);
                }
                else
                {
                    if (isServer && NetID == -1)
                    {
                        // NetID -1 = none player, none player send position and rotation
                        CmdUpdateTransform(this.transform.position, this.transform.rotation.eulerAngles.y);
                    }
                }
            }
            timeTmpsending = Time.time;
        }
        // interolate translation for none control object
        if ((!IsMine && NetID != -1) || (!isServer && NetID == -1))
        {
            float lerpValue = (Time.time - timeLastTrip) / latencyTime;
            if (this.transform.parent == null)
            {
                positionInterpolate = Vector3.Lerp(positionLate, positionSync, lerpValue);
                this.transform.position = Vector3.Lerp(this.transform.position, positionInterpolate, 0.6f);
            }
            Quaternion rotationTarget = this.transform.rotation;
            rotationTarget.eulerAngles = new Vector3(rotationTarget.eulerAngles.x, rotationYSync, rotationTarget.eulerAngles.z);
            rotationInterpolate = Quaternion.Lerp(this.transform.rotation, rotationTarget, lerpValue);
            this.transform.rotation = rotationInterpolate;
        }

        if (Motor != null && Motor.isEnabled)
        {
            MoveVelocity = Motor.movement.velocity;
        }
        else
        {
            // calculate movement velocity for none motor control
            Vector3 velocityTarget = (this.transform.position - previousPosition) / Time.deltaTime;
            previousPosition = this.transform.position;
            velocityTarget.y = 0;
            MoveVelocity = Vector3.Lerp(MoveVelocity, velocityTarget, 30 * Time.deltaTime);
        }
        MoveAnimation();
    }

    public void AttackAnimation(int attacktype)
    {
        AttackType = attacktype;
        CmdAttackAnimation((byte)attacktype);
    }

    public void AttackAnimation()
    {
        CmdAttackAnimation((byte)AttackType);
    }

    [ClientRpc(channel = 0)]
    private void RpcAttackAnimation(byte attacktype)
    {
        PlayAttackAnimation(true, attacktype);
    }

    [Command(channel = 0)]
    private void CmdAttackAnimation(byte attacktype)
    {
        RpcAttackAnimation(attacktype);
    }

    public void AttackTo(Vector3 direction, byte attacktype)
    {
        CmdattackTo(direction, attacktype);
    }

    [Command(channel = 0)]
    private void CmdattackTo(Vector3 direction, byte attacktype)
    {
        PlayAttackAnimation(true, attacktype);
    }

    [Command(channel = 0)]
    public void CmddoDamage(Vector3 origin, Vector3 direction, byte num, byte spread, byte seed, byte damage, float distance, byte penetrate, int id, byte team)
    {
        RpcdoDamage(origin, direction, num, spread, seed, damage, distance, penetrate, id, team);
    }

    [ClientRpc(channel = 0)]
    public void RpcdoDamage(Vector3 origin, Vector3 direction, byte num, byte spread, byte seed, byte damage, float distance, byte penetrate, int id, byte team)
    {
        doDamage(origin, direction, num, spread, seed, damage, distance, penetrate, id, team);
    }

    [Command(channel = 0)]
    public void CmddoDamagebyItemIndex(Vector3 origin, Vector3 direction, int itemid, byte seed)
    {
        RpcdoDamagebyItemIndex(origin, direction, itemid, seed);
    }

    [ClientRpc(channel = 0)]
    public void RpcdoDamagebyItemIndex(Vector3 origin, Vector3 direction, int itemid, byte seed)
    {
        FPSItemEquipment item = UnitZ.itemManager.GetFPSitem(itemid);
        if (item != null)
        {
            FPSWeaponEquipment weapon = item.GetComponent<FPSWeaponEquipment>();
            if (weapon != null)
            {
                doDamage(origin, direction, weapon.BulletNum, weapon.Spread, seed, weapon.Damage, weapon.Distance, weapon.MaxPenetrate, NetID, Team);
            }
        }
    }

    public void doDamage(Vector3 origin, Vector3 direction, byte num, byte spread, byte seed, byte damage, float distance, byte penetrate, int id, byte team)
    {
        if (rayActive)
        {
            if (rayActive.ShootRay(origin, direction, num, spread, seed, damage, distance, penetrate, id, team))
            {
                PlayDamageSound();
            }
        }
        if (inventory && !IsMine)
        {
            inventory.EquipmentOnAction(direction, num, spread, seed);
        }
    }

    public void DoDamage(Vector3 origin, Vector3 direction, byte num, byte spread, byte seed, byte damage, float distance, byte penetrate, int id, byte team)
    {
        CmddoDamage(origin, direction, num, spread, seed, damage, distance, penetrate, id, team);
    }

    public void DoDamage()
    {
        CmddoDamage(this.transform.position + DamageOffset, this.transform.forward, 1, 0, 1, Damage, DamageLength, Penetrate, NetID, Team);
    }

    public void DoDamageByItemIndex(Vector3 origin, Vector3 direction, int itemid, byte seed)
    {
        CmddoDamagebyItemIndex(origin, direction, itemid, seed);
    }

    [Command(channel = 0)]
    public void CmddoOverlapDamage(Vector3 origin, Vector3 direction, byte damage, float distance, float dot, int id, byte team)
    {
        RpcdoOverlapDamage(origin, direction, damage, distance, dot, id, team);
    }

    [ClientRpc(channel = 0)]
    public void RpcdoOverlapDamage(Vector3 origin, Vector3 direction, byte damage, float distance, float dot, int id, byte team)
    {
        doOverlapDamage(origin, direction, damage, distance, dot, id, team);
    }

    public void doOverlapDamage(Vector3 origin, Vector3 direction, byte damage, float distance, float dot, int id, byte team)
    {
        if (rayActive)
        {
            if (rayActive.Overlap(origin, direction, damage, distance, dot, id, team))
                PlayDamageSound();
        }

        if (inventory && !IsMine)
        {
            inventory.EquipmentOnAction(direction, 1, 0, 0);
        }
    }

    public void DoOverlapDamage(Vector3 origin, Vector3 direction, byte damage, float distance, float dot, int id, byte team)
    {
        if (rayActive)
        {
            if (rayActive.OverlapTest(origin, direction, damage, distance, dot, id, team))
                PlayDamageSound();
        }
        CmddoOverlapDamage(origin, direction, damage, distance, dot, id, team);
    }

    public void Checking(Vector3 origin, Vector3 direction)
    {
        if (rayActive)
        {
            rayActive.CheckingRay(origin, direction);
        }
    }

    public void Interactive(GameObject target)
    {
        NetworkIdentity objectnet = target.GetComponent<NetworkIdentity>();
        if (objectnet != null)
            CmdDirectObjectInteractive(objectnet.netId);
    }

    public void Interactive(Vector3 origin, Vector3 direction)
    {
        if (isLocalPlayer)
        {
            CmdInteractive(origin, direction);

            if (rayActive)
            {
                rayActive.ActiveLocalRay(origin, direction);
            }
        }
    }

    public void PickupItemCallback(ItemData item)
    {
        TargetReciveItem(connectionToClient, item.ItemID, item.NumTag, item.Quantity);
    }

    [TargetRpc]
    public void TargetReciveItem(NetworkConnection target, string itemid, int numtag, int num)
    {
        ItemData item = UnitZ.itemManager.GetItemDataByID(itemid);
        if (inventory != null && item != null)
        {
            if (inventory.AddItemTest(item, num))
            {
                if (inventory.stickerTarget != null)
                {
                    if (!inventory.DropItemBySticker(inventory.stickerTarget))
                    {
                        //Debug.Log("stick is empty");
                        inventory.DropItemBySameEquipType(item);
                    }
                    ItemCollector lastItemPicked = inventory.AddItemByItemData(item, num, numtag, inventory.stickerTarget.Index);
                    if (lastItemPicked != null)
                    {
                        //Debug.Log("Equip from ground " + lastItemPicked.Item.ItemName);
                        inventory.EquipItemToStickerByCollector(lastItemPicked, inventory.stickerTarget);
                        inventory.OnViewChanged();
                    }
                    inventory.stickerTarget = null;
                }
                else
                {
                    inventory.DropItemBySameEquipType(item);
                    inventory.AddItemByItemData(item, num, numtag, -1);
                }

                if (item.SoundPickup)
                {
                    AudioSource.PlayClipAtPoint(item.SoundPickup, this.transform.position);
                }
            }
        }
    }

    public void PickupItemBackpackCallback(ItemBackpack item)
    {
        TargetReciveItemBackpack(connectionToClient, item.SyncItemdata);
    }

    [TargetRpc]
    public void TargetReciveItemBackpack(NetworkConnection target, string itemdata)
    {

        if (inventory != null && itemdata != "")
        {
            inventory.AddItemFromText(itemdata);
        }
    }

    public void PickupStockCallback(ItemStocker stocker)
    {
        inventory.PeerTrade = stocker.inventory;
        stocker.inventory.PeerTrade = this.inventory;
        TargetReciveStock(connectionToClient, stocker.netId);
    }

    [TargetRpc]
    public void TargetReciveStock(NetworkConnection target, NetworkInstanceId objectid)
    {
        GameObject obj = ClientScene.FindLocalObject(objectid);
        if (obj)
        {
            ItemStocker itemstock = obj.GetComponent<ItemStocker>();
            if (itemstock)
            {
                itemstock.inventory.PeerTrade = this.inventory;
                inventory.PeerTrade = itemstock.inventory;
                itemstock.PickUpStock(this.gameObject);
            }
        }
    }
    [Command(channel = 0)]
    private void CmdDirectObjectInteractive(NetworkInstanceId objectid)
    {
        GameObject obj = ClientScene.FindLocalObject(objectid);
        if (obj)
        {
            obj.SendMessage("Pickup", this.gameObject, SendMessageOptions.DontRequireReceiver);
        }
    }

    [Command(channel = 0)]
    private void CmdInteractive(Vector3 origin, Vector3 direction)
    {
        if (rayActive)
        {
            rayActive.ActiveRay(origin, direction);
        }
    }

    public void Move(Vector3 directionVector)
    {
        if (Motor && Motor.isActiveAndEnabled)
        {
            Motor.inputMoveDirection = directionVector;
        }
    }

    public void PlayIdleSound()
    {
        if (Audiosource && SoundIdle.Length > 0)
        {
            Audiosource.PlayOneShot(SoundIdle[Random.Range(0, SoundIdle.Length)]);
        }
    }

    public void PlayDamageSound()
    {
        if (Audiosource && DamageSound.Length > 0)
        {
            Audiosource.PlayOneShot(DamageSound[Random.Range(0, DamageSound.Length)]);
        }
    }

    public void RemoveCharacterData()
    {
        if (isServer)
        {
            if (UnitZ.playerSave)
            {
                UnitZ.playerSave.DeleteSave(UserID, CharacterKey, UserName);
            }
        }
    }

    public void SaveCharacterData(bool saveToHost)
    {
        string savedata = UnitZ.playerSave.GetPlayerSaveToText(this);
        //Debug.Log ("Save data");
        if (saveToHost)
        {
            CmdSaveCharacterData(savedata);
        }
        else
        {
            if (UnitZ.playerSave)
            {
                UnitZ.playerSave.SaveToLocal(savedata);
            }
        }
    }

    [Command(channel = 0)]
    void CmdSaveCharacterData(string savedata)
    {
        if (UnitZ.playerSave)
        {
            UnitZ.playerSave.SaveToServer(savedata);
        }
    }

    public void LoadCharacterData(bool saveToHost)
    {
        string hasKey = UserID + "_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + CharacterKey + "_" + UserName;
        if (isLocalPlayer)
        {
            if (saveToHost)
            {
                CmdGetSaveData(hasKey);
            }
            else
            {
                if (UnitZ.playerSave)
                {
                    string data = UnitZ.playerSave.GetDataFromLocal(hasKey);
                    UnitZ.playerSave.ReceiveDataAndApply(data, this);
                }
            }
        }
    }

    [Command(channel = 0)]
    void CmdGetSaveData(string hasKey)
    {
        if (UserID == "")
            return;

        //Debug.Log ("Load from server " + hasKey);
        string data = UnitZ.playerSave.GetDataFromServer(hasKey);
        TargetGetData(connectionToClient, data);

    }

    [TargetRpc(channel = 0)]
    public void TargetGetData(NetworkConnection target, string data)
    {

        if (UnitZ.playerSave)
        {
            //Debug.Log("recived data "+data);
            UnitZ.playerSave.ReceiveDataAndApply(data, this);
        }
    }

    [Command(channel = 0)]
    public void CmdRequestSpawnObject(Vector3 position, Quaternion rotation, string itemID, string itemData)
    {

        ItemData itemdata = UnitZ.itemManager.GetItemDataByID(itemID);
        if (itemdata)
        {
            if (itemdata.ItemFPS)
            {
                FPSItemPlacing fpsplacer = itemdata.ItemFPS.GetComponent<FPSItemPlacing>();
                if (fpsplacer)
                {
                    if (fpsplacer.Item)
                    {
                        GameObject obj = UnitZ.gameNetwork.RequestSpawnObject(fpsplacer.Item, position, rotation);
                        if (obj)
                        {
                            ObjectPlacing objplaced = obj.GetComponent<ObjectPlacing>();
                            objplaced.SetItemUID(objplaced.GetUniqueID());
                            objplaced.SetItemID(itemID);
                            objplaced.SetItemData(itemData);
                        }
                    }
                }
            }
        }
    }

    [Command(channel = 0)]
    public void CmdRequestThrowObject(Vector3 position, Quaternion rotation, string itemID, Vector3 force)
    {
        ItemData itemdata = UnitZ.itemManager.GetItemDataByID(itemID);
        if (itemdata)
        {
            if (itemdata.ItemFPS)
            {
                FPSItemThrow fpsthrow = itemdata.ItemFPS.GetComponent<FPSItemThrow>();
                if (fpsthrow)
                {
                    if (fpsthrow.Item)
                    {
                        GameObject obj = UnitZ.gameNetwork.RequestSpawnObject(fpsthrow.Item, position, rotation);
                        if (obj)
                        {
                            DamageBase dm = obj.GetComponent<DamageBase>();
                            if (dm)
                            {
                                dm.OwnerID = NetID;
                                dm.OwnerTeam = Team;
                            }
                            if (obj.GetComponent<Rigidbody>())
                                obj.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
                        }
                    }
                }
            }
        }
    }

    [Command(channel = 0)]
    public void CmdSendMessage(string text)
    {
        RpcGotMessage(text);
    }

    [ClientRpc(channel = 0)]
    void RpcGotMessage(string text)
    {
        if (UnitZ.NetworkGameplay)
            UnitZ.NetworkGameplay.chatLog.AddLog(text);
    }

    [Command]
    public void CmdOnSpawned(Vector3 position)
    {
        this.transform.position = position;
        positionSync = position;
        rotationYSync = this.transform.rotation.eulerAngles.y;
        positionSyncTmp = positionSync;
        rotationYSyncTmp = rotationYSync;
        positionLate = positionSync;
        positionLastTrip = positionSync;
        RpcOnSpawned(position);
    }
    [ClientRpc]
    void RpcOnSpawned(Vector3 position)
    {
        this.transform.position = position;
        positionSync = position;
        rotationYSync = this.transform.rotation.eulerAngles.y;
        positionSyncTmp = positionSync;
        rotationYSyncTmp = rotationYSync;
        positionLate = positionSync;
        positionLastTrip = positionSync;
    }
}

[System.Serializable]
public class BodyStatePreset
{
    public float MoveSpeed = 3;
    public float MoveSpeedMax = 5f;
    public float FPSCameraMinY = -90;
    public float FPSCameraMaxY = 90;
    public float UpperChestOffset = -90;
    public float UpperWeightBlend = 1;

}
