//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

/// <summary>
/// AI character controller.
/// Just A basic AI Character controller 
/// will looking for a Target and moving to and Attacking
/// </summary>

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterSystem))]
public class AICharacterControllerNav : NetworkBehaviour
{
    [Header("Properties")]
    public string[] TargetTag = { "Player" };
    public float AIUpdateRate = 8;
    public GameObject ObjectTarget;
    [HideInInspector]
    public Vector3 PositionTarget;
    [HideInInspector]
    public CharacterSystem character;
    [HideInInspector]
    public float DistanceAttack = 2;
    [Header("Attacking")]
    public float DistanceMoveTo = 20;
    public float TurnSpeed = 10.0f;
    public bool BrutalMode;
    public bool RushMode;
    public float PatrolRange = 10;
    [HideInInspector]
    public Vector3 positionTemp;
    [HideInInspector]
    public int aiTime = 0;
    [HideInInspector]
    public int aiState = 0;
    private float attackTemp = 0;
    public float AttackDelay = 0.5f;
    [Header("Sound")]
    public float IdleSoundDelay = 10;
    private float soundTime, soundTimeDuration;
    private AIManager AImange;
    private NavMeshAgent navAgent;
    private Vector3 targetDirectiom;
    private float timeTmp;


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
    }

    public void Attack(Vector3 targetDirectiom)
    {
        if (Time.time > attackTemp + AttackDelay)
        {
            character.DoDamage();
            character.AttackAnimation();
            attackTemp = Time.time;
        }
    }

    public void AIDoAttack()
    {
        if (Time.time > attackTemp + AttackDelay)
        {
            character.DoDamage();
            character.AttackAnimation();
            attackTemp = Time.time;
        }
    }


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
        if (isServer && character.IsAlive)
        {

            float fps = (1 / Time.deltaTime);
            float delay = (fps / AIUpdateRate) * Time.deltaTime; // calculate delay of sending 

            if (Time.time > timeTmp + delay)
            { // sending by time
                timeTmp = Time.time;
                character.isServerControl = true;
                // get attack distance from primary weapon.
                DistanceAttack = character.PrimaryWeaponDistance;

                float distance = Vector3.Distance(PositionTarget, this.gameObject.transform.position);
                targetDirectiom = (PositionTarget - this.transform.position);

                Quaternion targetRotation = this.transform.rotation;
                float str = TurnSpeed * Time.time;
                // rotation to look at a target
                if (targetDirectiom != Vector3.zero)
                {
                    targetRotation = Quaternion.LookRotation(targetDirectiom);
                    targetRotation.x = 0;
                    targetRotation.z = 0;
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, str);
                }

                // if Target is exist
                if (ObjectTarget != null)
                {
                    DamageManager targetdamagemanager = ObjectTarget.GetComponent<DamageManager>();
                    PositionTarget = ObjectTarget.transform.position;
                    if (aiTime <= 0)
                    {
                        aiState = Random.Range(0, 4);
                        aiTime = Random.Range(10, 100);
                    }
                    else
                    {
                        aiTime--;
                    }

                    // attack in distance.
                    if (distance <= DistanceAttack)
                    {
                        if (aiState == 0 || BrutalMode)
                        {
                            Attack(targetDirectiom);
                        }
                    }
                    else
                    {
                        if (distance <= DistanceMoveTo)
                        {
                            // rotation facing to a target.
                            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, str);
                        }
                        else
                        {
                            // if target is out of distance
                            ObjectTarget = null;
                            if (aiState == 0)
                            {
                                aiState = 1;
                                aiTime = Random.Range(10, 500);
                                PositionTarget = positionTemp + new Vector3(Random.Range(-PatrolRange, PatrolRange), 0, Random.Range(-PatrolRange, PatrolRange));
                            }
                        }
                    }
                    if (targetdamagemanager && !targetdamagemanager.IsAlive)
                        ObjectTarget = null;

                }
                else
                {
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
                                    DamageManager targetdamagemanager = targets[i].GetComponent<DamageManager>();
                                    if (targetdamagemanager != null && targetdamagemanager.IsAlive)
                                    {
                                        float distancetargets = Vector3.Distance(targets[i].gameObject.transform.position, this.gameObject.transform.position);
                                        if ((distancetargets <= length && (distancetargets <= DistanceMoveTo || distancetargets <= DistanceAttack || RushMode)) && ObjectTarget != targets[i].gameObject)
                                        {
                                            length = distancetargets;
                                            ObjectTarget = targets[i].gameObject;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (aiState == 0)
                    {
                        // AI state == 0 mean AI is free, so moving to anywhere
                        aiState = 1;
                        aiTime = Random.Range(10, 200);
                        PositionTarget = positionTemp + new Vector3(Random.Range(-PatrolRange, PatrolRange), 0, Random.Range(-PatrolRange, PatrolRange));
                    }
                    if (aiTime <= 0)
                    {
                        // random AI state
                        aiState = Random.Range(0, 4);
                        aiTime = Random.Range(10, 200);
                    }
                    else
                    {
                        aiTime--;
                    }
                }
            }
            if (navAgent)
            {
                navAgent.SetDestination(PositionTarget);
                navAgent.speed = character.GetCurrentMoveSpeed();
            }
        }
    }
}
