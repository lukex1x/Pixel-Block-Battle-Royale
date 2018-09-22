//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterSystem))]
public class AICharacterShooterNav : NetworkBehaviour
{
    [Header("Properties")]
    public string[] TargetTag = { "Player" };
    public bool LockTarget = true;
    public float AIUpdateRate = 8;
    public float SyncDistance = 10;
    [Range(0,1)]
    public float Accuracy = 0.5f;
    public float CombatTime = 10;
    public float IdleTime = 30;
    public GameObject ObjectTarget;
    [HideInInspector]
    public Vector3 PositionTarget, PositionMove;
    [HideInInspector]
    public CharacterSystem character;
    public float DistanceAttack = 300;
    public float DistanceKill = 10;
    [HideInInspector]
    public GameObject ObjectiveTarget;

    [Header("Attacking")]
    public float DistanceMoveTo = 20;
    public float TurnSpeed = 10.0f;
    public bool Fighting = true;
    public bool RushMode;
    public float PatrolRange = 10;
    [HideInInspector]
    public Vector3 positionTemp;
    [HideInInspector]
    public float aiTime = 0;
    [HideInInspector]
    public int aiState = 0;
    private float attackTemp = 0;
    public float AttackDelay = 0.5f;
    public float SprayTimeMax = 10;
    public float TimeLoseTarget = 10;
    [Header("Sound")]
    public float IdleSoundDelay = 10;
    private float soundTime, soundTimeDuration;
    private AIManager AImange;
    private NavMeshAgent navAgent;
    private Vector3 targetDirection;
    private float timeTmp;
    public Transform Pointer;

    void Start()
    {
        character = this.gameObject.GetComponent<CharacterSystem>();
        navAgent = this.gameObject.GetComponent<NavMeshAgent>();
        navAgent.avoidancePriority = Random.Range(0, 100);
        positionTemp = this.transform.position;
        aiState = 0;
        attackTemp = Time.time;
        soundTime = Time.time;
        soundTimeDuration = Random.Range(0, IdleSoundDelay);
        character.NetID = -1;
        character.isServerControl = true;
    }

    private float sprayTime = 0;
    private bool isShooting = false;
    private float timeSprayTmp = 0;
    private float currentDistanceWeapon = 0;

    public void Attack()
    {
        if (!isShooting)
        {
            sprayTime = Random.Range(1, SprayTimeMax);
            timeSprayTmp = Time.time;
            isShooting = true;
        }
    }

    private void attacking()
    {
        if (isShooting)
        {
            if (Time.time > attackTemp + AttackDelay)
            {
                if (Time.time < timeSprayTmp + sprayTime)
                {
                    character.inventory.FPSEquipment.Trigger();
                }
                else
                {
                    attackTemp = Time.time;
                    isShooting = false;
                }
            }
            character.inventory.FPSEquipment.OnTriggerRelease();
        }
    }

    bool canSeeTarget(Vector3 pointer, GameObject target)
    {
        Vector3 dir = ((target.transform.position + (Vector3.up * Random.Range(0, 100) / 100.0f)) - pointer).normalized;
        RaycastHit targetinfo;
        if (Physics.Raycast(pointer, dir, out targetinfo))
        {
            if (targetinfo.collider.transform.root != this.gameObject.transform.root)
            {
                if (targetinfo.collider.transform.root == target.transform.root)
                {
                    Debug.DrawLine(pointer, target.transform.position, Color.red);
                    return true;
                }
            }
        }
        return false;
    }

    Vector3 getRandomGo()
    {
        if (UnitZ.NetworkGameplay)
        {
            Vector3 pos = positionTemp + new Vector3(Random.Range(-PatrolRange, PatrolRange), 0, Random.Range(-PatrolRange, PatrolRange));
            BattleRoyaleGamePlay gameplay = UnitZ.NetworkGameplay.GetComponent<BattleRoyaleGamePlay>();
            if (gameplay != null)
            {
                Vector2 safeArea = (Random.insideUnitCircle * gameplay.Radius);
                pos = gameplay.CentreArea + new Vector3(safeArea.x, 0, safeArea.y);
                RaycastHit hit;
                if (Physics.Raycast(pos + (Vector3.up * 100), -Vector3.up, out hit))
                {
                    pos = hit.point;
                }
            }
            else
            {
                if (ObjectiveTarget)
                {
                    pos = ObjectiveTarget.transform.position + new Vector3(Random.Range(-PatrolRange, PatrolRange), 0, Random.Range(-PatrolRange, PatrolRange));
                }
                else
                {
                    pos = positionTemp + new Vector3(Random.Range(-PatrolRange, PatrolRange), 0, Random.Range(-PatrolRange, PatrolRange));
                }
                
            }
            positionMoveTmp = pos;
            return pos;
        }
        else
        {
            Vector3 pos = positionTemp + new Vector3(Random.Range(-PatrolRange, PatrolRange), 0, Random.Range(-PatrolRange, PatrolRange));
            if (ObjectiveTarget)
                pos = ObjectiveTarget.transform.position + new Vector3(Random.Range(-PatrolRange, PatrolRange), 0, Random.Range(-PatrolRange, PatrolRange));

            positionMoveTmp = pos;
            return pos;
        }
    }

    GameObject findTarget()
    {
        GameObject gettarget = null;
        float length = float.MaxValue;
        for (int t = 0; t < TargetTag.Length; t++)
        {
            // Finding all the targets by Tags.
            TargetCollector targetget = UnitZ.aiManager.FindTargetTag(TargetTag[t]);
            if (targetget != null)
            {
                GameObject[] targets = targetget.Targets;
                if (targets != null && targets.Length > 0)
                {
                    for (int i = 0; i < targets.Length; i++)
                    {
                        if (targets[i] != null && targets[i].transform.root != this.transform.root)
                        {
                            DamageManager targetdamagemanager = targets[i].GetComponent<DamageManager>();
                            // if target has a damage manager and alive
                            if (targetdamagemanager != null && targetdamagemanager.IsAlive && targetdamagemanager.Team != character.Team || targetdamagemanager.Team == 0)
                            {
                                float distancetargets = Vector3.Distance(targets[i].gameObject.transform.position, this.gameObject.transform.position);
                                if ((distancetargets <= length && (distancetargets <= DistanceMoveTo || distancetargets <= currentDistanceWeapon)))
                                {
                                    // get a target only in distance or rush mode enabled
                                    if (canSeeTarget(Pointer.transform.position, targets[i].gameObject) || RushMode)
                                    {
                                        length = distancetargets;
                                        gettarget = targets[i].gameObject;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return gettarget;
    }

    float timeTmpLoseTarget;
    Vector3 positionMoveTmp;
    void Update()
    {
        if (character == null)
            return;

        // random play an idle sound.
        if (Time.time > soundTime + soundTimeDuration)
        {
            character.PlayIdleSound();
            soundTimeDuration = Random.Range(0, IdleSoundDelay);
            soundTime = Time.time;
        }

        if (!isServer || !character.IsAlive)
            return;

        float fps = (1 / Time.deltaTime);
        float delay = (fps / AIUpdateRate) * Time.deltaTime;

        currentDistanceWeapon = character.PrimaryWeaponDistance;
        if (currentDistanceWeapon > DistanceAttack)
            currentDistanceWeapon = DistanceAttack;

        if (Time.time > timeTmp + delay)
        {
            // update by time
            timeTmp = Time.time;
            float distance = Vector3.Distance(PositionTarget, this.gameObject.transform.position);

            // if Target is exist
            if (ObjectTarget != null)
            {
                DamageManager targetdamagemanager = ObjectTarget.GetComponent<DamageManager>();
                PositionTarget = ObjectTarget.transform.position;
                Debug.DrawLine(this.transform.position, PositionTarget, Color.blue);

                Vector3 noise = new Vector3(Random.Range(-100,100) / 100.0f, Random.Range(-100, 100) / 100.0f, Random.Range(-100, 100) / 100.0f) * (Accuracy - 1);
                if (Pointer != null)
                    Pointer.forward = ((PositionTarget + noise + (Vector3.up * Random.Range(0, 100) / 100.0f)) - Pointer.position).normalized;

                if (aiTime <= 0)
                {
                    aiState = Random.Range(0, 4);
                    aiTime = Random.Range(1, CombatTime);

                    if (distance < DistanceKill)
                    {
                        // if target distance is closer than Distanc Kill, AI can only stand attack or move attack
                        aiState = Random.Range(0, 2);
                    }

                    if (aiState == 0)
                    {
                        // stand
                        PositionMove = this.transform.position;
                    }

                    if (aiState == 1 || aiState == 2)
                    {
                        // move to safe zone
                        PositionMove = getRandomGo();
                    }
                }
                else
                {
                    aiTime -= (1 / AIUpdateRate);
                }

                // attack in distance.
                if (distance <= currentDistanceWeapon)
                {
                    // if see a target AI can attack
                    if (canSeeTarget(Pointer.transform.position, ObjectTarget))
                    {
                        if (aiState == 0)
                        {
                            // stand and attack
                            Attack();
                            PositionMove = this.transform.position;
                            targetDirection = (PositionTarget - this.transform.position);
                        }
                        if (aiState == 1)
                        {
                            // move and attack
                            Attack();
                            PositionMove = positionMoveTmp;
                            targetDirection = (PositionTarget - this.transform.position);
                        }

                        if (aiState == 2)
                        {
                            isShooting = false;
                            // run to safe zone then attack later
                            PositionMove = positionMoveTmp;
                            targetDirection = navAgent.desiredVelocity.normalized;
                        }
                        timeTmpLoseTarget = Time.time;
                    }
                    else
                    {
                        if (aiState != 2)
                        {
                            // if cannot see a target, AI will go to finding a target
                            PositionMove = PositionTarget;
                        }

                        targetDirection = navAgent.desiredVelocity.normalized;
                        isShooting = false;

                        // if cannot see a target for a while, AI lose Target
                        if (Time.time >= timeTmpLoseTarget + TimeLoseTarget)
                        {
                            ObjectTarget = null;
                            return;
                        }
                    }
                }
                else
                {
                    // if target distance is closer than move to distance AI can move to a target
                    if (distance <= DistanceMoveTo)
                    {
                        if (aiState == 0)
                        {
                            // stand and wait
                            //Attack();
                            PositionMove = this.transform.position;
                            targetDirection = (PositionTarget - this.transform.position);
                        }
                        if (aiState == 1)
                        {
                            // move to target
                            PositionMove = PositionTarget;
                            targetDirection = (PositionTarget - this.transform.position);
                        }

                        if (aiState == 2)
                        {
                            // run to safe zone
                            PositionMove = positionMoveTmp;
                            targetDirection = navAgent.desiredVelocity.normalized;
                        }
                    }
                    else
                    {
                        // if target is out of distance, lose target
                        ObjectTarget = null;
                        aiState = 0;
                    }
                }
                if (targetdamagemanager && !targetdamagemanager.IsAlive)
                {
                    ObjectTarget = null;
                    aiState = 0;
                }

                SetDestination(PositionMove);
            }
            else
            {
                isShooting = false;
                // AI can seek and attack a target when Fighting is Enabled
                if (Fighting)
                    ObjectTarget = findTarget();

                if (ObjectTarget != null)
                {
                    aiState = 0;
                    return;
                }

                if (aiState == 0)
                {
                    // AI state == 0 mean AI is free, so moving to anywhere
                    aiState = 1;
                    aiTime = Random.Range(1, IdleTime);
                    PositionTarget = getRandomGo();
                    PositionMove = PositionTarget;
                    SetDestination(PositionMove);
                }
                if (aiTime <= 0)
                {
                    // random AI state
                    aiState = 0;
                }
                else
                {
                    aiTime -= (1 / AIUpdateRate);
                }
                // faceing to a desier direction
                targetDirection = navAgent.desiredVelocity.normalized;
            }
            if (!LockTarget)
                findTarget();
        }

        attacking();

        Quaternion targetRotation = this.transform.rotation;
        // rotation to look at a target
        if (targetDirection != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(targetDirection);
            targetRotation.x = 0;
            targetRotation.z = 0;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, TurnSpeed * Time.deltaTime);
        }

        if (navAgent)
        {
            Debug.DrawLine(this.transform.position, PositionMove, Color.green);
            navAgent.speed = character.GetCurrentMoveSpeed();
        }
    }

    void SetDestination(Vector3 pos)
    {
        if (navAgent)
            navAgent.SetDestination(pos);
    }
}
